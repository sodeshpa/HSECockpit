/**
 * Tests for MetricLabel component.
 *
 * Validates:
 * - Correct text rendered for each status
 * - Correct colour classes applied per status
 * - Accessibility: text content present, proper aria attributes
 * - axe-core assertion for WCAG 2.1 AA compliance
 */
import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { MetricLabel } from "./MetricLabel";
import { expectNoA11yViolations } from "../../test/a11y";

describe("MetricLabel", () => {
  describe("renders correct text for each status", () => {
    it("renders 'Fresh' for FRESH status", () => {
      render(<MetricLabel status="FRESH" />);
      expect(screen.getByText("Fresh")).toBeInTheDocument();
    });

    it("renders 'Partial' for PARTIAL status", () => {
      render(<MetricLabel status="PARTIAL" />);
      expect(screen.getByText("Partial")).toBeInTheDocument();
    });

    it("renders 'Stale' for STALE status", () => {
      render(<MetricLabel status="STALE" />);
      expect(screen.getByText("Stale")).toBeInTheDocument();
    });

    it("renders 'No Data' for NO_DATA status", () => {
      render(<MetricLabel status="NO_DATA" />);
      expect(screen.getByText("No Data")).toBeInTheDocument();
    });

    it("renders nothing for an unknown status", () => {
      const { container } = render(<MetricLabel status="UNKNOWN" />);
      expect(container.innerHTML).toBe("");
    });
  });

  describe("applies correct colour classes", () => {
    it("applies green classes for FRESH", () => {
      render(<MetricLabel status="FRESH" />);
      const badge = screen.getByText("Fresh");
      expect(badge).toHaveClass("bg-green-100", "text-green-800", "border-green-300");
    });

    it("applies amber classes for PARTIAL", () => {
      render(<MetricLabel status="PARTIAL" />);
      const badge = screen.getByText("Partial");
      expect(badge).toHaveClass("bg-amber-100", "text-amber-800", "border-amber-300");
    });

    it("applies grey classes for STALE", () => {
      render(<MetricLabel status="STALE" />);
      const badge = screen.getByText("Stale");
      expect(badge).toHaveClass("bg-gray-100", "text-gray-700", "border-gray-300");
    });

    it("applies red/grey classes for NO_DATA", () => {
      render(<MetricLabel status="NO_DATA" />);
      const badge = screen.getByText("No Data");
      expect(badge).toHaveClass("bg-red-50", "text-red-700", "border-red-300");
    });
  });

  describe("accessibility", () => {
    it("has role=status attribute", () => {
      render(<MetricLabel status="FRESH" />);
      expect(screen.getByRole("status")).toBeInTheDocument();
    });

    it("has descriptive aria-label", () => {
      render(<MetricLabel status="PARTIAL" />);
      const badge = screen.getByRole("status");
      expect(badge).toHaveAttribute("aria-label", "Data status: Partial");
    });

    it("contains visible text content (not colour-only)", () => {
      render(<MetricLabel status="STALE" />);
      const badge = screen.getByRole("status");
      expect(badge).toHaveTextContent("Stale");
    });

    it("has no WCAG 2.1 AA violations for FRESH status", async () => {
      const { container } = render(<MetricLabel status="FRESH" />);
      await expectNoA11yViolations(container);
    });

    it("has no WCAG 2.1 AA violations for PARTIAL status", async () => {
      const { container } = render(<MetricLabel status="PARTIAL" />);
      await expectNoA11yViolations(container);
    });

    it("has no WCAG 2.1 AA violations for STALE status", async () => {
      const { container } = render(<MetricLabel status="STALE" />);
      await expectNoA11yViolations(container);
    });

    it("has no WCAG 2.1 AA violations for NO_DATA status", async () => {
      const { container } = render(<MetricLabel status="NO_DATA" />);
      await expectNoA11yViolations(container);
    });
  });

  describe("custom className", () => {
    it("accepts and applies additional className prop", () => {
      render(<MetricLabel status="FRESH" className="ml-2" />);
      const badge = screen.getByText("Fresh");
      expect(badge).toHaveClass("ml-2");
    });
  });
});
