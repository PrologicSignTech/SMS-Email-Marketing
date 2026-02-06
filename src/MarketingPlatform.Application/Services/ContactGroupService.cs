using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.DTOs.ContactGroup;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingPlatform.Application.Services
{
    public class ContactGroupService : IContactGroupService
    {
        private readonly IRepository<ContactGroup> _groupRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroupMember> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactGroupService> _logger;
        private readonly IDynamicGroupEvaluationService _dynamicGroupService;

        public ContactGroupService(
            IRepository<ContactGroup> groupRepository,
            IRepository<Contact> contactRepository,
            IRepository<ContactGroupMember> memberRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ContactGroupService> logger,
            IDynamicGroupEvaluationService dynamicGroupService)
        {
            _groupRepository = groupRepository;
            _contactRepository = contactRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dynamicGroupService = dynamicGroupService;
        }

        public async Task<ContactGroupDto?> GetGroupByIdAsync(string userId, int groupId)
        {
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return null;

            var dto = _mapper.Map<ContactGroupDto>(group);
            
            // Deserialize rule criteria if it's a dynamic group
            if (group.IsDynamic && !string.IsNullOrEmpty(group.RuleCriteria))
            {
                dto.RuleCriteria = DeserializeRuleCriteria(group.RuleCriteria);
            }

            return dto;
        }

        public async Task<PaginatedResult<ContactGroupDto>> GetGroupsAsync(string userId, PagedRequest request)
        {
            var query = (await _groupRepository.FindAsync(g => 
                g.UserId == userId && !g.IsDeleted)).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(g =>
                    g.Name.ToLower().Contains(searchLower) ||
                    (g.Description != null && g.Description.ToLower().Contains(searchLower)));
            }

            var totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "name" => request.SortDescending ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name),
                    "contactcount" => request.SortDescending ? query.OrderByDescending(g => g.ContactCount) : query.OrderBy(g => g.ContactCount),
                    "createdat" => request.SortDescending ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt),
                    _ => query.OrderByDescending(g => g.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(g => g.CreatedAt);
            }

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<ContactGroupDto>>(items);

            return new PaginatedResult<ContactGroupDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<ContactGroupDto> CreateGroupAsync(string userId, CreateContactGroupDto dto)
        {
            // Validate that a group is either static or dynamic, not both
            if (dto.IsStatic && dto.IsDynamic)
            {
                throw new InvalidOperationException("A group cannot be both static and dynamic");
            }

            // Validate dynamic group has rule criteria
            if (dto.IsDynamic && (dto.RuleCriteria == null || !dto.RuleCriteria.Rules.Any()))
            {
                throw new InvalidOperationException("Dynamic groups must have rule criteria defined");
            }

            var group = new ContactGroup
            {
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                IsStatic = dto.IsStatic,
                IsDynamic = dto.IsDynamic,
                ContactCount = 0
            };

            // Serialize rule criteria for dynamic groups
            if (dto.IsDynamic && dto.RuleCriteria != null)
            {
                var criteria = _mapper.Map<GroupRuleCriteria>(dto.RuleCriteria);
                group.RuleCriteria = JsonConvert.SerializeObject(criteria);
            }

            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            // For dynamic groups, immediately evaluate and populate members
            if (group.IsDynamic)
            {
                await _dynamicGroupService.UpdateDynamicGroupMembershipsAsync(userId, group.Id);
            }

            var result = _mapper.Map<ContactGroupDto>(group);
            if (group.IsDynamic && !string.IsNullOrEmpty(group.RuleCriteria))
            {
                result.RuleCriteria = DeserializeRuleCriteria(group.RuleCriteria);
            }

            return result;
        }

        public async Task<bool> UpdateGroupAsync(string userId, int groupId, CreateContactGroupDto dto)
        {
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            // Validate that a group is either static or dynamic, not both
            if (dto.IsStatic && dto.IsDynamic)
            {
                throw new InvalidOperationException("A group cannot be both static and dynamic");
            }

            // Validate dynamic group has rule criteria
            if (dto.IsDynamic && (dto.RuleCriteria == null || !dto.RuleCriteria.Rules.Any()))
            {
                throw new InvalidOperationException("Dynamic groups must have rule criteria defined");
            }

            group.Name = dto.Name;
            group.Description = dto.Description;
            group.IsStatic = dto.IsStatic;
            group.IsDynamic = dto.IsDynamic;

            // Serialize rule criteria for dynamic groups
            if (dto.IsDynamic && dto.RuleCriteria != null)
            {
                var criteria = _mapper.Map<GroupRuleCriteria>(dto.RuleCriteria);
                group.RuleCriteria = JsonConvert.SerializeObject(criteria);
            }
            else
            {
                group.RuleCriteria = null;
            }

            group.UpdatedAt = DateTime.UtcNow;

            _groupRepository.Update(group);
            await _unitOfWork.SaveChangesAsync();

            // For dynamic groups, re-evaluate and update members
            if (group.IsDynamic)
            {
                await _dynamicGroupService.UpdateDynamicGroupMembershipsAsync(userId, groupId);
            }

            return true;
        }

        public async Task<bool> DeleteGroupAsync(string userId, int groupId)
        {
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            group.IsDeleted = true;
            group.UpdatedAt = DateTime.UtcNow;

            // Soft delete all group memberships
            var members = await _memberRepository.FindAsync(m => 
                m.ContactGroupId == groupId && !m.IsDeleted);
            
            foreach (var member in members)
            {
                member.IsDeleted = true;
                member.UpdatedAt = DateTime.UtcNow;
            }

            _groupRepository.Update(group);
            _memberRepository.UpdateRange(members);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddContactToGroupAsync(string userId, int groupId, int contactId)
        {
            // Verify group ownership
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            // Verify contact ownership
            var contact = await _contactRepository.FirstOrDefaultAsync(c => 
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return false;

            // Check for duplicate membership
            var existingMember = await _memberRepository.FirstOrDefaultAsync(m => 
                m.ContactId == contactId && m.ContactGroupId == groupId && !m.IsDeleted);

            if (existingMember != null)
                return false; // Already a member

            // Add to group
            var member = new ContactGroupMember
            {
                ContactId = contactId,
                ContactGroupId = groupId,
                JoinedAt = DateTime.UtcNow
            };

            await _memberRepository.AddAsync(member);

            // Update contact count
            group.ContactCount++;
            _groupRepository.Update(group);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveContactFromGroupAsync(string userId, int groupId, int contactId)
        {
            // Verify group ownership
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            // Find membership
            var member = await _memberRepository.FirstOrDefaultAsync(m => 
                m.ContactId == contactId && m.ContactGroupId == groupId && !m.IsDeleted);

            if (member == null)
                return false;

            // Remove from group
            member.IsDeleted = true;
            member.UpdatedAt = DateTime.UtcNow;
            _memberRepository.Update(member);

            // Update contact count
            if (group.ContactCount > 0)
            {
                group.ContactCount--;
                _groupRepository.Update(group);
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddContactsToGroupAsync(string userId, int groupId, List<int> contactIds)
        {
            // Verify group ownership
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            // Verify all contacts belong to user
            var contacts = await _contactRepository.FindAsync(c => 
                contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);

            if (!contacts.Any())
                return false;

            // Get all existing memberships in a single query to avoid N+1
            var existingMembers = await _memberRepository.FindAsync(m => 
                contactIds.Contains(m.ContactId) && m.ContactGroupId == groupId && !m.IsDeleted);
            var existingContactIds = existingMembers.Select(m => m.ContactId).ToHashSet();

            int addedCount = 0;

            foreach (var contactId in contactIds)
            {
                // Check for duplicate membership using the pre-fetched set
                if (existingContactIds.Contains(contactId))
                    continue; // Skip duplicates

                // Add to group
                var member = new ContactGroupMember
                {
                    ContactId = contactId,
                    ContactGroupId = groupId,
                    JoinedAt = DateTime.UtcNow
                };

                await _memberRepository.AddAsync(member);
                addedCount++;
            }

            // Update contact count
            group.ContactCount += addedCount;
            _groupRepository.Update(group);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveContactsFromGroupAsync(string userId, int groupId, List<int> contactIds)
        {
            // Verify group ownership
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
                return false;

            // Find memberships
            var members = await _memberRepository.FindAsync(m => 
                contactIds.Contains(m.ContactId) && m.ContactGroupId == groupId && !m.IsDeleted);

            if (!members.Any())
                return false;

            int removedCount = 0;

            foreach (var member in members)
            {
                member.IsDeleted = true;
                member.UpdatedAt = DateTime.UtcNow;
                removedCount++;
            }

            _memberRepository.UpdateRange(members);

            // Update contact count
            group.ContactCount -= removedCount;
            if (group.ContactCount < 0)
                group.ContactCount = 0;

            _groupRepository.Update(group);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PaginatedResult<ContactDto>> GetGroupContactsAsync(string userId, int groupId, PagedRequest request)
        {
            // Verify group ownership
            var group = await _groupRepository.FirstOrDefaultAsync(g => 
                g.Id == groupId && g.UserId == userId && !g.IsDeleted);

            if (group == null)
            {
                return new PaginatedResult<ContactDto>
                {
                    Items = new List<ContactDto>(),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }

            // Get members
            var members = await _memberRepository.FindAsync(m => 
                m.ContactGroupId == groupId && !m.IsDeleted);

            var contactIds = members.Select(m => m.ContactId).ToList();

            if (!contactIds.Any())
            {
                return new PaginatedResult<ContactDto>
                {
                    Items = new List<ContactDto>(),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }

            var query = (await _contactRepository.FindAsync(c => 
                contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted)).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                    c.PhoneNumber.ToLower().Contains(searchLower) ||
                    (c.FirstName != null && c.FirstName.ToLower().Contains(searchLower)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(searchLower)));
            }

            var totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "email" => request.SortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                    "firstname" => request.SortDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
                    "lastname" => request.SortDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
                    "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    _ => query.OrderByDescending(c => c.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<ContactDto>();
            foreach (var contact in items)
            {
                var dto = _mapper.Map<ContactDto>(contact);
                dto.CustomAttributes = DeserializeCustomAttributes(contact.CustomAttributes);
                dtos.Add(dto);
            }

            return new PaginatedResult<ContactDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
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
            catch (JsonException)
            {
                // Invalid JSON format in custom attributes, return null
                return null;
            }
        }

        private GroupRuleCriteriaDto? DeserializeRuleCriteria(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var criteria = JsonConvert.DeserializeObject<GroupRuleCriteria>(json);
                return _mapper.Map<GroupRuleCriteriaDto>(criteria);
            }
            catch (JsonException)
            {
                // Invalid JSON format in rule criteria, return null
                return null;
            }
        }
    }
}
