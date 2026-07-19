/**
 * Reusable accessibility assertion helper.
 *
 * Usage in component tests:
 *
 *   import { render } from "@testing-library/react";
 *   import { expectNoA11yViolations } from "../test/a11y";
 *
 *   it("has no accessibility violations", async () => {
 *     const { container } = render(<MyComponent />);
 *     await expectNoA11yViolations(container);
 *   });
 *
 * The helper runs axe-core against the rendered DOM container and
 * asserts zero WCAG 2.1 AA violations. Any violation causes the test
 * to fail with a detailed report of the issue, impacted nodes, and
 * remediation suggestions.
 */
import { axe } from "vitest-axe";

/**
 * Assert that the given DOM container has no WCAG 2.1 AA violations.
 *
 * @param {HTMLElement} container - The rendered DOM container (from Testing Library's render().container)
 * @param {object} [options] - Optional axe-core run options to customise rules or tags
 * @returns {Promise<void>}
 */
export async function expectNoA11yViolations(container, options = {}) {
  const defaultOptions = {
    runOnly: {
      type: "tag",
      values: ["wcag2a", "wcag2aa", "wcag21aa"],
    },
    ...options,
  };

  const results = await axe(container, defaultOptions);
  expect(results).toHaveNoViolations();
}
