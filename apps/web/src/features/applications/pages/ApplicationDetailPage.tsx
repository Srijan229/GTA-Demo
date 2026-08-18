import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getMyApplication, withdrawApplication } from '../api/applicationApi';

export function ApplicationDetailPage() {
  const { id = '' } = useParams(); const client = useQueryClient(); const [reason, setReason] = useState(''); const [confirming, setConfirming] = useState(false);
  const query = useQuery({ queryKey: ['my-application', id], queryFn: () => getMyApplication(id), enabled: Boolean(id) });
  const withdraw = useMutation({ mutationFn: () => withdrawApplication(id, reason), onSuccess: data => { client.setQueryData(['my-application', id], data); void client.invalidateQueries({ queryKey: ['my-applications'] }); setConfirming(false); } });
  if (query.isPending) return <p role="status">Loading application details…</p>;
  if (query.isError) return <div className="error-banner" role="alert">This application could not be loaded.</div>;
  const application = query.data;
  return <div><Link to="/applicant/applications">← My applications</Link><header><span className="status-badge">{application.status}</span><h2>{application.reference}</h2><p>{application.phaseName} · {application.term}</p></header>
    <dl><dt>Employment basis</dt><dd>{application.employmentBasis}</dd><dt>Submitted</dt><dd>{new Date(application.submittedAtUtc).toLocaleString()}</dd></dl>
    <h3>Selected sections</h3><ol>{application.choices.map(choice => <li key={choice.sectionId}>{choice.courseCode}-{choice.sectionNumber}: {choice.courseTitle}</li>)}</ol>
    <h3>Status history</h3><ol>{application.statusHistory.map((entry, index) => <li key={`${entry.changedAtUtc}-${index}`}><strong>{entry.toStatus}</strong> — {new Date(entry.changedAtUtc).toLocaleString()}{entry.reason && <p>{entry.reason}</p>}</li>)}</ol>
    <section className="content-card"><h3>Withdraw application</h3>{application.canWithdraw ? confirming ? <div><label>Optional reason<textarea maxLength={500} value={reason} onChange={event => setReason(event.target.value)} /></label><p>Withdrawal is final and cannot be undone from the applicant portal.</p>{withdraw.isError && <div className="error-banner" role="alert">The application could not be withdrawn. Reload it to review its current status.</div>}<button type="button" disabled={withdraw.isPending} onClick={() => withdraw.mutate()}>Confirm withdrawal</button> <button type="button" onClick={() => setConfirming(false)}>Cancel</button></div> : <button type="button" onClick={() => setConfirming(true)}>Withdraw application</button> : <p>{application.withdrawalBlockedReason}</p>}</section>
  </div>;
}
