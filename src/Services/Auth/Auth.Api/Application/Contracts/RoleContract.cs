using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
    public sealed record RoleResponse(Guid RoleId, string Name, string? Description);

    public sealed record CreateRoleCommand(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    string? Description);

    public sealed record UpdateRoleCommand(
        [Required, StringLength(100, MinimumLength = 2)] string Name,
        string? Description);
}
