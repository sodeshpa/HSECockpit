/**
 * Trend direction indicator with accessible labelling.
 * Props:
 *   direction - "UP" | "DOWN" | "STABLE"
 *
 * Interpretation:
 *   UP = degrading (risk increasing, shown in red)
 *   DOWN = improving (risk decreasing, shown in green)
 *   STABLE = no change (shown in gray)
 */

const directionConfig = {
  UP: {
    symbol: "\u2191",
    colorClass: "text-red-600",
    label: "Trend: degrading",
  },
  DOWN: {
    symbol: "\u2193",
    colorClass: "text-green-600",
    label: "Trend: improving",
  },
  STABLE: {
    symbol: "\u2192",
    colorClass: "text-gray-500",
    label: "Trend: stable",
  },
};

export function TrendArrow({ direction }) {
  const config = directionConfig[direction] || directionConfig.STABLE;

  return (
    <span
      className={`inline-flex items-center text-lg font-bold ${config.colorClass}`}
      aria-label={config.label}
      role="img"
    >
      {config.symbol}
    </span>
  );
}
