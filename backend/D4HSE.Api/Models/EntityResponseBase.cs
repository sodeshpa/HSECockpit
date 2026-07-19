namespace D4HSE.Api.Models;

/// <summary>
/// Base class for all entity API responses.
/// Includes the data quality status so the UI can surface quality labels.
/// </summary>
public abstract class EntityResponseBase
{
    /// <summary>
    /// Indicates the data quality status of this record.
    /// Values: "VALID", "FLAGGED", "CONFLICT"
    /// </summary>
    public string DataQualityStatus { get; set; } = "VALID";
}
