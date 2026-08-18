import { useQuery } from '@tanstack/react-query';
import { getFacultySections } from '../api/facultyApi';

export function FacultySectionsPage() {
  const sections = useQuery({ queryKey: ['faculty-sections'], queryFn: getFacultySections });
  if (sections.isPending) return <p role="status">Loading assigned sections…</p>;
  if (sections.isError) return <div className="error-banner" role="alert">Assigned sections could not be loaded.</div>;
  return <div><header><h2>My sections</h2><p>Only sections assigned to your faculty identity are shown.</p></header><div className="table-scroll"><table><caption>{sections.data.length} assigned sections</caption><thead><tr><th>Course</th><th>Section</th><th>Term</th><th>Schedule</th><th>Applications</th></tr></thead><tbody>{sections.data.map((section) => <tr key={section.id}><td><strong>{section.courseCode}</strong><br />{section.courseTitle}</td><td>{section.sectionNumber}</td><td>{section.term}</td><td>{section.schedule ?? 'Not provided'}</td><td>{section.applicationCount}</td></tr>)}</tbody></table></div></div>;
}
