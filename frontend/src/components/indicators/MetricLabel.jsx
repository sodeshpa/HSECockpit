/**
 * MetricLabel — A badge component that indicates data freshness/completeness
 * for a given metric.
 *
 * Displays one of four statuses:
 * - "Fresh" (green) — data is recent and complete
 * - "Partial" (amber) — some data categories have quality issues
 * - "Stale" (grey) — data is beyond freshness threshold
 * - "No Data" (grey/red) — no data available for this metric
 *
 * Follows shadcn/ui Badge-like patterns with Tailwind CSS.
 * Accessible: uses text labels (not colour-only) and appropriate aria attributes.
 */

const STATUS_CONFIG = {
  FRESH: {
    label: "Fresh",
    classes:
      "bg-green-100 text-green-800 border-green-300",
  },
  PARTIAL: {
    label: "Partial",
    classes:
      "bg-amber-100 text-amber-800 border-amber-300",
  },
  STALE: {
    label: "Stale",
    classes:
      "bg-gray-100 text-gray-700 border-gray-300",
  },
  NO_DATA: {
    label: "No Data",
    classes:
      "bg-red-50 text-red-700 border-red-300",
  },
};

/**
 * @param {Object} props
 * @param {"FRESH" | "PARTIAL" | "STALE" | "NO_DATA"} props.status - The data quality status
 * @param {string} [props.className] - Additional CSS classes to apply
 */
export function MetricLabel({ status, className = "" }) {
  const config = STATUS_CONFIG[status];

  if (!config) {
    return null;
  }

  return (
    <span
      role="status"
      aria-label={`Data status: ${config.label}`}
      className={`inline-flex items-center rounded-md border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 ${config.classes} ${className}`.trim()}
    >
      {config.label}
    </span>
  );
}
