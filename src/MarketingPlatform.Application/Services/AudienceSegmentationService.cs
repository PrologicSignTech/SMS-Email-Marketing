using AutoMapper;
using MarketingPlatform.Application.DTOs.Audience;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class AudienceSegmentationService : IAudienceSegmentationService
    {
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroup> _groupRepository;
        private readonly IRepository<ContactGroupMember> _memberRepository;
        private readonly IRepository<ContactTagAssignment> _tagAssignmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AudienceSegmentationService> _logger;

        public AudienceSegmentationService(
            IRepository<Contact> contactRepository,
            IRepository<ContactGroup> groupRepository,
            IRepository<ContactGroupMember> memberRepository,
            IRepository<ContactTagAssignment> tagAssignmentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AudienceSegmentationService> logger)
        {
            _contactRepository = contactRepository;
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
            _tagAssignmentRepository = tagAssignmentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AudienceSegmentDto> EvaluateSegmentAsync(string userId, SegmentCriteriaDto criteria)
        {
            // Note: For large datasets (>10k contacts), consider implementing database-level filtering
            // or pagination to avoid memory issues
            var allContacts = await _contactRepository.FindAsync(c => 
                c.UserId == userId && !c.IsDeleted && c.IsActive);
            var contactsList = allContacts.ToList();

            // Pre-load all tag assignments for performance (avoid N+1 queries)
            var contactIds = contactsList.Select(c => c.Id).ToList();
            var allTagAssignments = await _tagAssignmentRepository.FindAsync(ta =>
                contactIds.Contains(ta.ContactId) && !ta.IsDeleted);
            var tagAssignmentsByContact = allTagAssignments
                .GroupBy(ta => ta.ContactId)
                .ToDictionary(g => g.Key, g => g.Select(ta => ta.ContactTagId).ToList());

            var matchedContactIds = new List<int>();

            foreach (var contact in contactsList)
            {
                if (await EvaluateContactAgainstCriteriaAsync(contact, criteria, tagAssignmentsByContact))
                {
                    matchedContactIds.Add(contact.Id);
                }
            }

            return new AudienceSegmentDto
            {
                TotalContacts = matchedContactIds.Count,
                ContactIds = matchedContactIds
            };
        }

        public async Task<int> CalculateAudienceSizeAsync(string userId, SegmentCriteriaDto criteria)
        {
            var segment = await EvaluateSegmentAsync(userId, criteria);
            return segment.TotalContacts;
        }

        public async Task<bool> UpdateDynamicGroupMembersAsync(string userId, int groupId)
        {
            var group = await _groupRepository.FirstOrDefaultAsync(g =>
                g.Id == groupId && g.UserId == userId && !g.IsDeleted && g.IsDynamic);

            if (group == null || string.IsNullOrEmpty(group.RuleCriteria))
                return false;

            try
            {
                // Parse rule criteria
                var criteria = JsonConvert.DeserializeObject<SegmentCriteriaDto>(group.RuleCriteria);
                if (criteria == null)
                    return false;

                // Evaluate segment
                var segment = await EvaluateSegmentAsync(userId, criteria);

                // Get current group members
                var currentMembers = await _memberRepository.FindAsync(m =>
                    m.ContactGroupId == groupId && !m.IsDeleted);
                var currentContactIds = currentMembers.Select(m => m.ContactId).ToHashSet();

                // Find contacts to add (in segment but not in group)
                var contactsToAdd = segment.ContactIds.Where(id => !currentContactIds.Contains(id)).ToList();

                // Find contacts to remove (in group but not in segment)
                var contactsToRemove = currentContactIds.Where(id => !segment.ContactIds.Contains(id)).ToList();

                // Add new members
                foreach (var contactId in contactsToAdd)
                {
                    var member = new ContactGroupMember
                    {
                        ContactId = contactId,
                        ContactGroupId = groupId,
                        JoinedAt = DateTime.UtcNow
                    };
                    await _memberRepository.AddAsync(member);
                }

                // Remove old members
                var membersToRemove = currentMembers.Where(m => contactsToRemove.Contains(m.ContactId));
                foreach (var member in membersToRemove)
                {
                    member.IsDeleted = true;
                    member.UpdatedAt = DateTime.UtcNow;
                }
                _memberRepository.UpdateRange(membersToRemove);

                // Update group contact count
                group.ContactCount = segment.TotalContacts;
                _groupRepository.Update(group);

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating dynamic group {groupId} members");
                return false;
            }
        }

        private async Task<bool> EvaluateContactAgainstCriteriaAsync(Contact contact, SegmentCriteriaDto criteria, Dictionary<int, List<int>> tagAssignmentsByContact)
        {
            if (criteria.Rules == null || !criteria.Rules.Any())
                return true; // No rules means all contacts match

            var results = new List<bool>();

            foreach (var rule in criteria.Rules)
            {
                var ruleResult = await EvaluateRuleAsync(contact, rule, tagAssignmentsByContact);
                results.Add(ruleResult);
            }

            // Apply logical operator
            if (criteria.LogicalOperator?.ToUpper() == "OR")
            {
                return results.Any(r => r); // Any rule matches
            }
            else
            {
                return results.All(r => r); // All rules must match (AND)
            }
        }

        private async Task<bool> EvaluateRuleAsync(Contact contact, SegmentRuleDto rule, Dictionary<int, List<int>> tagAssignmentsByContact)
        {
            var field = rule.Field?.ToLower() ?? "";
            var operatorType = rule.Operator?.ToLower() ?? "equals";
            var value = rule.Value ?? "";

            // Handle tag-based rules using pre-loaded assignments
            if (field == "tag")
            {
                var tagIds = tagAssignmentsByContact.TryGetValue(contact.Id, out var tags) ? tags : new List<int>();

                if (operatorType == "in" || operatorType == "contains")
                {
                    // Value should be comma-separated tag IDs
                    var targetTagIds = value.Split(',').Select(s => int.TryParse(s.Trim(), out var id) ? id : 0).Where(id => id > 0).ToList();
                    return tagIds.Any(id => targetTagIds.Contains(id));
                }
                return false;
            }

            // Handle custom attribute rules
            if (field.StartsWith("customattribute."))
            {
                var attributeKey = field.Substring("customattribute.".Length);
                var customAttrs = DeserializeCustomAttributes(contact.CustomAttributes);
                
                if (customAttrs != null && customAttrs.TryGetValue(attributeKey, out var attrValue))
                {
                    return EvaluateOperator(attrValue, operatorType, value);
                }
                return false;
            }

            // Handle standard contact fields
            var contactValue = field switch
            {
                "email" => contact.Email ?? "",
                "phonenumber" => contact.PhoneNumber,
                "firstname" => contact.FirstName ?? "",
                "lastname" => contact.LastName ?? "",
                "country" => contact.Country ?? "",
                "city" => contact.City ?? "",
                "postalcode" => contact.PostalCode ?? "",
                _ => ""
            };

            return EvaluateOperator(contactValue, operatorType, value);
        }

        private bool EvaluateOperator(string contactValue, string operatorType, string targetValue)
        {
            var contactLower = contactValue.ToLower();
            var targetLower = targetValue.ToLower();

            return operatorType switch
            {
                "equals" => contactLower == targetLower,
                "notequals" => contactLower != targetLower,
                "contains" => contactLower.Contains(targetLower),
                "notcontains" => !contactLower.Contains(targetLower),
                "startswith" => contactLower.StartsWith(targetLower),
                "endswith" => contactLower.EndsWith(targetLower),
                "in" => targetLower.Split(',').Select(s => s.Trim()).Contains(contactLower),
                "notin" => !targetLower.Split(',').Select(s => s.Trim()).Contains(contactLower),
                _ => false
            };
        }

        private Dictionary<string, string>? DeserializeCustomAttributes(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
