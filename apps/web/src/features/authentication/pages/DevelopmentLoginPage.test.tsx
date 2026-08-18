import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { DevelopmentLoginPage } from './DevelopmentLoginPage';

describe('DevelopmentLoginPage', () => {
  it('identifies the page as development-only and exposes each demo role', () => {
    render(
      <QueryClientProvider client={new QueryClient()}>
        <MemoryRouter><DevelopmentLoginPage /></MemoryRouter>
      </QueryClientProvider>,
    );
    expect(screen.getByText('Local Development')).toBeInTheDocument();
    expect(screen.getByText('Loading development users…')).toBeInTheDocument();
    expect(screen.getByText(/Production authentication will use GMU-approved Microsoft Entra ID/)).toBeInTheDocument();
  });
});
