using AdotaPet.Application.Accounts;
using AdotaPet.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace AdotaPet.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IIdentityService
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["PasswordTooShort"] = "A senha deve ter pelo menos 8 caracteres.",
        ["PasswordRequiresDigit"] = "A senha deve conter pelo menos um número.",
        ["PasswordRequiresLower"] = "A senha deve conter pelo menos uma letra minúscula.",
        ["InvalidEmail"] = "E-mail inválido.",
    };

    public async Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct)
    {
        var user = new ApplicationUser { UserName = email, Email = email };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw Translate(result.Errors);

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                "Failed to assign role: " + string.Join("; ", roleResult.Errors.Select(e => e.Code)));

        return user.Id;
    }

    public async Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return null;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        return role is null ? null : new IdentityUserInfo(user.Id, user.Email!, role);
    }

    private static DomainException Translate(IEnumerable<IdentityError> errors)
    {
        var list = errors.ToList();
        if (list.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            return new ConflictException("Já existe uma conta com este e-mail.");

        var messages = list
            .Select(e => Messages.TryGetValue(e.Code, out var m) ? m : "Dados da conta inválidos.")
            .Distinct();
        return new DomainValidationException(string.Join(" ", messages));
    }
}
