import { useCallback } from "react";

/**
 * Date range picker with accessible HTML date inputs styled with Tailwind.
 * Props:
 *   fromDate - Current start date string (yyyy-mm-dd) or ""
 *   toDate - Current end date string (yyyy-mm-dd) or ""
 *   onDateChange({ fromDate, toDate }) - Callback when either date changes
 */
export function DateRangePicker({ fromDate = "", toDate = "", onDateChange }) {
  const handleFromChange = useCallback(
    (e) => {
      onDateChange({ fromDate: e.target.value || null, toDate: toDate || null });
    },
    [onDateChange, toDate]
  );

  const handleToChange = useCallback(
    (e) => {
      onDateChange({ fromDate: fromDate || null, toDate: e.target.value || null });
    },
    [onDateChange, fromDate]
  );

  return (
    <div className="flex flex-wrap items-end gap-4 p-4 bg-white rounded-lg shadow-sm border border-gray-200">
      <div className="flex flex-col gap-1">
        <label
          htmlFor="date-from"
          className="text-sm font-medium text-gray-700"
        >
          From
        </label>
        <input
          type="date"
          id="date-from"
          value={fromDate}
          onChange={handleFromChange}
          max={toDate || undefined}
          className="block w-44 rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          aria-label="Start date"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor="date-to"
          className="text-sm font-medium text-gray-700"
        >
          To
        </label>
        <input
          type="date"
          id="date-to"
          value={toDate}
          onChange={handleToChange}
          min={fromDate || undefined}
          className="block w-44 rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          aria-label="End date"
        />
      </div>
    </div>
  );
}
