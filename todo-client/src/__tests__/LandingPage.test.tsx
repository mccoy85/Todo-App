/* @vitest-environment jsdom */
import '@testing-library/jest-dom/vitest';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LandingPage } from '../components/LandingPage';

describe('LandingPage', () => {
  it('renders the headline and launch link', () => {
    render(
      <MemoryRouter>
        <LandingPage />
      </MemoryRouter>
    );

    expect(screen.getByText(/Tasky keeps your day on track\./i)).toBeInTheDocument();

    const launchLink = screen.getByRole('link', { name: /launch tasky/i });
    expect(launchLink).toHaveAttribute('href', '/app');
  });
});
