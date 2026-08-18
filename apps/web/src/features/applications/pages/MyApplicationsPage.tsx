import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getMyApplications } from '../api/applicationApi';

export function MyApplicationsPage() {
  const [search, setSearch] = useState(''); const [status, setStatus] = useState('All');
  const applications = useQuery({ queryKey: ['my-applications'], queryFn: getMyApplications });
  if (applications.isPending) return <p role="status">Loading your applications…</p>;
  if (applications.isError) return <div className="error-banner" role="alert">Applications could not be loaded.</div>;
  const statuses = [...new Set(applications.data.map(item => item.status))];
  const filtered = applications.data.filter(item => (status === 'All' || item.status === status) && `${item.reference} ${item.term} ${item.choices.map(choice => choice.courseCode).join(' ')}`.toLowerCase().includes(search.toLowerCase()));
  return <div><header><h2>My Applications</h2><p>View all GTA applications you have submitted and their current status.</p></header>{applications.data.length === 0 ? <div className="empty-state"><h3>No submitted applications</h3><p><Link to="/applicant/sections">Apply to a course</Link> when opportunities are available.</p></div> : <><div className="table-filters"><label className="field"><span>Search applications</span><input type="search" value={search} onChange={event => setSearch(event.target.value)} placeholder="Reference, term, or course" /></label><label className="field"><span>Status</span><select value={status} onChange={event => setStatus(event.target.value)}><option>All</option>{statuses.map(value => <option key={value}>{value}</option>)}</select></label></div><div className="table-scroll"><table><caption>{filtered.length} application{filtered.length === 1 ? '' : 's'} found</caption><thead><tr><th>Application</th><th>Term</th><th>Course or section</th><th>Submitted</th><th>Status</th><th>Action</th></tr></thead><tbody>{filtered.map(application => <tr key={application.id}><td><strong>{application.reference}</strong></td><td>{application.term}</td><td>{application.choices.map(choice => `${choice.courseCode}-${choice.sectionNumber}`).join(', ')}</td><td>{new Date(application.submittedAtUtc).toLocaleDateString()}</td><td><span className="status-badge">{application.status}</span></td><td><Link to={`/applicant/applications/${application.id}`}>View details</Link></td></tr>)}</tbody></table></div></>}</div>;
}
