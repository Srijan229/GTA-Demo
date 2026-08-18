import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getFacultyApplications } from '../api/facultyApi';

export function FacultyApplicationsPage() {
  const applications = useQuery({ queryKey: ['faculty-applications'], queryFn: getFacultyApplications });
  if (applications.isPending) return <p role="status">Loading authorized applications…</p>;
  if (applications.isError) return <div className="error-banner" role="alert">Applications could not be loaded.</div>;
  return <div><header><h2>Faculty applications</h2><p>Review applicants connected to your assigned sections.</p></header><div className="table-scroll"><table><caption>{applications.data.length} authorized applications</caption><thead><tr><th>Applicant</th><th>Course</th><th>Program</th><th>Status</th><th>Review</th><th><span className="visually-hidden">Action</span></th></tr></thead><tbody>{applications.data.map((item) => <tr key={item.choiceId}><td>{item.applicantName}</td><td>{item.courseCode}-{item.sectionNumber}</td><td>{item.program}</td><td><span className="status-badge">{item.status}</span></td><td>{item.hireRecommended ? 'Hire recommended' : item.interviewMarked ? 'Interview' : 'Not started'}</td><td><Link className="button" to={`/faculty/applications/${item.choiceId}`}>Review application</Link></td></tr>)}</tbody></table></div></div>;
}
