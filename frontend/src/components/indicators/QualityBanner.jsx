import { useState } from "react";
import { AlertTriangle, X } from "lucide-react";

/**
 * QualityBanner — displays a warning banner when contributing records
 * have FLAGGED or CONFLICT data quality status.
 *
 * @param {object} props
 * @param {number} props.flaggedCount - Number of records with FLAGGED status
 * @param {number} props.conflictCount - Number of records with CONFLICT status
 * @param {number} props.totalCount - Total number of contributing records
 * @param {boolean} [props.dismissible=true] - Whether the banner can be dismissed
 * @param {function} [props.onDismiss] - Callback when banner is dismissed
 */
export function QualityBanner({
  flaggedCount = 0,
  conflictCount = 0,
  totalCount = 0,
  dismissible = true,
  onDismiss,
}) {
  const [dismissed, setDismissed] = useState(false);

  const hasIssues = flaggedCount > 0 || conflictCount > 0;

  if (!hasIssues || dismissed) {
    return null;
  }

  const handleDismiss = () => {
    setDismissed(true);
    if (onDismiss) {
      onDismiss();
    }
  };

  const issueDetails = buildIssueDetails(flaggedCount, conflictCount, totalCount);

  return (
    <div
      role="alert"
      aria-live="polite"
      className="flex items-center gap-3 rounded-md border border-amber-300 bg-amber-50 px-4 py-3 text-amber-900"
    >
      <AlertTriangle
        className="h-5 w-5 shrink-0 text-amber-600"
        aria-hidden="true"
      />
      <div className="flex-1">
        <p className="text-sm font-medium">
          Some data in this view has quality issues
        </p>
        <p className="text-xs text-amber-800">{issueDetails}</p>
      </div>
      {dismissible && (
        <button
          type="button"
          onClick={handleDismiss}
          className="inline-flex items-center rounded p-1 text-amber-600 hover:bg-amber-100 hover:text-amber-900 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-1"
          aria-label="Dismiss quality warning"
        >
          <X className="h-4 w-4" aria-hidden="true" />
        </button>
      )}
    </div>
  );
}

/**
 * Build a human-readable details string describing the quality issues.
 */
function buildIssueDetails(flaggedCount, conflictCount, totalCount) {
  const parts = [];

  if (flaggedCount > 0) {
    parts.push(`${flaggedCount} flagged`);
  }
  if (conflictCount > 0) {
    parts.push(`${conflictCount} conflicting`);
  }

  const issueText = parts.join(", ");
  const ofTotal = totalCount > 0 ? ` of ${totalCount} records` : " records";

  return `${issueText}${ofTotal}`;
}

export default QualityBanner;
