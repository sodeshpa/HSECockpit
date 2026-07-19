namespace D4HSE.Api.Authorization;

/// <summary>
/// Authorization policy names for use with [Authorize(Policy = ...)] attributes.
/// </summary>
public static class PolicyNames
{
    public const string HseManager = "HseManager";
    public const string DataOwner = "DataOwner";
    public const string Executive = "Executive";
    public const string Admin = "Admin";
}
