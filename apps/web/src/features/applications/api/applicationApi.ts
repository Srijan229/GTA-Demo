import { apiRequest } from '../../../shared/api/http';

export type AvailableSection = { id: string; phaseId: string; phaseName: string; term: string; courseCode: string; courseTitle: string; sectionNumber: string; schedule?: string | null; deliveryMethod?: string | null; availablePositions?: number | null; alreadyApplied: boolean };
export type ApplicationChoice = { sectionId: string; courseCode: string; sectionNumber: string; courseTitle: string };
export type ApplicantApplication = { id: string; reference: string; phaseName: string; term: string; employmentBasis: string; status: string; submittedAtUtc: string; choices: ApplicationChoice[] };
export type ApplicationStatusHistory = { fromStatus: string; toStatus: string; changedAtUtc: string; reason?: string | null };
export type ApplicantApplicationDetail = ApplicantApplication & { statusHistory: ApplicationStatusHistory[]; canWithdraw: boolean; withdrawalBlockedReason?: string | null };
export type SubmitApplication = { phaseId: string; employmentBasis: 'PartTime10Hours' | 'FullTime20Hours'; sectionIds: string[] };

export const getAvailableSections = () => apiRequest<AvailableSection[]>('/api/v1/applications/available-sections');
export const getApplicationConfiguration = () => apiRequest<{maximumSectionChoices:number}>('/api/v1/applications/configuration');
export const getMyApplications = () => apiRequest<ApplicantApplication[]>('/api/v1/applications/mine');
export const getMyApplication = (id: string) => apiRequest<ApplicantApplicationDetail>(`/api/v1/applications/mine/${id}`);
export const withdrawApplication = (id: string, reason?: string) => apiRequest<ApplicantApplicationDetail>(`/api/v1/applications/mine/${id}/withdraw`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ reason: reason || null }) });
export const submitApplication = (request: SubmitApplication) => apiRequest<ApplicantApplication>('/api/v1/applications/', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(request) });
