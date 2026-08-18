import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { getAvailableSections } from '../api/applicationApi';

export function AvailableSectionsPage() {
  const [search, setSearch] = useState('');
  const sections = useQuery({ queryKey: ['available-sections'], queryFn: getAvailableSections });
  if (sections.isPending) return <p role="status">Loading available sections…</p>;
  if (sections.isError) return <div className="error-banner" role="alert">Available sections could not be loaded.</div>;
  const filtered = sections.data.filter(section => `${section.courseCode} ${section.courseTitle} ${section.sectionNumber}`.toLowerCase().includes(search.toLowerCase()));
  return <div><header><h2>Apply to Course</h2><p>View GTA opportunities for your program and the currently open application phase.</p></header>{sections.data.length === 0 ? <div className="empty-state"><h3>No courses are currently available</h3><p>Check again during an active application phase.</p></div> : <><label className="field table-filter"><span>Search courses</span><input type="search" value={search} onChange={event => setSearch(event.target.value)} placeholder="Course code, section, or title" /></label><div className="table-scroll"><table><caption>{filtered.length} course section{filtered.length === 1 ? '' : 's'} found</caption><thead><tr><th>Course</th><th>Section</th><th>Term</th><th>Schedule</th><th>Delivery</th><th>Positions</th><th><span className="visually-hidden">Action</span></th></tr></thead><tbody>{filtered.map(section => <tr key={section.id}><td><strong>{section.courseCode}</strong><br /><span>{section.courseTitle}</span></td><td>{section.sectionNumber}</td><td>{section.term}</td><td>{section.schedule ?? 'Not provided'}</td><td>{section.deliveryMethod ?? 'Not provided'}</td><td>{section.availablePositions ?? 'Not provided'}</td><td>{section.alreadyApplied ? <span className="status-badge">Already applied</span> : <Link className="button" to={`/applicant/applications/new?section=${section.id}`}>Apply</Link>}</td></tr>)}</tbody></table></div></>}</div>;
}
