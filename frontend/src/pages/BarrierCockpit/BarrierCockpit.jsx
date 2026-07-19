import { useState, useCallback } from "react";
import { SiteAssetFilter } from "../../components/filters/SiteAssetFilter";
import { BarrierList } from "./BarrierList";

/**
 * BarrierCockpit page container.
 * Renders the SiteAssetFilter at top and BarrierList below,
 * passing filter state to the list.
 */
export function BarrierCockpit() {
  const [filters, setFilters] = useState({ siteId: null, assetId: null });

  const handleFilterChange = useCallback(({ siteId, assetId }) => {
    setFilters({ siteId, assetId });
  }, []);

  return (
    <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
      <header className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">
          Critical Barriers Cockpit
        </h1>
        <p className="mt-1 text-sm text-gray-600">
          Monitor barrier health status across sites and assets.
        </p>
      </header>

      <div className="mb-6">
        <SiteAssetFilter onFilterChange={handleFilterChange} />
      </div>

      <BarrierList siteId={filters.siteId} assetId={filters.assetId} />
    </div>
  );
}
