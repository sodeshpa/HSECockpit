import { Badge } from "../../components/ui/Badge";
import { MetricLabel } from "../../components/indicators/MetricLabel";

/**
 * Site Risk Score Card: displays the composite risk score for a selected site.
 * Shows score value (large number), risk band badge, factor breakdown, and data quality label.
 *
 * Props:
 *   data - { score, riskBand, factors: [{ name, value, weight }], dataQualityStatus }
 *   isLoading - boolean
 *   isError - boolean
 *   siteId - selected site (used for empty state messaging)
 */

const bandConfig = {
  LOW: { variant: "green", label: "Low" },
  MEDIUM: { variant: "amber", label: "Medium" },
  HIGH: { variant: "red", label: "High" },
  CRITICAL: { variant: "red", label: "Critical" },
};

export function SiteRiskScoreCard({ data, isLoading, isError, siteId }) {
  if (!siteId) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Site Risk Score</h2>
        <p className="mt-4 text-sm text-gray-500">Select a site to view risk score</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Site Risk Score</h2>
        <p className="mt-4 text-sm text-gray-500">Loading...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Site Risk Score</h2>
        <p className="mt-4 text-sm text-red-600">Data unavailable</p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Site Risk Score</h2>
        <p className="mt-4 text-sm text-gray-500">No data found</p>
      </div>
    );
  }

  const { score, riskBand, factors, dataQualityStatus } = data;
  const config = riskBand ? bandConfig[riskBand] : null;

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-900">Site Risk Score</h2>
        {dataQualityStatus && <MetricLabel status={dataQualityStatus} />}
      </div>

      <div className="mt-3 flex items-baseline gap-3">
        <p className="text-4xl font-bold text-gray-900" aria-label={`Risk score: ${score}`}>
          {score}
        </p>
        {config && (
          <Badge variant={config.variant} aria-label={`Risk band: ${config.label}`}>
            {config.label}
          </Badge>
        )}
      </div>
      <p className="text-sm text-gray-500 mt-1">out of 100</p>

      {factors && factors.length > 0 && (
        <div className="mt-4">
          <h3 className="text-sm font-medium text-gray-700 mb-2">Contributing Factors</h3>
          <ul className="space-y-1">
            {factors.map((factor) => (
              <li
                key={factor.name}
                className="flex items-center justify-between text-sm text-gray-600"
              >
                <span>{factor.name}</span>
                <span className="font-medium text-gray-900">
                  {factor.value}{factor.weight ? ` (${Math.round(factor.weight * 100)}%)` : ""}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </article>
  );
}
