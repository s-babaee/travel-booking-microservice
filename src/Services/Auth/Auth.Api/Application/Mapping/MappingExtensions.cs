using Auth.Api.Application.Contracts;
using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Mapping;

public static class MappingExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Status,
            user.IsDeleted);
    }

    public static RoleResponse ToResponse(this Role role)
    {
        return new RoleResponse(role.Id, role.Name, role.Description);
    }

    public static PermissionResponse ToResponse(this Permission permission)
    {
        return new PermissionResponse(permission.Id, permission.Code, permission.Description);
    }
}
