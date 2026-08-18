import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { getDocuments } from '../../documents/api/documentApi';
import { getProfileCompletion } from '../../profiles/api/profileApi';
import { getApplicationConfiguration, getAvailableSections, submitApplication, type SubmitApplication } from '../api/applicationApi';

export function NewApplicationPage() {
  const [params] = useSearchParams();
  const queryClient = useQueryClient();
  const sections = useQuery({ queryKey: ['available-sections'], queryFn: getAvailableSections });
  const completion = useQuery({ queryKey: ['profile-completion'], queryFn: getProfileCompletion });
  const documents = useQuery({ queryKey: ['applicant-documents'], queryFn: getDocuments });
  const configuration = useQuery({ queryKey: ['application-configuration'], queryFn: getApplicationConfiguration });
  const initialSection = params.get('section');
  const [selectedIds, setSelectedIds] = useState<string[]>(initialSection ? [initialSection] : []);
  const [basis, setBasis] = useState<SubmitApplication['employmentBasis']>('PartTime10Hours');
  const selected = useMemo(() => sections.data?.filter((section) => selectedIds.includes(section.id)) ?? [], [sections.data, selectedIds]);
  const hasResume = documents.data?.some((item) => item.type === 'Resume') ?? false;
  const hasTranscript = documents.data?.some((item) => item.type === 'UnofficialTranscript') ?? false;
  const ready = completion.data?.percentage === 100 && hasResume && hasTranscript;
  const submit = useMutation({ mutationFn: submitApplication, onSuccess: () => { void queryClient.invalidateQueries({ queryKey: ['available-sections'] }); void queryClient.invalidateQueries({ queryKey: ['my-applications'] }); } });

  if (sections.isPending || completion.isPending || documents.isPending || configuration.isPending) return <p role="status">Preparing your application…</p>;
  if (sections.isError || completion.isError || documents.isError || configuration.isError) return <div className="error-banner" role="alert">The application form could not be prepared.</div>;
  if (submit.data) return <div className="confirmation-panel"><p className="role-label">Application submitted</p><h2>Thank you, your application is complete</h2><dl><div><dt>Reference</dt><dd>{submit.data.reference}</dd></div><div><dt>Submitted</dt><dd>{new Date(submit.data.submittedAtUtc).toLocaleString()}</dd></div><div><dt>Status</dt><dd>{submit.data.status}</dd></div></dl><p>Faculty reviewers can now evaluate your selected sections. You can follow progress from My applications.</p><Link className="button" to="/applicant/applications">View applications</Link></div>;

  const phase = selected[0] ? sections.data.find((section) => section.id === selected[0]?.id) : sections.data[0];
  return <div><header><h2>New application</h2><p>Review your readiness, select up to five sections, and explicitly confirm submission.</p></header>
    <section className="readiness-card" aria-labelledby="readiness-title"><h3 id="readiness-title">Application readiness</h3><ul><li className={completion.data.percentage === 100 ? 'ready' : 'not-ready'}>Profile: {completion.data.percentage}% complete</li><li className={hasResume ? 'ready' : 'not-ready'}>Resume: {hasResume ? 'Ready' : 'Missing'}</li><li className={hasTranscript ? 'ready' : 'not-ready'}>Unofficial transcript: {hasTranscript ? 'Ready' : 'Missing'}</li></ul>{!ready && <p><Link to="/applicant/profile">Complete profile</Link> or <Link to="/applicant/documents">manage documents</Link> before submitting.</p>}</section>
    <fieldset className="application-section"><legend>Employment basis</legend><label><input checked={basis === 'PartTime10Hours'} name="basis" type="radio" onChange={() => setBasis('PartTime10Hours')} /> Part-time (10 hours/week)</label><label><input checked={basis === 'FullTime20Hours'} name="basis" type="radio" onChange={() => setBasis('FullTime20Hours')} /> Full-time (20 hours/week)</label></fieldset>
    <fieldset className="application-section"><legend>Selected sections (up to {configuration.data.maximumSectionChoices})</legend>{sections.data.map((section) => <label className="section-choice" key={section.id}><input type="checkbox" checked={selectedIds.includes(section.id)} disabled={section.alreadyApplied || (!selectedIds.includes(section.id) && selectedIds.length >= configuration.data.maximumSectionChoices)} onChange={(event) => setSelectedIds((current) => event.target.checked ? [...current, section.id] : current.filter((id) => id !== section.id))} /><span><strong>{section.courseCode}-{section.sectionNumber}</strong> — {section.courseTitle}<small>{section.term} · {section.schedule ?? 'Schedule not provided'}</small></span></label>)}</fieldset>
    <section className="review-panel"><h3>Review and submit</h3><p><strong>Phase:</strong> {phase?.phaseName ?? 'No active phase'}</p><p><strong>Basis:</strong> {basis === 'PartTime10Hours' ? 'Part-time (10 hours/week)' : 'Full-time (20 hours/week)'}</p><p><strong>Sections:</strong> {selected.length ? selected.map((item) => `${item.courseCode}-${item.sectionNumber}`).join(', ') : 'None selected'}</p><p>Submitting confirms that the information and documents in your application are ready for faculty review.</p><button className="button" type="button" disabled={!ready || selectedIds.length === 0 || submit.isPending || !phase} onClick={() => phase && submit.mutate({ phaseId: phase.phaseId, employmentBasis: basis, sectionIds: selectedIds })}>{submit.isPending ? 'Submitting…' : 'Confirm and submit application'}</button>{submit.isError && <div className="error-banner" role="alert">The application could not be submitted. It may already exist or the phase may have closed.</div>}</section>
  </div>;
}
