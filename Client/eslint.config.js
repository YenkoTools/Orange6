// @ts-check
import eslint from "@eslint/js";
import tseslint from "typescript-eslint";
import sonarjs from "eslint-plugin-sonarjs";
import astro from "eslint-plugin-astro";

export default tseslint.config(
  // Base JS recommended rules
  eslint.configs.recommended,

  // TypeScript-aware rules for .ts/.tsx files
  ...tseslint.configs.recommended,

  // SonarJS rules (bug detection, code smells, cognitive complexity)
  sonarjs.configs.recommended,

  // Astro-specific rules for .astro files
  ...astro.configs.recommended,

  // Project-specific overrides
  {
    rules: {
      // Allow 'any' in test/config files where it's often unavoidable
      "@typescript-eslint/no-explicit-any": "warn",
    },
  },

  // Ignore build output and config files that don't benefit from linting
  {
    ignores: ["dist/**", "node_modules/**", ".astro/**"],
  },
);
