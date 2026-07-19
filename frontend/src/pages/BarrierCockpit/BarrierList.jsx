import { AlertTriangle } from "lucide-react";
import { useBarriers, useDegradedBarriers } from "../../hooks/useBarriers";
import { RagBadge } from "../../components/indicators/RagBadge";
import { TrendArrow } from "../../components/indicators/TrendArrow";
import { BarrierSparkline } from "../../components/indicators/BarrierSparkline";

/**
 * Computes trend direction by comparing the latest observation to the prior one.
 * Returns "UP" (degrading), "DOWN" (improving), or "STABLE".
 */
function computeTrendDirection(barrier) {
  if (!barrier.observations || barrier.observations.length < 2) {
    return "STABLE";
  }

  const ragOrder = { GREEN: 0, AMBER: 1, RED: 2 };
  const latest = ragOrder[barrier.observations[0]?.ragStatus] ?? 0;
  const prior = ragOrder[barrier.observations[1]?.ragStatus] ?? 0;

  if (latest > prior) return "UP";
  if (latest < prior) return "DOWN";
  return "STABLE";
}

/**
 * Formats an ISO date string to a user-friendly locale date.
 */
function formatDate(dateStr) {
  if (!dateStr) return "N/A";
  return new Date(dateStr).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

/**
 * Returns Tailwind background classes for degraded barrier rows.
 */
function getDegradedRowClasses(barrier, degradedIds) {
  if (!degradedIds.has(barrier.id)) return "";

  if (barrier.currentStatus === "RED") return "bg-red-50";
  if (barrier.currentStatus === "AMBER") return "bg-amber-50";
  return "";
}

/**
 * BarrierList displays a list of barriers with RAG badge, trend arrow,
 * sparkline, and last assessed date. Degraded barriers are highlighted
 * with an amber or red background tint and a degradation flag icon.
 *
 * Props:
 *   siteId - optional site filter
 *   assetId - optional asset filter
 */
export function BarrierList({ siteId, assetId }) {
  const { data: barriers, isLoading, isError, error } = useBarriers(siteId, assetId);
  const { data: degradedBarriers } = useDegradedBarriers(siteId);

  // Build a Set of degraded barrier IDs for O(1) lookups
  const degradedIds = new Set(
    (degradedBarriers || []).map((b) => b.id || b.barrierId)
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" role="status">
        <span className="text-gray-500">Loading barriers...</span>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-md bg-red-50 p-4 text-red-700" role="alert">
        Failed to load barriers: {error?.message || "Unknown error"}
      </div>
    );
  }

  if (!barriers || barriers.length === 0) {
    return (
      <div className="rounded-md bg-gray-50 p-6 text-center text-gray-600">
        No barriers found for the selected filters.
      </div>
    );
  }

  return (
    <section aria-label="Barrier list">
      <ul className="divide-y divide-gray-200 rounded-lg border border-gray-200 bg-white shadow-sm">
        {barriers.map((barrier) => {
          const isDegraded = degradedIds.has(barrier.id);
          const rowBgClass = getDegradedRowClasses(barrier, degradedIds);

          return (
            <li
              key={barrier.id}
              className={`flex flex-wrap items-center gap-4 px-4 py-3 hover:bg-gray-50 focus-within:ring-2 focus-within:ring-blue-500 ${rowBgClass}`}
              tabIndex={0}
            >
              {/* Barrier name, type, and degradation flag */}
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-1.5">
                  {isDegraded && (
                    <AlertTriangle
                      className="h-4 w-4 shrink-0 text-amber-600"
                      aria-label="Degraded barrier"
                    />
                  )}
                  <p className="truncate text-sm font-medium text-gray-900">
                    {barrier.name}
                  </p>
                </div>
                {barrier.type && (
                  <p className="text-xs text-gray-500">{barrier.type}</p>
                )}
              </div>

              {/* Site / Asset info */}
              <div className="text-xs text-gray-500 w-32 truncate">
                {barrier.siteName}
                {barrier.assetName && ` / ${barrier.assetName}`}
              </div>

              {/* RAG Badge */}
              <div className="w-20">
                <RagBadge status={barrier.currentStatus} />
              </div>

              {/* Trend Arrow */}
              <div className="w-8 text-center">
                <TrendArrow direction={computeTrendDirection(barrier)} />
              </div>

              {/* Trend Sparkline */}
              <div className="w-[120px]">
                <BarrierSparkline barrierId={barrier.id} periodDays={90} />
              </div>

              {/* Last assessed date */}
              <div className="w-28 text-xs text-gray-500">
                {formatDate(barrier.lastAssessedAt)}
              </div>

              {/* Criticality rank */}
              <div className="w-16 text-right">
                <span className="inline-flex items-center rounded bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-700">
                  Rank {barrier.criticalityRank}
                </span>
              </div>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
