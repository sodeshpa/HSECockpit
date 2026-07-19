using Microsoft.AspNetCore.Authorization;

namespace D4HSE.Api.Authorization;

/// <summary>
/// Authorization requirement that checks for specific HSE roles from the Cognito custom:role claim.
/// </summary>
public class HseRoleRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string> AllowedRoles { get; }

    public HseRoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}
