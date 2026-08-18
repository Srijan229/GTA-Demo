import { useQuery } from '@tanstack/react-query';
import { adminApi } from '../api/adminApi';

export function AdminDashboardPage() {
  const query = useQuery({ queryKey: ['admin-dashboard'], queryFn: adminApi.dashboard });
  if (query.isPending) return <p role="status">Loading administration dashboard…</p>;
  if (query.isError) return <div className="error-banner" role="alert">The administration dashboard could not be loaded.</div>;
  const data = query.data;
  return <div><div className="summary-grid"><article><span>Applications</span><strong>{data.applications}</strong></article><article><span>Applicants</span><strong>{data.applicants}</strong></article><article><span>Active sections</span><strong>{data.activeSections}</strong></article><article><span>Awaiting review</span><strong>{data.awaitingReview}</strong></article></div><h2>Operational warnings</h2>{data.warnings.length ? <ul>{data.warnings.map(warning => <li key={warning}>{warning}</li>)}</ul> : <p>No warnings.</p>}</div>;
}
