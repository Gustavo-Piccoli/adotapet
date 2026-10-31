using System.Security.Claims;

namespace AdotPet.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("sub") ?? throw new InvalidOperationException("Missing 'sub' claim.");

    public static Guid GetProfileId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("profile_id"), out var id)
            ? id
            : throw new InvalidOperationException("Missing or invalid 'profile_id' claim.");

    public static string GetRole(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("role") ?? throw new InvalidOperationException("Missing 'role' claim.");
}
