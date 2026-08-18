import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { facultyDocumentUrl, getFacultyReview, recordFacultyAction, type FacultyAction } from '../api/facultyApi';

export function FacultyReviewPage() {
  const { choiceId = '' } = useParams();
  const queryClient = useQueryClient();
  const review = useQuery({ queryKey: ['faculty-review', choiceId], queryFn: () => getFacultyReview(choiceId), enabled: Boolean(choiceId) });
  const [notes, setNotes] = useState('');
  const action = useMutation({ mutationFn: (request: FacultyAction) => recordFacultyAction(choiceId, request), onSuccess: () => { void queryClient.invalidateQueries({ queryKey: ['faculty-review', choiceId] }); void queryClient.invalidateQueries({ queryKey: ['faculty-applications'] }); } });
  if (review.isPending) return <p role="status">Loading applicant review…</p>;
  if (review.isError) return <div className="error-banner" role="alert">This application could not be loaded or is not assigned to you.</div>;
  const data = review.data;
  const doAction = (request: Omit<FacultyAction, 'internalNotes'>) => action.mutate({ ...request, internalNotes: notes || data.internalNotes || null });
  return <div><header className="review-header"><div><p className="role-label">{data.reference}</p><h2>{data.profile.displayName}</h2><p>{data.profile.program} · {data.courseCode}-{data.sectionNumber} · Submitted {new Date(data.submittedAtUtc).toLocaleDateString()}</p></div><span className="status-badge">{data.status}</span></header><div className="review-layout"><div className="review-main">
    <section className="content-card"><h3>Academic profile</h3><dl className="detail-grid"><div><dt>Degree</dt><dd>{data.profile.degree ?? 'Not provided'}</dd></div><div><dt>Major</dt><dd>{data.profile.major ?? 'Not provided'}</dd></div><div><dt>GPA</dt><dd>{data.profile.gpa ?? 'Not provided'}</dd></div><div><dt>Employment basis</dt><dd>{data.employmentBasis}</dd></div></dl></section>
    <section className="content-card"><h3>Education</h3>{data.profile.education.map((item) => <article key={item.id}><strong>{item.institution}</strong><p>{[item.degree, item.fieldOfStudy].filter(Boolean).join(' · ')}</p></article>)}{data.profile.education.length === 0 && <p>No education records provided.</p>}</section>
    <section className="content-card"><h3>Experience</h3>{data.profile.experience.map((item) => <article key={item.id}><strong>{item.title}</strong><p>{item.organization}{item.isGtaExperience ? ' · GTA experience' : ''}</p></article>)}{data.profile.experience.length === 0 && <p>No experience records provided.</p>}</section>
    <section className="content-card"><h3>Documents</h3>{data.documents.map((document) => <p key={document.id}><a className="secondary-button download-link" href={facultyDocumentUrl(document.id)}>{document.type}: {document.originalFileName}</a></p>)}</section>
  </div><aside className="action-panel"><h3>Faculty review</h3><p><strong>Interview:</strong> {data.interviewMarked ? 'Marked' : 'Not marked'}</p><p><strong>Hire recommendation:</strong> {data.hireRecommended ? 'Recommended' : 'Not recommended'}</p>{data.internalNotes && <p><strong>Current internal notes:</strong><br />{data.internalNotes}</p>}<label className="field"><span>Update internal notes</span><textarea maxLength={2000} rows={6} value={notes} onChange={(event) => setNotes(event.target.value)} /><small>Visible only to authorized faculty and administrators.</small></label>
    <button className="secondary-button" disabled={action.isPending} type="button" onClick={() => doAction({ action: 'Interview', active: !data.interviewMarked })}>{data.interviewMarked ? 'Undo interview mark' : 'Mark for interview'}</button>
    <button className="button" disabled={action.isPending || !data.interviewMarked} type="button" onClick={() => doAction({ action: 'HireRecommendation', active: !data.hireRecommended })}>{data.hireRecommended ? 'Remove hire recommendation' : 'Recommend for hire'}</button>
    {action.isError && <div className="error-banner" role="alert">The action could not be recorded. Check the required action order and workload limit.</div>}{action.isSuccess && <p className="success-message" role="status">Faculty action recorded.</p>}
  </aside></div></div>;
}
