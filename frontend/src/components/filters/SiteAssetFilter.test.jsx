import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SiteAssetFilter } from "./SiteAssetFilter";

// Mock the hooks module
vi.mock("../../hooks/useBarriers", () => ({
  useSites: vi.fn(),
  useAssets: vi.fn(),
}));

import { useSites, useAssets } from "../../hooks/useBarriers";

function renderWithQuery(ui) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>
  );
}

describe("SiteAssetFilter", () => {
  beforeEach(() => {
    useSites.mockReturnValue({
      data: [
        { id: "site-1", name: "Alpha Platform" },
        { id: "site-2", name: "Beta Refinery" },
      ],
      isLoading: false,
    });
    useAssets.mockReturnValue({
      data: [
        { id: "asset-1", name: "Compressor A" },
        { id: "asset-2", name: "Valve B" },
      ],
      isLoading: false,
    });
  });

  it("renders site and asset dropdowns with labels", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    expect(screen.getByLabelText("Site")).toBeInTheDocument();
    expect(screen.getByLabelText("Asset")).toBeInTheDocument();
  });

  it("populates site options from useSites hook", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const siteSelect = screen.getByLabelText("Site");
    expect(siteSelect).toHaveDisplayValue("All Sites");
    expect(screen.getByText("Alpha Platform")).toBeInTheDocument();
    expect(screen.getByText("Beta Refinery")).toBeInTheDocument();
  });

  it("disables asset dropdown when no site is selected", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const assetSelect = screen.getByLabelText("Asset");
    expect(assetSelect).toBeDisabled();
  });

  it("enables asset dropdown when a site is selected", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const siteSelect = screen.getByLabelText("Site");
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    const assetSelect = screen.getByLabelText("Asset");
    expect(assetSelect).not.toBeDisabled();
  });

  it("calls onFilterChange when site changes", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const siteSelect = screen.getByLabelText("Site");
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    expect(onChange).toHaveBeenCalledWith({ siteId: "site-1", assetId: null });
  });

  it("resets asset to All when site changes", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    // Select a site
    const siteSelect = screen.getByLabelText("Site");
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    // Select an asset
    const assetSelect = screen.getByLabelText("Asset");
    fireEvent.change(assetSelect, { target: { value: "asset-1" } });

    // Change site — asset should reset
    fireEvent.change(siteSelect, { target: { value: "site-2" } });

    expect(assetSelect).toHaveDisplayValue("All Assets");
    expect(onChange).toHaveBeenLastCalledWith({
      siteId: "site-2",
      assetId: null,
    });
  });

  it("calls onFilterChange with both siteId and assetId when asset is selected", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const siteSelect = screen.getByLabelText("Site");
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    const assetSelect = screen.getByLabelText("Asset");
    fireEvent.change(assetSelect, { target: { value: "asset-1" } });

    expect(onChange).toHaveBeenLastCalledWith({
      siteId: "site-1",
      assetId: "asset-1",
    });
  });

  it("passes null for siteId and assetId when All Sites is selected", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    // First select a site
    const siteSelect = screen.getByLabelText("Site");
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    // Then go back to All Sites
    fireEvent.change(siteSelect, { target: { value: "" } });

    expect(onChange).toHaveBeenLastCalledWith({
      siteId: null,
      assetId: null,
    });
  });

  it("is keyboard navigable (select elements are focusable)", () => {
    const onChange = vi.fn();
    renderWithQuery(<SiteAssetFilter onFilterChange={onChange} />);

    const siteSelect = screen.getByLabelText("Site");

    // Site select should be focusable
    siteSelect.focus();
    expect(document.activeElement).toBe(siteSelect);

    // Select a site to enable the asset dropdown
    fireEvent.change(siteSelect, { target: { value: "site-1" } });

    const assetSelect = screen.getByLabelText("Asset");
    assetSelect.focus();
    expect(document.activeElement).toBe(assetSelect);
  });
});
