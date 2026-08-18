import { apiRequest } from '../../../shared/api/http';
import type { ApplicantDocument } from '../../documents/api/documentApi';
import type { ApplicantProfile } from '../../profiles/api/profileApi';

export type FacultySection = { id: string; courseCode: string; sectionNumber: string; courseTitle: string; term: string; schedule?: string | null; applicationCount: number };
export type FacultyApplication = { choiceId: string; applicationId: string; applicantName: string; program: string; courseCode: string; sectionNumber: string; status: string; submittedAtUtc: string; interviewMarked: boolean; hireRecommended: boolean };
export type FacultyReview = { choiceId: string; applicationId: string; reference: string; status: string; employmentBasis: string; courseCode: string; sectionNumber: string; submittedAtUtc: string; profile: ApplicantProfile; documents: ApplicantDocument[]; interviewMarked: boolean; hireRecommended: boolean; internalNotes?: string | null };
export type FacultyAction = { action: 'Interview' | 'HireRecommendation'; active: boolean; internalNotes?: string | null };
export type FacultyInterview = { choiceId:string;applicationId:string;applicantName:string;program:string;courseCode:string;sectionNumber:string;term:string;applicationStatus:string;employmentBasis:string;interviewMarkedAtUtc:string;hireRecommended:boolean;activePlacements:number;maximumPlacements:number;decisionState:'AwaitingDecision'|'HireRecommended'|'Placed' };

export const getFacultySections = () => apiRequest<FacultySection[]>('/api/v1/faculty/sections');
export const getFacultyApplications = () => apiRequest<FacultyApplication[]>('/api/v1/faculty/applications');
export const getFacultyInterviews = () => apiRequest<FacultyInterview[]>('/api/v1/faculty/interviews');
export const getFacultyReview = (choiceId: string) => apiRequest<FacultyReview>(`/api/v1/faculty/applications/${choiceId}`);
export const recordFacultyAction = (choiceId: string, action: FacultyAction) => apiRequest(`/api/v1/faculty/applications/${choiceId}/actions`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(action) });
export const facultyDocumentUrl = (id: string) => `/api/v1/faculty/documents/${id}/content`;
