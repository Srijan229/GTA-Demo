import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getCurrentUser } from '../../authentication/api/authenticationApi';
import { getDocuments } from '../../documents/api/documentApi';
import { getProfileCompletion } from '../../profiles/api/profileApi';
import { getAvailableSections, getMyApplications } from '../api/applicationApi';

export function ApplicantDashboardPage() {
  const user = useQuery({ queryKey: ['current-user'], queryFn: getCurrentUser });
  const completion = useQuery({ queryKey: ['profile-completion'], queryFn: getProfileCompletion });
  const documents = useQuery({ queryKey: ['applicant-documents'], queryFn: getDocuments });
  const applications = useQuery({ queryKey: ['my-applications'], queryFn: getMyApplications });
  const sections = useQuery({ queryKey: ['available-sections'], queryFn: getAvailableSections });
  const queries = [user, completion, documents, applications, sections];
  if (queries.some(query => query.isPending)) return <p role="status">Loading your GTA home page…</p>;
  if (queries.some(query => query.isError)) return <div className="error-banner" role="alert">Your GTA home page could not be loaded.</div>;
  const latest = applications.data?.[0]; const phase = sections.data?.[0]; const documentCount = documents.data?.length ?? 0;
  return <div className="gta-home"><header className="welcome-panel"><p className="eyebrow">GTA applicant portal</p><h2>Welcome, {user.data?.displayName}</h2><p>Manage your GTA profile, apply to available courses, and review your applications.</p></header>
    {phase ? <section className="phase-banner"><div><strong>Applications are open</strong><p>{phase.phaseName} · {phase.term}</p></div><Link className="button" to="/applicant/sections">Apply to Course</Link></section> : <section className="notice"><strong>No application phase is currently open.</strong><p>You can still review your GTA profile and previous applications.</p></section>}
    <div className="portal-actions" aria-label="Applicant actions"><Link className="portal-action" to="/applicant/profile"><span className="portal-icon" aria-hidden="true">1</span><div><h3>View GTA Profile</h3><p>Review and update your personal, academic, education, and experience information.</p><strong>{completion.data?.percentage ?? 0}% complete</strong></div></Link><Link className="portal-action" to="/applicant/sections"><span className="portal-icon" aria-hidden="true">2</span><div><h3>Apply to Course</h3><p>View available GTA course sections and submit an application.</p><strong>{sections.data?.length ?? 0} available</strong></div></Link><Link className="portal-action" to="/applicant/applications"><span className="portal-icon" aria-hidden="true">3</span><div><h3>My Applications</h3><p>Review submitted applications, current status, and application history.</p><strong>{applications.data?.length ?? 0} application(s)</strong></div></Link></div>
    <div className="summary-grid"><article><span>Profile completion</span><strong>{completion.data?.percentage ?? 0}%</strong></article><article><span>Documents submitted</span><strong>{documentCount}/2</strong></article><article><span>Applications</span><strong>{applications.data?.length ?? 0}</strong></article><article><span>Latest status</span><strong className="summary-status">{latest?.status ?? 'None'}</strong></article></div>
    <section><h3>Next steps</h3><ul className="next-steps">{(completion.data?.percentage ?? 0) < 100 && <li><Link to="/applicant/profile">Complete your GTA profile</Link> before applying.</li>}{documentCount < 2 && <li><Link to="/applicant/documents">Upload your resume and unofficial transcript</Link>.</li>}{completion.data?.percentage === 100 && documentCount === 2 && !latest && <li><Link to="/applicant/sections">Apply to an available course</Link>.</li>}{latest && <li><Link to={`/applicant/applications/${latest.id}`}>View your latest application</Link> and its current status.</li>}</ul></section>
  </div>;
}
