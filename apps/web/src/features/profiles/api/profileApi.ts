import { apiRequest } from '../../../shared/api/http';

export type EducationRecord = { id?: string | undefined; institution: string; degree?: string | null | undefined; fieldOfStudy?: string | null | undefined; startDate?: string | null | undefined; endDate?: string | null | undefined };
export type ExperienceRecord = { id?: string | undefined; organization: string; title: string; description?: string | null | undefined; startDate?: string | null | undefined; endDate?: string | null | undefined; isGtaExperience: boolean };
export type ApplicantProfile = {
  displayName: string; email: string; universityId?: string | null | undefined; preferredName?: string | null | undefined; phoneNumber?: string | null | undefined;
  program?: string | null | undefined; degree?: string | null | undefined; major?: string | null | undefined; gpa?: number | null | undefined; expectedGraduationTerm?: string | null | undefined;
  expectedGraduationYear?: number | null | undefined; linkedInUrl?: string | null | undefined; education: EducationRecord[]; experience: ExperienceRecord[]; updatedAtUtc: string;
};
export type ProfileCompletion = { percentage: number; completedSections: string[]; incompleteSections: string[] };
export type UpdateApplicantProfile = Omit<ApplicantProfile, 'displayName' | 'email' | 'universityId' | 'updatedAtUtc'>;

export const getProfile = () => apiRequest<ApplicantProfile>('/api/v1/profile/me/');
export const getProfileCompletion = () => apiRequest<ProfileCompletion>('/api/v1/profile/me/completion');
export const updateProfile = (profile: UpdateApplicantProfile) => apiRequest<ApplicantProfile>('/api/v1/profile/me/', { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(profile) });
