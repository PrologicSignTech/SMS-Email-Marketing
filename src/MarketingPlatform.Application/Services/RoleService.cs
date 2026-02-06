using AutoMapper;
using MarketingPlatform.Application.DTOs.Role;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using MarketingPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IMapper mapper,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;

            var roleDto = _mapper.Map<RoleDto>(role);
            roleDto.PermissionNames = GetPermissionNames(role.Permissions);
            roleDto.UserCount = role.UserRoles.Count;
            return roleDto;
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string name)
        {
            var role = await _roleRepository.GetByNameAsync(name);
            if (role == null) return null;

            var roleDto = _mapper.Map<RoleDto>(role);
            roleDto.PermissionNames = GetPermissionNames(role.Permissions);
            roleDto.UserCount = role.UserRoles.Count;
            return roleDto;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(role =>
            {
                var roleDto = _mapper.Map<RoleDto>(role);
                roleDto.PermissionNames = GetPermissionNames(role.Permissions);
                roleDto.UserCount = role.UserRoles.Count;
                return roleDto;
            });
        }

        public async Task<IEnumerable<RoleDto>> GetActiveRolesAsync()
        {
            var roles = await _roleRepository.GetActiveRolesAsync();
            return roles.Select(role =>
            {
                var roleDto = _mapper.Map<RoleDto>(role);
                roleDto.PermissionNames = GetPermissionNames(role.Permissions);
                roleDto.UserCount = role.UserRoles.Count;
                return roleDto;
            });
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto, string createdBy)
        {
            // Check if role already exists
            if (await _roleRepository.ExistsAsync(createRoleDto.Name))
            {
                throw new InvalidOperationException($"Role with name '{createRoleDto.Name}' already exists.");
            }

            // Convert permission names to flags
            var permissionFlags = ConvertPermissionNamesToFlags(createRoleDto.Permissions);

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                Permissions = permissionFlags,
                IsSystemRole = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdRole = await _roleRepository.CreateAsync(role);
            _logger.LogInformation("Role {RoleName} created by {CreatedBy}", createdRole.Name, createdBy);

            var roleDto = _mapper.Map<RoleDto>(createdRole);
            roleDto.PermissionNames = GetPermissionNames(createdRole.Permissions);
            roleDto.UserCount = 0;
            return roleDto;
        }

        public async Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {id} not found.");
            }

            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot modify system roles.");
            }

            if (updateRoleDto.Description != null)
            {
                role.Description = updateRoleDto.Description;
            }

            if (updateRoleDto.Permissions != null)
            {
                role.Permissions = ConvertPermissionNamesToFlags(updateRoleDto.Permissions);
            }

            if (updateRoleDto.IsActive.HasValue)
            {
                role.IsActive = updateRoleDto.IsActive.Value;
            }

            var updatedRole = await _roleRepository.UpdateAsync(role);
            _logger.LogInformation("Role {RoleName} updated", updatedRole.Name);

            var roleDto = _mapper.Map<RoleDto>(updatedRole);
            roleDto.PermissionNames = GetPermissionNames(updatedRole.Permissions);
            roleDto.UserCount = updatedRole.UserRoles.Count;
            return roleDto;
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {id} not found.");
            }

            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot delete system roles.");
            }

            if (role.UserRoles.Any())
            {
                throw new InvalidOperationException("Cannot delete role with assigned users. Remove all users first.");
            }

            await _roleRepository.DeleteAsync(id);
            _logger.LogInformation("Role {RoleName} deleted", role.Name);
        }

        public async Task<IEnumerable<UserRoleDto>> GetUsersInRoleAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {roleId} not found.");
            }

            var users = await _userRoleRepository.GetUsersInRoleAsync(roleId);
            if (!users.Any())
            {
                return Enumerable.Empty<UserRoleDto>();
            }

            var userRoles = await _userRoleRepository.GetUserRolesAsync(users.First().Id);

            return users.Select(user =>
            {
                var userRole = userRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == roleId);
                return new UserRoleDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleId = roleId,
                    RoleName = role.Name,
                    AssignedAt = userRole?.AssignedAt ?? DateTime.UtcNow,
                    AssignedBy = userRole?.AssignedBy
                };
            });
        }

        public async Task AssignRoleToUserAsync(string userId, int roleId, string assignedBy)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {roleId} not found.");
            }

            if (await _userRoleRepository.UserHasRoleAsync(userId, roleId))
            {
                throw new InvalidOperationException("User already has this role.");
            }

            var userRole = new Core.Entities.UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            };

            await _userRoleRepository.AssignRoleToUserAsync(userRole);
            _logger.LogInformation("Role {RoleName} assigned to user {UserId} by {AssignedBy}", 
                role.Name, userId, assignedBy);
        }

        public async Task RemoveRoleFromUserAsync(string userId, int roleId)
        {
            if (!await _userRoleRepository.UserHasRoleAsync(userId, roleId))
            {
                throw new InvalidOperationException("User does not have this role.");
            }

            await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
            _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
        {
            var permissions = await _userRoleRepository.GetUserPermissionsAsync(userId);
            return GetPermissionNames(permissions);
        }

        public async Task<bool> UserHasPermissionAsync(string userId, Permission permission)
        {
            var userPermissions = await _userRoleRepository.GetUserPermissionsAsync(userId);
            return (userPermissions & (long)permission) != 0;
        }

        private List<string> GetPermissionNames(long permissionFlags)
        {
            var permissions = new List<string>();
            foreach (Permission permission in Enum.GetValues(typeof(Permission)))
            {
                if (permission != Permission.All && (permissionFlags & (long)permission) != 0)
                {
                    permissions.Add(permission.ToString());
                }
            }
            return permissions;
        }

        private long ConvertPermissionNamesToFlags(List<string> permissionNames)
        {
            long flags = 0;
            foreach (var name in permissionNames)
            {
                if (Enum.TryParse<Permission>(name, out var permission))
                {
                    flags |= (long)permission;
                }
            }
            return flags;
        }
    }
}
