import { apiRequest } from '../../../shared/api/http';

export type ApplicantDocument = { id: string; type: 'Resume' | 'UnofficialTranscript'; originalFileName: string; mediaType: string; byteLength: number; version: number; uploadedAtUtc: string };

export const getDocuments = () => apiRequest<ApplicantDocument[]>('/api/v1/documents/');
export function uploadDocument(type: ApplicantDocument['type'], file: File) {
  const data = new FormData();
  data.append('file', file);
  return apiRequest<ApplicantDocument>(`/api/v1/documents/${type}`, { method: 'POST', body: data });
}
export const documentContentUrl = (id: string) => `/api/v1/documents/${id}/content`;
