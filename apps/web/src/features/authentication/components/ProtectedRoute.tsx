import { useQuery } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { ApiError } from '../../../shared/api/http';
import { getCurrentUser } from '../api/authenticationApi';

type ProtectedRouteProps = {
  allowedRole: 'Applicant' | 'Faculty' | 'Administrator';
  children: ReactNode;
};

export function ProtectedRoute({ allowedRole, children }: ProtectedRouteProps) {
  const currentUser = useQuery({
    queryKey: ['current-user'],
    queryFn: getCurrentUser,
    retry: (count, error) => !(error instanceof ApiError && error.status === 401) && count < 1,
  });

  if (currentUser.isPending) return <main className="system-page"><p role="status">Loading your session…</p></main>;
  if (currentUser.error instanceof ApiError && currentUser.error.status === 401) return <Navigate to="/login" replace />;
  if (currentUser.isError) return <main className="system-page"><div role="alert"><h1>Session unavailable</h1><p>The API could not verify your session.</p></div></main>;
  if (!currentUser.data.roles.includes(allowedRole)) return <Navigate to="/access-denied" replace />;
  return children;
}
