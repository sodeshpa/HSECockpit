import { LineChart, Line, ResponsiveContainer } from "recharts";
import { useBarrierTrend } from "../../hooks/useBarriers";

/**
 * A tiny sparkline showing barrier condition score over time.
 * Uses Recharts LineChart with no axes, labels, or grid — pure sparkline.
 *
 * Props:
 *   barrierId  - the barrier ID to fetch trend data for
 *   periodDays - number of days to look back (default: 90)
 */
export function BarrierSparkline({ barrierId, periodDays = 90 }) {
  const { data: trendData, isLoading, isError } = useBarrierTrend(
    barrierId,
    periodDays
  );

  if (isLoading) {
    return (
      <div
        className="h-[30px] w-[120px] animate-pulse rounded bg-gray-100"
        aria-label="Loading trend data"
      />
    );
  }

  if (isError || !trendData || trendData.length === 0) {
    return (
      <div
        className="h-[30px] w-[120px] rounded bg-gray-50"
        aria-label="No trend data available"
      />
    );
  }

  // Map trend points to chart data
  const chartData = trendData.map((point) => ({
    date: point.observedAt,
    score: point.conditionScore,
  }));

  return (
    <div
      className="h-[30px] w-[120px]"
      aria-label={`Barrier trend sparkline: ${chartData.length} observations over ${periodDays} days`}
      role="img"
    >
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData}>
          <Line
            type="monotone"
            dataKey="score"
            stroke="#3b82f6"
            strokeWidth={1.5}
            dot={false}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
