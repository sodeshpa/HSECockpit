using D4HSE.Ingestion.Models;
using FluentValidation;

namespace D4HSE.Ingestion.Validators;

/// <summary>
/// FluentValidation rules for barrier inspection source records.
/// Validation messages are business-readable for display in data quality views.
/// </summary>
public class BarrierInspectionRecordValidator : AbstractValidator<BarrierInspectionRecord>
{
    private static readonly string[] ValidRagStatuses = ["GREEN", "AMBER", "RED"];

    public BarrierInspectionRecordValidator()
    {
        RuleFor(x => x.BarrierId)
            .NotEmpty()
            .WithMessage("Barrier ID is required for each inspection record.")
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.BarrierId))
            .WithMessage("Barrier ID must be a valid identifier (UUID format).");

        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required to associate the inspection with a location.")
            .Must(BeAValidGuid)
            .When(x => !string.IsNullOrWhiteSpace(x.SiteId))
            .WithMessage("Site ID must be a valid identifier (UUID format).");

        RuleFor(x => x.ObservedAt)
            .NotEmpty()
            .WithMessage("Observation date is required for each barrier inspection.")
            .Must(BeAValidDate)
            .When(x => !string.IsNullOrWhiteSpace(x.ObservedAt))
            .WithMessage("Observation date must be a valid date in ISO format (yyyy-MM-dd).")
            .Must(NotBeInTheFuture)
            .When(x => !string.IsNullOrWhiteSpace(x.ObservedAt) && BeAValidDate(x.ObservedAt))
            .WithMessage("Observation date cannot be in the future.");

        RuleFor(x => x.RagStatus)
            .NotEmpty()
            .WithMessage("RAG status is required (must be GREEN, AMBER, or RED).")
            .Must(BeAValidRagStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.RagStatus))
            .WithMessage("RAG status must be one of: GREEN, AMBER, RED.");

        RuleFor(x => x.ConditionScore)
            .InclusiveBetween(0m, 100m)
            .When(x => x.ConditionScore.HasValue)
            .WithMessage("Condition score must be between 0 and 100.");
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

    private static bool BeAValidRagStatus(string? value)
    {
        return value is not null && ValidRagStatuses.Contains(value.ToUpperInvariant());
    }
}
