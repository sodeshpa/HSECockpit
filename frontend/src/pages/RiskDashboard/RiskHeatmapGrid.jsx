import { Badge } from "../../components/ui/Badge";

/**
 * Risk Heatmap Grid: displays sites with their risk band as colour-coded badges.
 * Uses accessible text labels on all colour indicators.
 *
 * Props:
 *   data - Array of { siteId, siteName, riskBand, riskScore }
 *   isLoading - boolean
 *   isError - boolean
 */

const bandConfig = {
  LOW: { variant: "green", label: "Low" },
  MEDIUM: { variant: "amber", label: "Medium" },
  HIGH: { variant: "red", label: "High" },
  CRITICAL: { variant: "red", label: "Critical" },
};

function RiskBandBadge({ band }) {
  const config = band ? bandConfig[band] : null;

  if (!config) {
    return (
      <Badge variant="gray" aria-label="Risk band: Unknown">
        Unknown
      </Badge>
    );
  }

  return (
    <Badge variant={config.variant} aria-label={`Risk band: ${config.label}`}>
      {config.label}
    </Badge>
  );
}

export function RiskHeatmapGrid({ data, isLoading, isError }) {
  if (isLoading) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Risk Heatmap</h2>
        <p className="mt-4 text-sm text-gray-500">Loading...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Risk Heatmap</h2>
        <p className="mt-4 text-sm text-red-600">Data unavailable</p>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Risk Heatmap</h2>
        <p className="mt-4 text-sm text-gray-500">No data found</p>
      </div>
    );
  }

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Risk Heatmap</h2>

      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left" aria-label="Site risk heatmap">
          <thead>
            <tr className="border-b border-gray-200">
              <th scope="col" className="py-2 px-3 font-medium text-gray-700">
                Site
              </th>
              <th scope="col" className="py-2 px-3 font-medium text-gray-700">
                Risk Score
              </th>
              <th scope="col" className="py-2 px-3 font-medium text-gray-700">
                Risk Band
              </th>
            </tr>
          </thead>
          <tbody>
            {data.map((entry) => (
              <tr
                key={entry.siteId}
                className="border-b border-gray-100 hover:bg-gray-50"
              >
                <td className="py-2 px-3 text-gray-900">{entry.siteName}</td>
                <td className="py-2 px-3 text-gray-900">{entry.riskScore}</td>
                <td className="py-2 px-3">
                  <RiskBandBadge band={entry.riskBand} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </article>
  );
}
