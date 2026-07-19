import { TrendArrow } from "../../components/indicators/TrendArrow";

/**
 * Near-Miss Summary Card showing total count, prior period count,
 * percentage change, and trend direction arrow.
 *
 * Props:
 *   data - { totalCount, priorPeriodCount, percentageChange, trendDirection }
 *   isLoading - boolean
 *   isError - boolean
 */
export function NearMissSummaryCard({ data, isLoading, isError }) {
  if (isLoading) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Near-Miss Summary</h2>
        <p className="mt-4 text-sm text-gray-500">Loading...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Near-Miss Summary</h2>
        <p className="mt-4 text-sm text-red-600">Data unavailable</p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Near-Miss Summary</h2>
        <p className="mt-4 text-sm text-gray-500">No data found</p>
      </div>
    );
  }

  const { totalCount, priorPeriodCount, percentageChange, trendDirection } = data;

  // Determine trend direction if not provided by API
  const direction =
    trendDirection ||
    (percentageChange > 0 ? "UP" : percentageChange < 0 ? "DOWN" : "STABLE");

  const formattedChange = percentageChange != null
    ? `${percentageChange > 0 ? "+" : ""}${percentageChange.toFixed(1)}%`
    : "N/A";

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-gray-900">Near-Miss Summary</h2>

      <div className="mt-2 flex items-baseline gap-3">
        <p className="text-3xl font-bold text-gray-900" aria-label={`Total near misses: ${totalCount}`}>
          {totalCount}
        </p>
        <TrendArrow direction={direction} />
        <span className="text-sm font-medium text-gray-600" aria-label={`Change: ${formattedChange}`}>
          {formattedChange}
        </span>
      </div>

      <p className="mt-1 text-sm text-gray-500">
        Prior period: {priorPeriodCount ?? "N/A"}
      </p>
    </article>
  );
}
