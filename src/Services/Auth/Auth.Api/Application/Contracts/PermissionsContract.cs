using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
    public sealed record PermissionResponse(Guid PermissionId, string Code, string? Description);

    public sealed record CreatePermissionCommand(
        [Required, StringLength(150, MinimumLength = 2)] string Code,
        string? Description);

    public sealed record UpdatePermissionCommand(
        [Required, StringLength(150, MinimumLength = 2)] string Code,
        string? Description);
}
