using AdotaPet.Application.Accounts;
using AdotaPet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace AdotaPet.Api.Auth;

public static class ProfileExistenceValidator
{
    /// <summary>
    /// Fails authentication (401) when the token's profile no longer exists, e.g. a token issued before the
    /// database was reset. Without it the first write would hit a foreign-key error and answer 500; the 401
    /// makes the SPA drop the stale session. Costs one indexed lookup per authenticated request.
    /// </summary>
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var role = principal?.FindFirst("role")?.Value;
        if (!Guid.TryParse(principal?.FindFirst("profile_id")?.Value, out var profileId))
        {
            context.Fail("Missing or invalid profile_id claim.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var exists = role switch
        {
            Roles.Shelter => await db.Shelters.AnyAsync(s => s.Id == profileId, context.HttpContext.RequestAborted),
            Roles.Adopter => await db.Adopters.AnyAsync(a => a.Id == profileId, context.HttpContext.RequestAborted),
            _ => false,
        };

        if (!exists)
            context.Fail("The profile in the token no longer exists.");
    }
}
