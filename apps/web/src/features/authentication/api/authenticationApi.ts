import { apiRequest } from '../../../shared/api/http';

export type DevelopmentUser = {
  id: string;
  displayName: string;
  email: string;
  roles: string[];
  description: string;
};

export type CurrentUser = Omit<DevelopmentUser, 'description'>;

export function getDevelopmentUsers() {
  return apiRequest<DevelopmentUser[]>('/api/v1/development/users');
}

export function createDevelopmentSession(userId: string) {
  return apiRequest<void>(`/api/v1/development/session/${userId}`, { method: 'POST' });
}

export function getCurrentUser() {
  return apiRequest<CurrentUser>('/api/v1/auth/me');
}

export function deleteDevelopmentSession() {
  return apiRequest<void>('/api/v1/development/session', { method: 'DELETE' });
}
