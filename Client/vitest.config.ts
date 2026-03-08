import { getViteConfig } from 'astro/config';
import { defineConfig } from 'vitest/config';

export default defineConfig(
  getViteConfig({
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['./tests/setup.ts'],
      include: ['tests/unit/**/*.{test,spec}.{ts,tsx}'],
      exclude: ['tests/e2e/**'],
    },
  })
);
