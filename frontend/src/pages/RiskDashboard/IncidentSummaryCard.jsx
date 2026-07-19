import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

/**
 * Incident Summary Card showing total incident count and a severity breakdown
 * as a stacked bar chart (Low/Medium/High/Critical).
 *
 * Props:
 *   data - { totalCount, severityBreakdown: { low, medium, high, critical } }
 *   isLoading - boolean
 *   isError - boolean
 */
export function IncidentSummaryCard({ data, isLoading, isError }) {
  if (isLoading) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Incident Summary</h2>
        <p className="mt-4 text-sm text-gray-500">Loading...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Incident Summary</h2>
        <p className="mt-4 text-sm text-red-600">Data unavailable</p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Incident Summary</h2>
        <p className="mt-4 text-sm text-gray-500">No data found</p>
      </div>
    );
  }

  const { totalCount, severityBreakdown } = data;
  const chartData = [
    {
      name: "Incidents",
      Low: severityBreakdown?.low || 0,
      Medium: severityBreakdown?.medium || 0,
      High: severityBreakdown?.high || 0,
      Critical: severityBreakdown?.critical || 0,
    },
  ];

  return (
    <article className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-gray-900">Incident Summary</h2>
      <p className="mt-1 text-3xl font-bold text-gray-900" aria-label={`Total incidents: ${totalCount}`}>
        {totalCount}
      </p>
      <p className="text-sm text-gray-500">Total incidents in period</p>

      <div className="mt-4" aria-label="Severity breakdown chart" role="img">
        <ResponsiveContainer width="100%" height={180}>
          <BarChart data={chartData} layout="vertical" margin={{ top: 5, right: 20, left: 20, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis type="number" />
            <YAxis type="category" dataKey="name" hide />
            <Tooltip />
            <Legend />
            <Bar dataKey="Low" stackId="severity" fill="#22c55e" name="Low" />
            <Bar dataKey="Medium" stackId="severity" fill="#f59e0b" name="Medium" />
            <Bar dataKey="High" stackId="severity" fill="#f97316" name="High" />
            <Bar dataKey="Critical" stackId="severity" fill="#ef4444" name="Critical" />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Accessible text breakdown for screen readers */}
      <dl className="sr-only">
        <dt>Low severity</dt>
        <dd>{severityBreakdown?.low || 0}</dd>
        <dt>Medium severity</dt>
        <dd>{severityBreakdown?.medium || 0}</dd>
        <dt>High severity</dt>
        <dd>{severityBreakdown?.high || 0}</dd>
        <dt>Critical severity</dt>
        <dd>{severityBreakdown?.critical || 0}</dd>
      </dl>
    </article>
  );
}
