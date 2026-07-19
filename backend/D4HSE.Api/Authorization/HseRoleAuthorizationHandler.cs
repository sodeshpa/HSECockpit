using Microsoft.AspNetCore.Authorization;

namespace D4HSE.Api.Authorization;

/// <summary>
/// Handles HseRoleRequirement by checking the user's "hse_role" claim against the allowed roles.
/// The "admin" role always satisfies any role requirement.
/// </summary>
public class HseRoleAuthorizationHandler : AuthorizationHandler<HseRoleRequirement>
{
    public const string HseRoleClaimType = "hse_role";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HseRoleRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(HseRoleClaimType)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Task.CompletedTask;
        }

        if (requirement.AllowedRoles.Contains(roleClaim, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
