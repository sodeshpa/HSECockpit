import { Badge } from "../ui/Badge";

/**
 * RAG-specific badge that maps status to a coloured Badge with text label.
 * Props:
 *   status - "GREEN" | "AMBER" | "RED" | null
 */

const statusConfig = {
  GREEN: { variant: "green", label: "Green" },
  AMBER: { variant: "amber", label: "Amber" },
  RED: { variant: "red", label: "Red" },
};

export function RagBadge({ status }) {
  const config = status ? statusConfig[status] : null;

  if (!config) {
    return (
      <Badge variant="gray" aria-label="No data available">
        No Data
      </Badge>
    );
  }

  return (
    <Badge variant={config.variant} aria-label={`Status: ${config.label}`}>
      {config.label}
    </Badge>
  );
}
