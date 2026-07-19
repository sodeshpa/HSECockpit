/**
 * Smoke test to verify axe-core accessibility helper is configured correctly.
 * This test renders a minimal accessible HTML fragment and asserts zero violations.
 */
import { describe, it, expect } from "vitest";
import { expectNoA11yViolations } from "./a11y";

describe("a11y helper", () => {
  it("passes for accessible HTML", async () => {
    const container = document.createElement("div");
    container.innerHTML = `
      <main>
        <h1>Accessible Page</h1>
        <button type="button">Click me</button>
      </main>
    `;
    document.body.appendChild(container);

    await expectNoA11yViolations(container);

    document.body.removeChild(container);
  });

  it("detects violations in inaccessible HTML", async () => {
    const container = document.createElement("div");
    // An image without alt text violates WCAG
    container.innerHTML = `<img src="test.png" />`;
    document.body.appendChild(container);

    const { axe } = await import("vitest-axe");
    const results = await axe(container, {
      runOnly: { type: "tag", values: ["wcag2a", "wcag2aa"] },
    });

    expect(results.violations.length).toBeGreaterThan(0);

    document.body.removeChild(container);
  });
});
