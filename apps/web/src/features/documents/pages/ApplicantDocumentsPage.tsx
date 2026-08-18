import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRef, useState } from 'react';
import { documentContentUrl, getDocuments, uploadDocument, type ApplicantDocument } from '../api/documentApi';

type DocumentKind = ApplicantDocument['type'];
const definitions: { type: DocumentKind; title: string; help: string; accept: string }[] = [
  { type: 'Resume', title: 'Resume', help: 'PDF or DOCX, up to 10 MB', accept: '.pdf,.docx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document' },
  { type: 'UnofficialTranscript', title: 'Unofficial transcript', help: 'PDF, up to 10 MB', accept: '.pdf,application/pdf' },
];

export function ApplicantDocumentsPage() {
  const documents = useQuery({ queryKey: ['applicant-documents'], queryFn: getDocuments });
  if (documents.isPending) return <p role="status">Loading your documents…</p>;
  if (documents.isError) return <div className="error-banner" role="alert">Your documents could not be loaded.</div>;
  return <div><header><h2>Documents</h2><p>Upload the documents required for your GTA application. Files are available only through authorized requests.</p></header><div className="document-grid">{definitions.map((definition) => <DocumentCard key={definition.type} definition={definition} document={documents.data.find((item) => item.type === definition.type)} />)}</div></div>;
}

function DocumentCard({ definition, document }: { definition: (typeof definitions)[number]; document?: ApplicantDocument | undefined }) {
  const queryClient = useQueryClient();
  const input = useRef<HTMLInputElement>(null);
  const [candidate, setCandidate] = useState<File>();
  const [validationError, setValidationError] = useState<string>();
  const upload = useMutation({ mutationFn: (file: File) => uploadDocument(definition.type, file), onSuccess: () => { setCandidate(undefined); if (input.current) input.current.value = ''; void queryClient.invalidateQueries({ queryKey: ['applicant-documents'] }); } });

  function select(file?: File) {
    setValidationError(undefined);
    if (!file) return;
    if (file.size > 10 * 1024 * 1024) { setValidationError('File must be 10 MB or smaller.'); return; }
    const extension = file.name.toLowerCase().split('.').pop();
    if (definition.type === 'UnofficialTranscript' && extension !== 'pdf') { setValidationError('Transcript must be a PDF file.'); return; }
    if (definition.type === 'Resume' && extension !== 'pdf' && extension !== 'docx') { setValidationError('Resume must be a PDF or DOCX file.'); return; }
    setCandidate(file);
  }

  return <article className="document-card">
    <div><p className="role-label">{definition.title}</p>{document ? <><h3>{document.originalFileName}</h3><dl className="document-meta"><div><dt>Uploaded</dt><dd>{new Date(document.uploadedAtUtc).toLocaleString()}</dd></div><div><dt>Size</dt><dd>{formatBytes(document.byteLength)}</dd></div><div><dt>Version</dt><dd>{document.version}</dd></div></dl></> : <><h3>No document uploaded</h3><p>Add this document before submitting an application.</p></>}</div>
    <div className="upload-zone" onDragOver={(event) => event.preventDefault()} onDrop={(event) => { event.preventDefault(); select(event.dataTransfer.files[0]); }}>
      <input className="visually-hidden" ref={input} type="file" accept={definition.accept} id={`file-${definition.type}`} onChange={(event) => select(event.target.files?.[0])} />
      <label className="secondary-button" htmlFor={`file-${definition.type}`}>{document ? 'Choose replacement' : 'Choose file'}</label><span> or drag and drop</span><small>{definition.help}</small>
    </div>
    {candidate && <div className="selected-file"><span>{candidate.name} ({formatBytes(candidate.size)})</span><button className="button" disabled={upload.isPending} type="button" onClick={() => upload.mutate(candidate)}>{upload.isPending ? 'Uploading…' : document ? 'Replace document' : 'Upload document'}</button></div>}
    {document && <a className="secondary-button download-link" href={documentContentUrl(document.id)}>Download</a>}
    {validationError && <p className="field-error" role="alert">{validationError}</p>}
    {upload.isError && <p className="field-error" role="alert">Upload failed. Confirm the file is valid and try again.</p>}
    {upload.isSuccess && <p className="success-message" role="status">{definition.title} uploaded successfully.</p>}
  </article>;
}

function formatBytes(bytes: number) { return bytes < 1024 * 1024 ? `${Math.max(1, Math.round(bytes / 1024))} KB` : `${(bytes / 1024 / 1024).toFixed(1)} MB`; }
