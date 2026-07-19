import { useState, useCallback } from "react";
import { useSites, useAssets } from "../../hooks/useBarriers";

/**
 * Cascading site/asset filter bar.
 * - Site dropdown populated from API
 * - Asset dropdown enabled only when a site is selected
 * - When site changes, asset resets to "All"
 * - Calls onFilterChange({ siteId, assetId }) when selection changes
 */
export function SiteAssetFilter({ onFilterChange }) {
  const [selectedSiteId, setSelectedSiteId] = useState("");
  const [selectedAssetId, setSelectedAssetId] = useState("");

  const { data: sites, isLoading: sitesLoading } = useSites();
  const { data: assets, isLoading: assetsLoading } = useAssets(
    selectedSiteId || undefined
  );

  const handleSiteChange = useCallback(
    (e) => {
      const siteId = e.target.value;
      setSelectedSiteId(siteId);
      setSelectedAssetId("");
      onFilterChange({ siteId: siteId || null, assetId: null });
    },
    [onFilterChange]
  );

  const handleAssetChange = useCallback(
    (e) => {
      const assetId = e.target.value;
      setSelectedAssetId(assetId);
      onFilterChange({
        siteId: selectedSiteId || null,
        assetId: assetId || null,
      });
    },
    [onFilterChange, selectedSiteId]
  );

  return (
    <div className="flex flex-wrap items-end gap-4 p-4 bg-white rounded-lg shadow-sm border border-gray-200">
      <div className="flex flex-col gap-1">
        <label
          htmlFor="site-filter"
          className="text-sm font-medium text-gray-700"
        >
          Site
        </label>
        <select
          id="site-filter"
          value={selectedSiteId}
          onChange={handleSiteChange}
          disabled={sitesLoading}
          className="block w-48 rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
        >
          <option value="">All Sites</option>
          {sites?.map((site) => (
            <option key={site.id} value={site.id}>
              {site.name}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor="asset-filter"
          className="text-sm font-medium text-gray-700"
        >
          Asset
        </label>
        <select
          id="asset-filter"
          value={selectedAssetId}
          onChange={handleAssetChange}
          disabled={!selectedSiteId || assetsLoading}
          aria-disabled={!selectedSiteId}
          className="block w-48 rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
        >
          <option value="">All Assets</option>
          {assets?.map((asset) => (
            <option key={asset.id} value={asset.id}>
              {asset.name}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}
