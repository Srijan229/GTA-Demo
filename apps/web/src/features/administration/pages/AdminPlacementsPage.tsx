import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { adminApi } from '../api/adminApi';

export function AdminPlacementsPage() {
  const client = useQueryClient();
  const [feedback, setFeedback] = useState<string>();
  const query = useQuery({ queryKey: ['admin-placements'], queryFn: adminApi.placements });
  const update = useMutation({
    mutationFn: ({ choiceId, active }: { choiceId: string; active: boolean }) => adminApi.updatePlacement(choiceId, active),
    onSuccess: result => { setFeedback(`Placement updated. Applicant is now ${result.assignmentState}.`); void client.invalidateQueries({ queryKey: ['admin-placements'] }); void client.invalidateQueries({ queryKey: ['admin-dashboard'] }); },
    onError: () => setFeedback('The placement could not be changed. Reload the current data and check workload and section capacity.'),
  });
  if (query.isPending) return <p role="status">Loading placement candidates…</p>;
  if (query.isError) return <div className="error-banner" role="alert">Placement candidates could not be loaded.</div>;
  return <div>
    <p>Only choices with an active faculty hire recommendation appear here. Assignment totals and section capacity are enforced by the server.</p>
    {feedback && <p role="status">{feedback}</p>}
    {query.data.length === 0 ? <p>No applicants are currently recommended for placement.</p> : <div className="table-scroll"><table><thead><tr><th>Applicant</th><th>Employment</th><th>Assignment state</th><th>Recommended section</th><th>Capacity</th><th>Action</th></tr></thead><tbody>{query.data.map(item => <tr key={item.choiceId}><td>{item.applicantName}<br /><small>{item.reference}</small></td><td>{item.employmentBasis}</td><td>{item.assignmentState} ({item.activePlacements}/{item.maximumPlacements})</td><td>{item.courseCode}-{item.sectionNumber}<br /><small>{item.term}</small></td><td>{item.filledPositions}/{item.availablePositions ?? 'Unlimited'}</td><td><button type="button" disabled={update.isPending} onClick={() => update.mutate({ choiceId: item.choiceId, active: !item.isPlacedHere })}>{item.isPlacedHere ? 'Remove placement' : 'Place applicant'}</button></td></tr>)}</tbody></table></div>}
  </div>;
}
