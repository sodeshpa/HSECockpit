import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { QualityBanner } from "./QualityBanner";
import { expectNoA11yViolations } from "../../test/a11y";

describe("QualityBanner", () => {
  it("renders when flaggedCount > 0", () => {
    render(
      <QualityBanner flaggedCount={3} conflictCount={0} totalCount={10} />
    );

    expect(
      screen.getByText("Some data in this view has quality issues")
    ).toBeInTheDocument();
    expect(screen.getByText("3 flagged of 10 records")).toBeInTheDocument();
  });

  it("renders when conflictCount > 0", () => {
    render(
      <QualityBanner flaggedCount={0} conflictCount={2} totalCount={8} />
    );

    expect(
      screen.getByText("Some data in this view has quality issues")
    ).toBeInTheDocument();
    expect(
      screen.getByText("2 conflicting of 8 records")
    ).toBeInTheDocument();
  });

  it("renders when both flaggedCount and conflictCount > 0", () => {
    render(
      <QualityBanner flaggedCount={2} conflictCount={1} totalCount={15} />
    );

    expect(
      screen.getByText("2 flagged, 1 conflicting of 15 records")
    ).toBeInTheDocument();
  });

  it("does not render when flaggedCount = 0 and conflictCount = 0", () => {
    const { container } = render(
      <QualityBanner flaggedCount={0} conflictCount={0} totalCount={10} />
    );

    expect(container.firstChild).toBeNull();
  });

  it("contains accessible text (not colour-only)", () => {
    render(
      <QualityBanner flaggedCount={5} conflictCount={2} totalCount={20} />
    );

    // The warning message is conveyed via text, not colour alone
    expect(
      screen.getByText("Some data in this view has quality issues")
    ).toBeInTheDocument();
    expect(
      screen.getByText("5 flagged, 2 conflicting of 20 records")
    ).toBeInTheDocument();
  });

  it("has proper aria attributes", () => {
    render(
      <QualityBanner flaggedCount={1} conflictCount={0} totalCount={5} />
    );

    const alert = screen.getByRole("alert");
    expect(alert).toHaveAttribute("aria-live", "polite");
  });

  it("can be dismissed when dismissible is true", () => {
    const onDismiss = vi.fn();
    render(
      <QualityBanner
        flaggedCount={1}
        conflictCount={0}
        totalCount={5}
        dismissible={true}
        onDismiss={onDismiss}
      />
    );

    const dismissButton = screen.getByLabelText("Dismiss quality warning");
    fireEvent.click(dismissButton);

    expect(onDismiss).toHaveBeenCalledTimes(1);
    expect(
      screen.queryByText("Some data in this view has quality issues")
    ).not.toBeInTheDocument();
  });

  it("does not show dismiss button when dismissible is false", () => {
    render(
      <QualityBanner
        flaggedCount={1}
        conflictCount={0}
        totalCount={5}
        dismissible={false}
      />
    );

    expect(
      screen.queryByLabelText("Dismiss quality warning")
    ).not.toBeInTheDocument();
  });

  it("has no accessibility violations", async () => {
    const { container } = render(
      <QualityBanner flaggedCount={3} conflictCount={1} totalCount={10} />
    );

    await expectNoA11yViolations(container);
  });
});
