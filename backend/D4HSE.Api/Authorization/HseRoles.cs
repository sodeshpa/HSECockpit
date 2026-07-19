namespace D4HSE.Api.Authorization;

/// <summary>
/// Constants for HSE role names matching the Cognito custom:role attribute values.
/// </summary>
public static class HseRoles
{
    public const string HseManager = "hse-manager";
    public const string DataOwner = "hse-data-owner";
    public const string Executive = "executive";
    public const string Admin = "admin";
}
