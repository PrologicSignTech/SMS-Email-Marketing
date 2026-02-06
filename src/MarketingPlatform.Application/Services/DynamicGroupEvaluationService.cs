using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace MarketingPlatform.Application.Services
{
    public class DynamicGroupEvaluationService : IDynamicGroupEvaluationService
    {
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactEngagement> _engagementRepository;
        private readonly IRepository<ContactTagAssignment> _tagAssignmentRepository;
        private readonly IRepository<ContactGroup> _groupRepository;
        private readonly IRepository<ContactGroupMember> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DynamicGroupEvaluationService> _logger;

        public DynamicGroupEvaluationService(
            IRepository<Contact> contactRepository,
            IRepository<ContactEngagement> engagementRepository,
            IRepository<ContactTagAssignment> tagAssignmentRepository,
            IRepository<ContactGroup> groupRepository,
            IRepository<ContactGroupMember> memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<DynamicGroupEvaluationService> logger)
        {
            _contactRepository = contactRepository;
            _engagementRepository = engagementRepository;
            _tagAssignmentRepository = tagAssignmentRepository;
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<int>> EvaluateGroupRulesAsync(string userId, GroupRuleCriteria criteria)
        {
            try
            {
                // Get all contacts for the user
                var contacts = await _contactRepository.FindAsync(c => 
                    c.UserId == userId && !c.IsDeleted && c.IsActive);

                if (!contacts.Any())
                    return new List<int>();

                // Pre-load engagement data and tag assignments to avoid N+1 queries
                var contactIds = contacts.Select(c => c.Id).ToList();
                var engagements = await _engagementRepository.FindAsync(e => 
                    contactIds.Contains(e.ContactId) && !e.IsDeleted);
                var tagAssignments = await _tagAssignmentRepository.FindAsync(ta => 
                    contactIds.Contains(ta.ContactId) && !ta.IsDeleted);

                // Create lookup dictionaries for faster access
                // Handle potential duplicates by taking the first engagement per contact
                var engagementLookup = engagements
                    .GroupBy(e => e.ContactId)
                    .ToDictionary(g => g.Key, g => g.First());
                var tagAssignmentLookup = tagAssignments
                    .GroupBy(ta => ta.ContactId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var matchingContactIds = new List<int>();

                foreach (var contact in contacts)
                {
                    if (await EvaluateContactAgainstRules(contact, criteria, engagementLookup, tagAssignmentLookup))
                    {
                        matchingContactIds.Add(contact.Id);
                    }
                }

                return matchingContactIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating group rules for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateDynamicGroupMembershipsAsync(string userId, int groupId)
        {
            try
            {
                // Get the dynamic group
                var group = await _groupRepository.FirstOrDefaultAsync(g => 
                    g.Id == groupId && g.UserId == userId && !g.IsDeleted && g.IsDynamic);

                if (group == null)
                {
                    _logger.LogWarning("Dynamic group {GroupId} not found for user {UserId}", groupId, userId);
                    return false;
                }

                if (string.IsNullOrEmpty(group.RuleCriteria))
                {
                    _logger.LogWarning("Dynamic group {GroupId} has no rule criteria", groupId);
                    return false;
                }

                // Parse the rule criteria
                var criteria = JsonConvert.DeserializeObject<GroupRuleCriteria>(group.RuleCriteria);
                if (criteria == null || !criteria.Rules.Any())
                {
                    _logger.LogWarning("Dynamic group {GroupId} has invalid or empty rule criteria", groupId);
                    return false;
                }

                // Evaluate rules to get matching contact IDs
                var matchingContactIds = await EvaluateGroupRulesAsync(userId, criteria);

                // Get current members
                var currentMembers = await _memberRepository.FindAsync(m => 
                    m.ContactGroupId == groupId && !m.IsDeleted);
                var currentContactIds = currentMembers.Select(m => m.ContactId).ToHashSet();

                // Determine contacts to add and remove
                var contactsToAdd = matchingContactIds.Where(id => !currentContactIds.Contains(id)).ToList();
                var contactsToRemove = currentContactIds.Where(id => !matchingContactIds.Contains(id)).ToList();

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

                // Remove members that no longer match
                var membersToRemove = currentMembers.Where(m => contactsToRemove.Contains(m.ContactId));
                foreach (var member in membersToRemove)
                {
                    member.IsDeleted = true;
                    member.UpdatedAt = DateTime.UtcNow;
                }
                _memberRepository.UpdateRange(membersToRemove);

                // Update contact count
                group.ContactCount = matchingContactIds.Count;
                group.UpdatedAt = DateTime.UtcNow;
                _groupRepository.Update(group);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated dynamic group {GroupId}: Added {AddedCount}, Removed {RemovedCount}, Total {TotalCount}",
                    groupId, contactsToAdd.Count, contactsToRemove.Count, matchingContactIds.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dynamic group {GroupId} for user {UserId}", groupId, userId);
                throw;
            }
        }

        public async Task UpdateAllDynamicGroupsAsync(string userId)
        {
            try
            {
                // Get all dynamic groups for the user
                var dynamicGroups = await _groupRepository.FindAsync(g => 
                    g.UserId == userId && !g.IsDeleted && g.IsDynamic);

                foreach (var group in dynamicGroups)
                {
                    await UpdateDynamicGroupMembershipsAsync(userId, group.Id);
                }

                _logger.LogInformation("Updated {Count} dynamic groups for user {UserId}", 
                    dynamicGroups.Count(), userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all dynamic groups for user {UserId}", userId);
                throw;
            }
        }

        private async Task<bool> EvaluateContactAgainstRules(
            Contact contact, 
            GroupRuleCriteria criteria,
            Dictionary<int, ContactEngagement> engagementLookup,
            Dictionary<int, List<ContactTagAssignment>> tagAssignmentLookup)
        {
            if (!criteria.Rules.Any())
                return false;

            var results = new List<bool>();

            foreach (var rule in criteria.Rules)
            {
                var result = await EvaluateRule(contact, rule, engagementLookup, tagAssignmentLookup);
                results.Add(result);
            }

            // Apply logic (AND or OR)
            return criteria.Logic == RuleLogic.And
                ? results.All(r => r)
                : results.Any(r => r);
        }

        private async Task<bool> EvaluateRule(
            Contact contact, 
            GroupRule rule,
            Dictionary<int, ContactEngagement> engagementLookup,
            Dictionary<int, List<ContactTagAssignment>> tagAssignmentLookup)
        {
            try
            {
                switch (rule.Field)
                {
                    case RuleField.Email:
                        return EvaluateStringField(contact.Email, rule.Operator, rule.Value);
                    
                    case RuleField.FirstName:
                        return EvaluateStringField(contact.FirstName, rule.Operator, rule.Value);
                    
                    case RuleField.LastName:
                        return EvaluateStringField(contact.LastName, rule.Operator, rule.Value);
                    
                    case RuleField.PhoneNumber:
                        return EvaluateStringField(contact.PhoneNumber, rule.Operator, rule.Value);
                    
                    case RuleField.Country:
                        return EvaluateStringField(contact.Country, rule.Operator, rule.Value);
                    
                    case RuleField.City:
                        return EvaluateStringField(contact.City, rule.Operator, rule.Value);
                    
                    case RuleField.PostalCode:
                        return EvaluateStringField(contact.PostalCode, rule.Operator, rule.Value);
                    
                    case RuleField.IsActive:
                        return EvaluateBooleanField(contact.IsActive, rule.Operator, rule.Value);
                    
                    case RuleField.HasTag:
                        return EvaluateTagField(contact.Id, rule.Operator, rule.Value, tagAssignmentLookup);
                    
                    case RuleField.CustomAttribute:
                        return EvaluateCustomAttributeField(contact.CustomAttributes, rule.AttributeName, 
                            rule.Operator, rule.Value);
                    
                    case RuleField.TotalMessagesSent:
                    case RuleField.TotalMessagesDelivered:
                    case RuleField.TotalClicks:
                    case RuleField.EngagementScore:
                    case RuleField.LastEngagementDate:
                        return EvaluateEngagementField(contact.Id, rule.Field, rule.Operator, rule.Value, engagementLookup);
                    
                    default:
                        _logger.LogWarning("Unknown rule field: {Field}", rule.Field);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating rule for contact {ContactId}, field {Field}", 
                    contact.Id, rule.Field);
                return false;
            }
        }

        private bool EvaluateStringField(string? fieldValue, RuleOperator op, string? ruleValue)
        {
            var isFieldNull = string.IsNullOrEmpty(fieldValue);
            fieldValue = fieldValue?.ToLower() ?? "";
            ruleValue = ruleValue?.ToLower() ?? "";

            switch (op)
            {
                case RuleOperator.Equals:
                    return fieldValue == ruleValue;
                case RuleOperator.NotEquals:
                    return fieldValue != ruleValue;
                case RuleOperator.Contains:
                    return fieldValue.Contains(ruleValue);
                case RuleOperator.NotContains:
                    return !fieldValue.Contains(ruleValue);
                case RuleOperator.StartsWith:
                    return fieldValue.StartsWith(ruleValue);
                case RuleOperator.EndsWith:
                    return fieldValue.EndsWith(ruleValue);
                case RuleOperator.IsNull:
                    return isFieldNull;
                case RuleOperator.IsNotNull:
                    return !isFieldNull;
                case RuleOperator.In:
                    var values = ruleValue.Split(',').Select(v => v.Trim()).ToList();
                    return values.Contains(fieldValue);
                case RuleOperator.NotIn:
                    var notValues = ruleValue.Split(',').Select(v => v.Trim()).ToList();
                    return !notValues.Contains(fieldValue);
                default:
                    return false;
            }
        }

        private bool EvaluateBooleanField(bool fieldValue, RuleOperator op, string? ruleValue)
        {
            if (!bool.TryParse(ruleValue, out bool targetValue))
                return false;

            switch (op)
            {
                case RuleOperator.Equals:
                    return fieldValue == targetValue;
                case RuleOperator.NotEquals:
                    return fieldValue != targetValue;
                default:
                    return false;
            }
        }

        private bool EvaluateNumericField(decimal fieldValue, RuleOperator op, string? ruleValue)
        {
            if (!decimal.TryParse(ruleValue, out decimal targetValue))
                return false;

            switch (op)
            {
                case RuleOperator.Equals:
                    return fieldValue == targetValue;
                case RuleOperator.NotEquals:
                    return fieldValue != targetValue;
                case RuleOperator.GreaterThan:
                    return fieldValue > targetValue;
                case RuleOperator.LessThan:
                    return fieldValue < targetValue;
                case RuleOperator.GreaterThanOrEqual:
                    return fieldValue >= targetValue;
                case RuleOperator.LessThanOrEqual:
                    return fieldValue <= targetValue;
                default:
                    return false;
            }
        }

        private bool EvaluateDateField(DateTime? fieldValue, RuleOperator op, string? ruleValue)
        {
            if (!fieldValue.HasValue)
            {
                return op == RuleOperator.IsNull;
            }

            if (op == RuleOperator.IsNotNull)
                return true;

            if (!DateTime.TryParse(ruleValue, out DateTime targetValue))
                return false;

            switch (op)
            {
                case RuleOperator.Equals:
                    return fieldValue.Value.Date == targetValue.Date;
                case RuleOperator.NotEquals:
                    return fieldValue.Value.Date != targetValue.Date;
                case RuleOperator.GreaterThan:
                    return fieldValue.Value > targetValue;
                case RuleOperator.LessThan:
                    return fieldValue.Value < targetValue;
                case RuleOperator.GreaterThanOrEqual:
                    return fieldValue.Value >= targetValue;
                case RuleOperator.LessThanOrEqual:
                    return fieldValue.Value <= targetValue;
                default:
                    return false;
            }
        }

        private bool EvaluateTagField(
            int contactId, 
            RuleOperator op, 
            string? ruleValue,
            Dictionary<int, List<ContactTagAssignment>> tagAssignmentLookup)
        {
            var hasTagAssignments = tagAssignmentLookup.TryGetValue(contactId, out var tagAssignments);
            var hasTag = hasTagAssignments && tagAssignments != null && tagAssignments.Any();

            if (string.IsNullOrEmpty(ruleValue))
            {
                // Check if contact has any tags
                return op == RuleOperator.IsNotNull ? hasTag : !hasTag;
            }

            // Check if contact has a specific tag (by tag ID)
            if (int.TryParse(ruleValue, out int tagId))
            {
                var hasSpecificTag = hasTagAssignments && tagAssignments != null && 
                    tagAssignments.Any(ta => ta.ContactTagId == tagId);
                return op == RuleOperator.Equals ? hasSpecificTag : !hasSpecificTag;
            }

            return false;
        }

        private bool EvaluateCustomAttributeField(string? customAttributesJson, string? attributeName, 
            RuleOperator op, string? ruleValue)
        {
            if (string.IsNullOrEmpty(attributeName))
                return false;

            Dictionary<string, string>? attributes = null;
            
            if (!string.IsNullOrEmpty(customAttributesJson))
            {
                try
                {
                    attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(customAttributesJson);
                }
                catch (JsonException)
                {
                    // Invalid JSON, treat as no attributes
                    return op == RuleOperator.IsNull;
                }
            }

            if (attributes == null || !attributes.ContainsKey(attributeName))
            {
                // Attribute doesn't exist
                return op == RuleOperator.IsNull;
            }

            // Handle IsNotNull case when attribute exists
            if (op == RuleOperator.IsNotNull)
            {
                return true;
            }

            var attributeValue = attributes[attributeName];
            return EvaluateStringField(attributeValue, op, ruleValue);
        }

        private bool EvaluateEngagementField(
            int contactId, 
            RuleField field, 
            RuleOperator op, 
            string? ruleValue,
            Dictionary<int, ContactEngagement> engagementLookup)
        {
            if (!engagementLookup.TryGetValue(contactId, out var engagement))
            {
                return op == RuleOperator.IsNull;
            }

            switch (field)
            {
                case RuleField.TotalMessagesSent:
                    return EvaluateNumericField(engagement.TotalMessagesSent, op, ruleValue);
                
                case RuleField.TotalMessagesDelivered:
                    return EvaluateNumericField(engagement.TotalMessagesDelivered, op, ruleValue);
                
                case RuleField.TotalClicks:
                    return EvaluateNumericField(engagement.TotalClicks, op, ruleValue);
                
                case RuleField.EngagementScore:
                    return EvaluateNumericField(engagement.EngagementScore, op, ruleValue);
                
                case RuleField.LastEngagementDate:
                    return EvaluateDateField(engagement.LastEngagementDate, op, ruleValue);
                
                default:
                    return false;
            }
        }
    }
}
