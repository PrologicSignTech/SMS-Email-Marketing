using AutoMapper;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.ContactTag;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class ContactTagService : IContactTagService
    {
        private readonly IRepository<ContactTag> _tagRepository;
        private readonly IRepository<ContactTagAssignment> _assignmentRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactTagService> _logger;

        public ContactTagService(
            IRepository<ContactTag> tagRepository,
            IRepository<ContactTagAssignment> assignmentRepository,
            IRepository<Contact> contactRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ContactTagService> logger)
        {
            _tagRepository = tagRepository;
            _assignmentRepository = assignmentRepository;
            _contactRepository = contactRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ContactTagDto?> GetByIdAsync(string userId, int id)
        {
            var tag = await _tagRepository.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId && !t.IsDeleted);

            if (tag == null)
                return null;

            var dto = _mapper.Map<ContactTagDto>(tag);
            
            // Get contact count for this tag
            var assignments = await _assignmentRepository.FindAsync(a =>
                a.ContactTagId == id && !a.IsDeleted);
            dto.ContactCount = assignments.Count();

            return dto;
        }

        public async Task<PaginatedResult<ContactTagDto>> GetAllAsync(string userId, PagedRequest request)
        {
            var query = (await _tagRepository.FindAsync(t =>
                t.UserId == userId && !t.IsDeleted)).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(searchLower));
            }

            var totalCount = query.Count();

            // Apply sorting
            query = query.OrderBy(t => t.Name);

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var tagIds = items.Select(t => t.Id).ToList();

            // Get contact counts for all tags in a single query
            var assignmentCounts = (await _assignmentRepository.FindAsync(a =>
                tagIds.Contains(a.ContactTagId) && !a.IsDeleted))
                .GroupBy(a => a.ContactTagId)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtos = new List<ContactTagDto>();
            foreach (var tag in items)
            {
                var dto = _mapper.Map<ContactTagDto>(tag);
                dto.ContactCount = assignmentCounts.TryGetValue(tag.Id, out var count) ? count : 0;
                dtos.Add(dto);
            }

            return new PaginatedResult<ContactTagDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<ContactTagDto> CreateAsync(string userId, CreateContactTagDto dto)
        {
            // Check for duplicate tag name
            var existing = await _tagRepository.FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.Name.ToLower() == dto.Name.ToLower() &&
                !t.IsDeleted);

            if (existing != null)
            {
                throw new InvalidOperationException($"Tag '{dto.Name}' already exists.");
            }

            var tag = _mapper.Map<ContactTag>(dto);
            tag.UserId = userId;

            await _tagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ContactTagDto>(tag);
            result.ContactCount = 0;
            return result;
        }

        public async Task<bool> UpdateAsync(string userId, int id, CreateContactTagDto dto)
        {
            var tag = await _tagRepository.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId && !t.IsDeleted);

            if (tag == null)
                return false;

            // Check for duplicate tag name
            var existing = await _tagRepository.FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.Name.ToLower() == dto.Name.ToLower() &&
                t.Id != id &&
                !t.IsDeleted);

            if (existing != null)
            {
                throw new InvalidOperationException($"Tag '{dto.Name}' already exists.");
            }

            tag.Name = dto.Name;
            tag.Color = dto.Color;
            tag.UpdatedAt = DateTime.UtcNow;

            _tagRepository.Update(tag);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(string userId, int id)
        {
            var tag = await _tagRepository.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId && !t.IsDeleted);

            if (tag == null)
                return false;

            tag.IsDeleted = true;
            tag.UpdatedAt = DateTime.UtcNow;

            // Also delete all tag assignments
            var assignments = await _assignmentRepository.FindAsync(a =>
                a.ContactTagId == id && !a.IsDeleted);

            foreach (var assignment in assignments)
            {
                assignment.IsDeleted = true;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            _tagRepository.Update(tag);
            _assignmentRepository.UpdateRange(assignments);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTagToContactAsync(string userId, int contactId, int tagId)
        {
            // Verify contact ownership
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return false;

            // Verify tag ownership
            var tag = await _tagRepository.FirstOrDefaultAsync(t =>
                t.Id == tagId && t.UserId == userId && !t.IsDeleted);

            if (tag == null)
                return false;

            // Check if already assigned
            var existing = await _assignmentRepository.FirstOrDefaultAsync(a =>
                a.ContactId == contactId && a.ContactTagId == tagId && !a.IsDeleted);

            if (existing != null)
                return false; // Already assigned

            var assignment = new ContactTagAssignment
            {
                ContactId = contactId,
                ContactTagId = tagId
            };

            await _assignmentRepository.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveTagFromContactAsync(string userId, int contactId, int tagId)
        {
            // Verify contact ownership
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return false;

            // Find assignment
            var assignment = await _assignmentRepository.FirstOrDefaultAsync(a =>
                a.ContactId == contactId && a.ContactTagId == tagId && !a.IsDeleted);

            if (assignment == null)
                return false;

            assignment.IsDeleted = true;
            assignment.UpdatedAt = DateTime.UtcNow;

            _assignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<ContactTagDto>> GetContactTagsAsync(string userId, int contactId)
        {
            // Verify contact ownership
            var contact = await _contactRepository.FirstOrDefaultAsync(c =>
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return new List<ContactTagDto>();

            // Get all tag assignments for this contact
            var assignments = await _assignmentRepository.FindAsync(a =>
                a.ContactId == contactId && !a.IsDeleted);

            var tagIds = assignments.Select(a => a.ContactTagId).ToList();

            if (!tagIds.Any())
                return new List<ContactTagDto>();

            // Get tags
            var tags = await _tagRepository.FindAsync(t =>
                tagIds.Contains(t.Id) && !t.IsDeleted);

            // Get contact counts for all tags in a single query
            var assignmentCounts = (await _assignmentRepository.FindAsync(a =>
                tagIds.Contains(a.ContactTagId) && !a.IsDeleted))
                .GroupBy(a => a.ContactTagId)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtos = new List<ContactTagDto>();
            foreach (var tag in tags)
            {
                var dto = _mapper.Map<ContactTagDto>(tag);
                dto.ContactCount = assignmentCounts.TryGetValue(tag.Id, out var count) ? count : 0;
                dtos.Add(dto);
            }

            return dtos.OrderBy(t => t.Name).ToList();
        }
    }
}
