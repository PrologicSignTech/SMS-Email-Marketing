using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using MarketingPlatform.Application.DTOs.Common;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.Text;

namespace MarketingPlatform.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IRepository<Contact> _contactRepository;
        private readonly IRepository<ContactGroupMember> _groupMemberRepository;
        private readonly IRepository<ContactTagAssignment> _tagRepository;
        private readonly IRepository<ContactGroup> _contactGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IRepository<Contact> contactRepository,
            IRepository<ContactGroupMember> groupMemberRepository,
            IRepository<ContactTagAssignment> tagRepository,
            IRepository<ContactGroup> contactGroupRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ContactService> logger)
        {
            _contactRepository = contactRepository;
            _groupMemberRepository = groupMemberRepository;
            _tagRepository = tagRepository;
            _contactGroupRepository = contactGroupRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ContactDto?> GetContactByIdAsync(string userId, int contactId)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c => 
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return null;

            var dto = _mapper.Map<ContactDto>(contact);
            dto.CustomAttributes = DeserializeCustomAttributes(contact.CustomAttributes);

            // Load groups
            var groupMembers = await _groupMemberRepository.FindAsync(gm => 
                gm.ContactId == contactId && !gm.IsDeleted);
            var groupIds = groupMembers.Select(gm => gm.ContactGroupId).ToList();
            var groups = await _contactGroupRepository.FindAsync(g => 
                groupIds.Contains(g.Id) && !g.IsDeleted);
            dto.Groups = groups.Select(g => g.Name).ToList();

            // Load tags
            var tagAssignments = await _tagRepository.FindAsync(ta => 
                ta.ContactId == contactId && !ta.IsDeleted);
            dto.Tags = tagAssignments.Select(ta => ta.ContactTagId.ToString()).ToList();

            return dto;
        }

        public async Task<PaginatedResult<ContactDto>> GetContactsAsync(string userId, PagedRequest request)
        {
            var query = (await _contactRepository.FindAsync(c => 
                c.UserId == userId && !c.IsDeleted)).AsQueryable();

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

        public async Task<ContactDto> CreateContactAsync(string userId, CreateContactDto dto)
        {
            // Check for duplicates
            var duplicateCheck = await CheckForDuplicatesAsync(userId, new CheckDuplicateDto 
            { 
                PhoneNumber = dto.PhoneNumber, 
                Email = dto.Email 
            });

            if (duplicateCheck.HasDuplicates)
            {
                var duplicateInfo = string.Join(", ", duplicateCheck.Duplicates.Select(d => d.DuplicateReason));
                throw new InvalidOperationException($"Duplicate contact found with matching {duplicateInfo}. Use the POST /api/contacts/duplicates/resolve endpoint to handle duplicates, or modify the conflicting fields.");
            }

            var contact = _mapper.Map<Contact>(dto);
            contact.UserId = userId;
            contact.CustomAttributes = SerializeCustomAttributes(dto.CustomAttributes);

            await _contactRepository.AddAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ContactDto>(contact);
            result.CustomAttributes = dto.CustomAttributes;
            return result;
        }

        public async Task<bool> UpdateContactAsync(string userId, int contactId, UpdateContactDto dto)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c => 
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return false;

            // Check for duplicates if email or phone is being changed
            if ((dto.Email != null && dto.Email != contact.Email) || 
                (dto.PhoneNumber != null && dto.PhoneNumber != contact.PhoneNumber))
            {
                var duplicateCheck = await CheckForDuplicatesAsync(userId, new CheckDuplicateDto 
                { 
                    PhoneNumber = dto.PhoneNumber ?? contact.PhoneNumber, 
                    Email = dto.Email ?? contact.Email 
                }, excludeContactId: contactId);

                if (duplicateCheck.HasDuplicates)
                {
                    var duplicateInfo = string.Join(", ", duplicateCheck.Duplicates.Select(d => d.DuplicateReason));
                    throw new InvalidOperationException($"Duplicate contact found with matching {duplicateInfo}. Use the POST /api/contacts/duplicates/resolve endpoint to handle duplicates, or modify the conflicting fields.");
                }
            }

            contact.PhoneNumber = dto.PhoneNumber ?? contact.PhoneNumber;
            contact.Email = dto.Email ?? contact.Email;
            contact.FirstName = dto.FirstName;
            contact.LastName = dto.LastName;
            contact.Country = dto.Country;
            contact.City = dto.City;
            contact.PostalCode = dto.PostalCode;
            contact.IsActive = dto.IsActive;
            contact.CustomAttributes = SerializeCustomAttributes(dto.CustomAttributes);
            contact.UpdatedAt = DateTime.UtcNow;

            _contactRepository.Update(contact);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteContactAsync(string userId, int contactId)
        {
            var contact = await _contactRepository.FirstOrDefaultAsync(c => 
                c.Id == contactId && c.UserId == userId && !c.IsDeleted);

            if (contact == null)
                return false;

            contact.IsDeleted = true;
            contact.UpdatedAt = DateTime.UtcNow;

            _contactRepository.Update(contact);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteContactsAsync(string userId, List<int> contactIds)
        {
            var contacts = await _contactRepository.FindAsync(c => 
                contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);

            if (!contacts.Any())
                return false;

            foreach (var contact in contacts)
            {
                contact.IsDeleted = true;
                contact.UpdatedAt = DateTime.UtcNow;
            }

            _contactRepository.UpdateRange(contacts);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ContactImportResultDto> ImportContactsFromCsvAsync(string userId, IFormFile file, int? groupId = null)
        {
            var result = new ContactImportResultDto();

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                });

                var records = csv.GetRecords<ContactImportDto>().ToList();
                result.TotalRows = records.Count;

                foreach (var record in records)
                {
                    try
                    {
                        // Check for duplicate with detailed information
                        var duplicateCheck = await CheckForDuplicatesAsync(userId, new CheckDuplicateDto
                        {
                            PhoneNumber = record.PhoneNumber,
                            Email = record.Email
                        });

                        if (duplicateCheck.HasDuplicates)
                        {
                            result.DuplicateCount++;
                            var firstDuplicate = duplicateCheck.Duplicates.First();
                            result.DuplicateDetails.Add(new DuplicateImportContactDto
                            {
                                PhoneNumber = record.PhoneNumber,
                                Email = record.Email,
                                FirstName = record.FirstName,
                                LastName = record.LastName,
                                DuplicateReason = firstDuplicate.DuplicateReason,
                                ExistingContactId = firstDuplicate.ContactId
                            });
                            continue;
                        }

                        var contact = new Contact
                        {
                            UserId = userId,
                            PhoneNumber = record.PhoneNumber ?? string.Empty,
                            Email = record.Email,
                            FirstName = record.FirstName,
                            LastName = record.LastName,
                            Country = record.Country,
                            City = record.City,
                            PostalCode = record.PostalCode,
                            IsActive = true
                        };

                        await _contactRepository.AddAsync(contact);
                        await _unitOfWork.SaveChangesAsync();

                        // Add to group if specified
                        if (groupId.HasValue)
                        {
                            var groupMember = new ContactGroupMember
                            {
                                ContactId = contact.Id,
                                ContactGroupId = groupId.Value,
                                JoinedAt = DateTime.UtcNow
                            };
                            await _groupMemberRepository.AddAsync(groupMember);
                            
                            // Update group contact count
                            var group = await _contactGroupRepository.GetByIdAsync(groupId.Value);
                            if (group != null)
                            {
                                group.ContactCount++;
                                _contactGroupRepository.Update(group);
                            }
                        }

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row error: {ex.Message}");
                        _logger.LogError(ex, "Error importing contact");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"File processing error: {ex.Message}");
                _logger.LogError(ex, "Error processing CSV file");
            }

            return result;
        }

        public async Task<ContactImportResultDto> ImportContactsFromExcelAsync(string userId, IFormFile file, int? groupId = null)
        {
            var result = new ContactImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                result.TotalRows = rowCount - 1; // Exclude header

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var phoneNumber = worksheet.Cells[row, 1].Value?.ToString();
                        var email = worksheet.Cells[row, 2].Value?.ToString();
                        var firstName = worksheet.Cells[row, 3].Value?.ToString();
                        var lastName = worksheet.Cells[row, 4].Value?.ToString();
                        var country = worksheet.Cells[row, 5].Value?.ToString();
                        var city = worksheet.Cells[row, 6].Value?.ToString();
                        var postalCode = worksheet.Cells[row, 7].Value?.ToString();

                        // Check for duplicate with detailed information
                        var duplicateCheck = await CheckForDuplicatesAsync(userId, new CheckDuplicateDto
                        {
                            PhoneNumber = phoneNumber,
                            Email = email
                        });

                        if (duplicateCheck.HasDuplicates)
                        {
                            result.DuplicateCount++;
                            var firstDuplicate = duplicateCheck.Duplicates.First();
                            result.DuplicateDetails.Add(new DuplicateImportContactDto
                            {
                                PhoneNumber = phoneNumber,
                                Email = email,
                                FirstName = firstName,
                                LastName = lastName,
                                DuplicateReason = firstDuplicate.DuplicateReason,
                                ExistingContactId = firstDuplicate.ContactId
                            });
                            continue;
                        }

                        var contact = new Contact
                        {
                            UserId = userId,
                            PhoneNumber = phoneNumber ?? string.Empty,
                            Email = email,
                            FirstName = firstName,
                            LastName = lastName,
                            Country = country,
                            City = city,
                            PostalCode = postalCode,
                            IsActive = true
                        };

                        await _contactRepository.AddAsync(contact);
                        await _unitOfWork.SaveChangesAsync();

                        // Add to group if specified
                        if (groupId.HasValue)
                        {
                            var groupMember = new ContactGroupMember
                            {
                                ContactId = contact.Id,
                                ContactGroupId = groupId.Value,
                                JoinedAt = DateTime.UtcNow
                            };
                            await _groupMemberRepository.AddAsync(groupMember);
                            
                            // Update group contact count
                            var group = await _contactGroupRepository.GetByIdAsync(groupId.Value);
                            if (group != null)
                            {
                                group.ContactCount++;
                                _contactGroupRepository.Update(group);
                            }
                        }

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row {row} error: {ex.Message}");
                        _logger.LogError(ex, $"Error importing contact at row {row}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"File processing error: {ex.Message}");
                _logger.LogError(ex, "Error processing Excel file");
            }

            return result;
        }

        public async Task<byte[]> ExportContactsToCsvAsync(string userId, List<int>? contactIds = null)
        {
            IEnumerable<Contact> contacts;

            if (contactIds != null && contactIds.Any())
            {
                contacts = await _contactRepository.FindAsync(c => 
                    contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);
            }
            else
            {
                contacts = await _contactRepository.FindAsync(c => 
                    c.UserId == userId && !c.IsDeleted);
            }

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteField("PhoneNumber");
            csv.WriteField("Email");
            csv.WriteField("FirstName");
            csv.WriteField("LastName");
            csv.WriteField("Country");
            csv.WriteField("City");
            csv.WriteField("PostalCode");
            csv.WriteField("CreatedAt");
            csv.NextRecord();

            foreach (var contact in contacts)
            {
                csv.WriteField(contact.PhoneNumber);
                csv.WriteField(contact.Email);
                csv.WriteField(contact.FirstName);
                csv.WriteField(contact.LastName);
                csv.WriteField(contact.Country);
                csv.WriteField(contact.City);
                csv.WriteField(contact.PostalCode);
                csv.WriteField(contact.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportContactsToExcelAsync(string userId, List<int>? contactIds = null)
        {
            IEnumerable<Contact> contacts;

            if (contactIds != null && contactIds.Any())
            {
                contacts = await _contactRepository.FindAsync(c => 
                    contactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);
            }
            else
            {
                contacts = await _contactRepository.FindAsync(c => 
                    c.UserId == userId && !c.IsDeleted);
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Contacts");

            // Headers
            worksheet.Cells[1, 1].Value = "PhoneNumber";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "FirstName";
            worksheet.Cells[1, 4].Value = "LastName";
            worksheet.Cells[1, 5].Value = "Country";
            worksheet.Cells[1, 6].Value = "City";
            worksheet.Cells[1, 7].Value = "PostalCode";
            worksheet.Cells[1, 8].Value = "CreatedAt";

            // Data
            int row = 2;
            foreach (var contact in contacts)
            {
                worksheet.Cells[row, 1].Value = contact.PhoneNumber;
                worksheet.Cells[row, 2].Value = contact.Email;
                worksheet.Cells[row, 3].Value = contact.FirstName;
                worksheet.Cells[row, 4].Value = contact.LastName;
                worksheet.Cells[row, 5].Value = contact.Country;
                worksheet.Cells[row, 6].Value = contact.City;
                worksheet.Cells[row, 7].Value = contact.PostalCode;
                worksheet.Cells[row, 8].Value = contact.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<List<ContactDto>> SearchContactsAsync(string userId, string searchTerm)
        {
            var searchLower = searchTerm.ToLower();
            var contacts = await _contactRepository.FindAsync(c =>
                c.UserId == userId &&
                !c.IsDeleted &&
                ((c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                 c.PhoneNumber.ToLower().Contains(searchLower) ||
                 (c.FirstName != null && c.FirstName.ToLower().Contains(searchLower)) ||
                 (c.LastName != null && c.LastName.ToLower().Contains(searchLower))));

            var dtos = new List<ContactDto>();
            foreach (var contact in contacts)
            {
                var dto = _mapper.Map<ContactDto>(contact);
                dto.CustomAttributes = DeserializeCustomAttributes(contact.CustomAttributes);
                dtos.Add(dto);
            }

            return dtos;
        }

        private string? SerializeCustomAttributes(Dictionary<string, string>? attributes)
        {
            if (attributes == null || !attributes.Any())
                return null;

            return JsonConvert.SerializeObject(attributes);
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

        public async Task<DuplicateCheckResultDto> CheckForDuplicatesAsync(string userId, CheckDuplicateDto dto, int? excludeContactId = null)
        {
            var result = new DuplicateCheckResultDto();
            var duplicates = new List<DuplicateContactDto>();

            // Check for email duplicates
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailDuplicates = await _contactRepository.FindAsync(c =>
                    c.UserId == userId &&
                    !c.IsDeleted &&
                    c.Email != null &&
                    c.Email.ToLower() == dto.Email.ToLower() &&
                    (!excludeContactId.HasValue || c.Id != excludeContactId.Value));

                foreach (var contact in emailDuplicates)
                {
                    var existing = duplicates.FirstOrDefault(d => d.ContactId == contact.Id);
                    if (existing != null)
                    {
                        existing.DuplicateReason = "Both";
                    }
                    else
                    {
                        duplicates.Add(new DuplicateContactDto
                        {
                            ContactId = contact.Id,
                            PhoneNumber = contact.PhoneNumber,
                            Email = contact.Email,
                            FirstName = contact.FirstName,
                            LastName = contact.LastName,
                            CreatedAt = contact.CreatedAt,
                            DuplicateReason = "Email"
                        });
                    }
                }
            }

            // Check for phone number duplicates
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var phoneDuplicates = await _contactRepository.FindAsync(c =>
                    c.UserId == userId &&
                    !c.IsDeleted &&
                    c.PhoneNumber != null &&
                    c.PhoneNumber.ToLower() == dto.PhoneNumber.ToLower() &&
                    (!excludeContactId.HasValue || c.Id != excludeContactId.Value));

                foreach (var contact in phoneDuplicates)
                {
                    var existing = duplicates.FirstOrDefault(d => d.ContactId == contact.Id);
                    if (existing != null)
                    {
                        existing.DuplicateReason = "Both";
                    }
                    else
                    {
                        duplicates.Add(new DuplicateContactDto
                        {
                            ContactId = contact.Id,
                            PhoneNumber = contact.PhoneNumber,
                            Email = contact.Email,
                            FirstName = contact.FirstName,
                            LastName = contact.LastName,
                            CreatedAt = contact.CreatedAt,
                            DuplicateReason = "PhoneNumber"
                        });
                    }
                }
            }

            result.Duplicates = duplicates;
            result.HasDuplicates = duplicates.Any();
            return result;
        }

        public async Task<DuplicateReportDto> GetDuplicateReportAsync(string userId)
        {
            var report = new DuplicateReportDto();
            var groups = new Dictionary<string, DuplicateGroupDto>();

            // Note: For large datasets (>10k contacts), consider pagination or background processing
            var allContacts = await _contactRepository.FindAsync(c => c.UserId == userId && !c.IsDeleted);
            var contactsList = allContacts.ToList();

            // Group by email
            var emailGroups = contactsList
                .Where(c => !string.IsNullOrEmpty(c.Email))
                .GroupBy(c => c.Email!.ToLower())
                .Where(g => g.Count() > 1);

            foreach (var group in emailGroups)
            {
                var key = $"email:{group.Key}";
                if (!groups.ContainsKey(key))
                {
                    groups[key] = new DuplicateGroupDto
                    {
                        DuplicateKey = group.Key,
                        DuplicateType = "Email",
                        Count = group.Count(),
                        Contacts = group.Select(c => new DuplicateContactDto
                        {
                            ContactId = c.Id,
                            PhoneNumber = c.PhoneNumber,
                            Email = c.Email,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            CreatedAt = c.CreatedAt,
                            DuplicateReason = "Email"
                        }).ToList()
                    };
                }
            }

            // Group by phone number
            var phoneGroups = contactsList
                .Where(c => !string.IsNullOrEmpty(c.PhoneNumber))
                .GroupBy(c => c.PhoneNumber?.ToLower() ?? string.Empty)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key));

            foreach (var group in phoneGroups)
            {
                var key = $"phone:{group.Key}";
                if (!groups.ContainsKey(key))
                {
                    groups[key] = new DuplicateGroupDto
                    {
                        DuplicateKey = group.Key,
                        DuplicateType = "PhoneNumber",
                        Count = group.Count(),
                        Contacts = group.Select(c => new DuplicateContactDto
                        {
                            ContactId = c.Id,
                            PhoneNumber = c.PhoneNumber,
                            Email = c.Email,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            CreatedAt = c.CreatedAt,
                            DuplicateReason = "PhoneNumber"
                        }).ToList()
                    };
                }
            }

            report.DuplicateGroups = groups.Values.ToList();
            report.TotalDuplicates = report.DuplicateGroups.Sum(g => g.Count);
            return report;
        }

        public async Task<ResolveDuplicateResultDto> ResolveDuplicatesAsync(string userId, ResolveDuplicateDto dto)
        {
            var result = new ResolveDuplicateResultDto();

            try
            {
                // Validate primary contact
                var primaryContact = await _contactRepository.FirstOrDefaultAsync(c =>
                    c.Id == dto.PrimaryContactId && c.UserId == userId && !c.IsDeleted);

                if (primaryContact == null)
                {
                    result.Success = false;
                    result.Message = "Primary contact not found";
                    return result;
                }

                // Get duplicate contacts
                var duplicateContacts = await _contactRepository.FindAsync(c =>
                    dto.DuplicateContactIds.Contains(c.Id) && c.UserId == userId && !c.IsDeleted);

                if (!duplicateContacts.Any())
                {
                    result.Success = false;
                    result.Message = "No duplicate contacts found";
                    return result;
                }

                switch (dto.Action)
                {
                    case ResolutionAction.KeepPrimary:
                        // Just delete the duplicates
                        foreach (var contact in duplicateContacts)
                        {
                            contact.IsDeleted = true;
                            contact.UpdatedAt = DateTime.UtcNow;
                        }
                        _contactRepository.UpdateRange(duplicateContacts);
                        result.ContactsAffected = duplicateContacts.Count();
                        result.Message = $"Deleted {duplicateContacts.Count()} duplicate contact(s)";
                        break;

                    case ResolutionAction.MergeIntoPrimary:
                        // Merge data into primary (fill in missing fields)
                        // Note: PhoneNumber is required with default empty string, so we check for empty
                        foreach (var contact in duplicateContacts)
                        {
                            if (string.IsNullOrEmpty(primaryContact.Email) && !string.IsNullOrEmpty(contact.Email))
                                primaryContact.Email = contact.Email;
                            if (string.IsNullOrEmpty(primaryContact.PhoneNumber) && !string.IsNullOrEmpty(contact.PhoneNumber))
                                primaryContact.PhoneNumber = contact.PhoneNumber;
                            if (string.IsNullOrEmpty(primaryContact.FirstName) && !string.IsNullOrEmpty(contact.FirstName))
                                primaryContact.FirstName = contact.FirstName;
                            if (string.IsNullOrEmpty(primaryContact.LastName) && !string.IsNullOrEmpty(contact.LastName))
                                primaryContact.LastName = contact.LastName;
                            if (string.IsNullOrEmpty(primaryContact.Country) && !string.IsNullOrEmpty(contact.Country))
                                primaryContact.Country = contact.Country;
                            if (string.IsNullOrEmpty(primaryContact.City) && !string.IsNullOrEmpty(contact.City))
                                primaryContact.City = contact.City;
                            if (string.IsNullOrEmpty(primaryContact.PostalCode) && !string.IsNullOrEmpty(contact.PostalCode))
                                primaryContact.PostalCode = contact.PostalCode;

                            // Merge custom attributes
                            if (!string.IsNullOrEmpty(contact.CustomAttributes))
                            {
                                var primaryAttrs = DeserializeCustomAttributes(primaryContact.CustomAttributes) ?? new Dictionary<string, string>();
                                var contactAttrs = DeserializeCustomAttributes(contact.CustomAttributes) ?? new Dictionary<string, string>();

                                foreach (var attr in contactAttrs)
                                {
                                    if (!primaryAttrs.ContainsKey(attr.Key))
                                        primaryAttrs[attr.Key] = attr.Value;
                                }

                                primaryContact.CustomAttributes = SerializeCustomAttributes(primaryAttrs);
                            }

                            // Mark duplicate as deleted
                            contact.IsDeleted = true;
                            contact.UpdatedAt = DateTime.UtcNow;
                        }

                        primaryContact.UpdatedAt = DateTime.UtcNow;
                        _contactRepository.Update(primaryContact);
                        _contactRepository.UpdateRange(duplicateContacts);
                        result.ContactsAffected = duplicateContacts.Count() + 1;
                        result.Message = $"Merged {duplicateContacts.Count()} contact(s) into primary contact";
                        break;

                    case ResolutionAction.KeepAll:
                        // Do nothing, just acknowledge
                        result.ContactsAffected = 0;
                        result.Message = "All contacts kept as-is";
                        break;
                }

                await _unitOfWork.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error resolving duplicates: {ex.Message}";
                _logger.LogError(ex, "Error resolving duplicate contacts");
            }

            return result;
        }
    }
}
