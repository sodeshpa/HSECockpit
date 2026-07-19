using D4HSE.Ingestion.Models;
using FluentValidation;

namespace D4HSE.Ingestion.Validators;

/// <summary>
/// FluentValidation rules for maintenance source records.
/// Validation messages are business-readable for display in data quality views.
/// </summary>
public class MaintenanceSourceRecordValidator : AbstractValidator<MaintenanceSourceRecord>
{
    public MaintenanceSourceRecordValidator()
    {
        RuleFor(x => x.AssetId)
            .NotEmpty()
            .WithMessage("Asset ID is required to associate the maintenance record with an asset.")
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.AssetId))
            .WithMessage("Asset ID must be a valid identifier (UUID format).");

        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required to associate the maintenance record with a location.")
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.SiteId))
            .WithMessage("Site ID must be a valid identifier (UUID format).");

        RuleFor(x => x.MaintenanceDate)
            .NotEmpty()
            .WithMessage("Maintenance date is required for each maintenance record.")
            .Must(BeAValidDate)
            .When(x => !string.IsNullOrWhiteSpace(x.MaintenanceDate))
            .WithMessage("Maintenance date must be a valid date in ISO format (yyyy-MM-dd).")
            .Must(NotBeInTheFuture)
            .When(x => !string.IsNullOrWhiteSpace(x.MaintenanceDate) && BeAValidDate(x.MaintenanceDate))
            .WithMessage("Maintenance date cannot be in the future.");

        RuleFor(x => x.ActivityType)
            .NotEmpty()
            .WithMessage("Activity type is required for each maintenance record.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ActivityType))
            .WithMessage("Activity type must not exceed 100 characters.");

        RuleFor(x => x.Outcome)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Outcome))
            .WithMessage("Outcome must not exceed 100 characters.");
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
}
