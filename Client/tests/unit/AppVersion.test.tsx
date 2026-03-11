import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import AppVersion from '../../src/components/AppVersion';

describe('AppVersion', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders nothing while loading', () => {
    vi.stubGlobal('fetch', vi.fn(() => new Promise(() => {})));
    const { container } = render(<AppVersion />);
    expect(container).toBeEmptyDOMElement();
  });

  it('renders the git tag and build date on success', async () => {
    vi.stubGlobal('fetch', vi.fn(() =>
      Promise.resolve({
        json: () => Promise.resolve({
          GitTag: 'v1.2.3',
          BuildNumber: '42',
          BuildDate: '2026-03-07T00:00:00Z',
          CommitHash: 'abc1234',
        }),
      })
    ));

    render(<AppVersion />);

    await waitFor(() => {
      expect(screen.getByText(/v1\.2\.3/)).toBeInTheDocument();
    });
  });

  it('renders nothing when fetch fails', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(new Error('network error'))));

    const { container } = render(<AppVersion />);

    await waitFor(() => {
      expect(container).toBeEmptyDOMElement();
    });
  });
});
