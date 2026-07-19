using D4HSE.Ingestion.Models;
using FluentValidation;

namespace D4HSE.Ingestion.Validators;

/// <summary>
/// FluentValidation rules for incident/near-miss source records.
/// Validation messages are business-readable for display in data quality views.
/// </summary>
public class IncidentSourceRecordValidator : AbstractValidator<IncidentSourceRecord>
{
    private static readonly string[] ValidSeverities = ["LOW", "MEDIUM", "HIGH", "CRITICAL"];

    public IncidentSourceRecordValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required to associate the incident with a location.")
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.SiteId))
            .WithMessage("Site ID must be a valid identifier (UUID format).");

        RuleFor(x => x.IncidentDate)
            .NotEmpty()
            .WithMessage("Incident date is required for each incident/near-miss record.")
            .Must(BeAValidDate)
            .When(x => !string.IsNullOrWhiteSpace(x.IncidentDate))
            .WithMessage("Incident date must be a valid date in ISO format (yyyy-MM-dd).")
            .Must(NotBeInTheFuture)
            .When(x => !string.IsNullOrWhiteSpace(x.IncidentDate) && BeAValidDate(x.IncidentDate))
            .WithMessage("Incident date cannot be in the future.");

        RuleFor(x => x.Severity)
            .NotEmpty()
            .WithMessage("Severity is required (must be LOW, MEDIUM, HIGH, or CRITICAL).")
            .Must(BeAValidSeverity)
            .When(x => !string.IsNullOrWhiteSpace(x.Severity))
            .WithMessage("Severity must be one of: LOW, MEDIUM, HIGH, CRITICAL.");

        RuleFor(x => x.AssetId)
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.AssetId))
            .WithMessage("Asset ID must be a valid identifier (UUID format) when provided.");

        RuleFor(x => x.IncidentType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.IncidentType))
            .WithMessage("Incident type must not exceed 100 characters.");
    }

    private static bool BeAValidGuid(string? value)
    {
        return Guid.TryParse(value, out _);
    }

    private static bool BeAValidDate(string? value)
    {
        return DateOnly.TryParse(value, out _);
    }

    private static bool NotBeInTheFuture(string? value)
    {
        if (DateOnly.TryParse(value, out var date))
        {
            return date <= DateOnly.FromDateTime(DateTime.UtcNow);
        }
        return false;
    }

    private static bool BeAValidSeverity(string? value)
    {
        return value is not null && ValidSeverities.Contains(value.ToUpperInvariant());
    }
}
