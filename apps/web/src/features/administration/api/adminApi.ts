import { apiRequest } from '../../../shared/api/http';

export type AdminDashboard = { applications: number; applicants: number; activeSections: number; faculty: number; awaitingReview: number; unassignedSections: number; warnings: string[] };
export type AdminApplication = { id: string; reference: string; applicantName: string; program: string; state: string; submittedAtUtc?: string | null; choiceCount: number };
export type AdminApplicant = { userId: string; displayName: string; email: string; universityId?: string | null; program?: string | null; profileComplete: boolean; applicationCount: number };
export type AdminSection = { id: string; courseCode: string; title: string; sectionNumber: string; term: string; availablePositions?: number | null; isActive: boolean; facultyUserId?: string | null; facultyName?: string | null };
export type AdminPhase = { id: string; name: string; program: string; term: string; opensAtUtc: string; closesAtUtc: string; isActive: boolean };
export type AdminUser = { id: string; displayName: string; email: string; isActive: boolean; roles: string[] };
export type AdminSetting = { key: string; value: string; description?: string | null; isDevelopmentOnly: boolean; updatedAtUtc: string };
export type AdminAudit = { id: string; occurredAtUtc: string; action: string; entityType: string; entityReference?: string | null; result: string; correlationId: string };
export type PlacementCandidate = { choiceId: string; applicationId: string; reference: string; applicantName: string; employmentBasis: string; assignmentState: string; activePlacements: number; maximumPlacements: number; sectionId: string; courseCode: string; sectionNumber: string; term: string; availablePositions?: number | null; filledPositions: number; isPlacedHere: boolean };
export type PlacementAction = { choiceId: string; active: boolean; assignmentState: string; activePlacements: number; maximumPlacements: number; changedAtUtc: string };
export type SectionImportError = { rowNumber: number; message: string }; export type SectionImportResult = { id?: string; fileName?: string; importedAtUtc?: string; totalRows: number; acceptedRows: number; rejectedRows: number; errors: SectionImportError[] };
export type EmailDelivery = { id:string;recipient:string;subject:string;state:string;attemptCount:number;createdAtUtc:string;sentAtUtc?:string|null;lastError?:string|null;correlationId?:string|null };

const get = <T>(resource: string) => apiRequest<T>(`/api/v1/admin/${resource}`);
const put = (path: string, body: unknown) => apiRequest<void>(`/api/v1/admin/${path}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
export const adminApi = {
  dashboard: () => get<AdminDashboard>('dashboard'), applications: () => get<AdminApplication[]>('applications'), applicants: () => get<AdminApplicant[]>('applicants'), sections: () => get<AdminSection[]>('sections'), phases: () => get<AdminPhase[]>('phases'), users: () => get<AdminUser[]>('users'), settings: () => get<AdminSetting[]>('settings'), audit: () => get<AdminAudit[]>('audit'), placements: () => get<PlacementCandidate[]>('placements'),
  updateSection: (section: AdminSection) => put(`sections/${section.id}`, { isActive: section.isActive, availablePositions: section.availablePositions }),
  assignFaculty: (id: string, facultyUserId?: string | null) => put(`sections/${id}/faculty`, { facultyUserId: facultyUserId || null }),
  updatePhase: (phase: AdminPhase) => put(`phases/${phase.id}`, { opensAtUtc: phase.opensAtUtc, closesAtUtc: phase.closesAtUtc, isActive: phase.isActive }),
  updateUser: (user: AdminUser) => put(`users/${user.id}`, { isActive: user.isActive, roles: user.roles }),
  updateSetting: (key: string, value: string) => put(`settings/${encodeURIComponent(key)}`, { value }),
  updatePlacement: (choiceId: string, active: boolean) => apiRequest<PlacementAction>(`/api/v1/admin/placements/${choiceId}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ active }) }),
  importHistory: () => get<SectionImportResult[]>('section-imports'),
  emailDeliveries: () => get<EmailDelivery[]>('email-deliveries'),
  uploadImport: (file: File, preview: boolean) => { const data=new FormData(); data.append('file',file); return apiRequest<SectionImportResult>(`/api/v1/admin/section-imports${preview?'/preview':''}`,{method:'POST',body:data}); },
};
