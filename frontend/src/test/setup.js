/**
 * Vitest global test setup.
 *
 * This file is referenced in vite.config.js under test.setupFiles.
 * It loads Testing Library matchers and configures vitest-axe so that
 * every component test file can use accessibility assertions without
 * additional imports.
 */
import "@testing-library/jest-dom";
import { expect } from "vitest";
import * as matchers from "vitest-axe/matchers";

expect.extend(matchers);
