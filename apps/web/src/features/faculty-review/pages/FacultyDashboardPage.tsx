import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getFacultyApplications, getFacultySections } from '../api/facultyApi';

export function FacultyDashboardPage() {
  const sections = useQuery({ queryKey: ['faculty-sections'], queryFn: getFacultySections });
  const applications = useQuery({ queryKey: ['faculty-applications'], queryFn: getFacultyApplications });
  if (sections.isPending || applications.isPending) return <p role="status">Loading faculty dashboard…</p>;
  if (sections.isError || applications.isError) return <div className="error-banner" role="alert">Faculty dashboard could not be loaded.</div>;
  return <div><div className="summary-grid"><article><span>Assigned sections</span><strong>{sections.data.length}</strong></article><article><span>Applications</span><strong>{applications.data.length}</strong></article><article><span>Awaiting interview</span><strong>{applications.data.filter((item) => !item.interviewMarked).length}</strong></article><article><span>Hire recommendations</span><strong>{applications.data.filter((item) => item.hireRecommended).length}</strong></article></div><section className="content-card"><h2>Applications requiring attention</h2>{applications.data.filter((item) => !item.interviewMarked).slice(0, 5).map((item) => <p key={item.choiceId}><Link to={`/faculty/applications/${item.choiceId}`}>{item.applicantName} — {item.courseCode}-{item.sectionNumber}</Link></p>)}{applications.data.length === 0 && <p>No applications are currently assigned to your sections.</p>}</section></div>;
}
