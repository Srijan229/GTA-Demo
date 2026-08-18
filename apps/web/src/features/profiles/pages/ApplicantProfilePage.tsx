import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';
import type { InputHTMLAttributes } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { getProfile, getProfileCompletion, updateProfile } from '../api/profileApi';
import { profileSchema, type ProfileFormValues } from '../schemas/profileSchema';

const emptyEducation = { institution: '', degree: '', fieldOfStudy: '', startDate: '', endDate: '' };
const emptyExperience = { organization: '', title: '', description: '', startDate: '', endDate: '', isGtaExperience: false };

export function ApplicantProfilePage() {
  const queryClient = useQueryClient();
  const profile = useQuery({ queryKey: ['applicant-profile'], queryFn: getProfile });
  const completion = useQuery({ queryKey: ['profile-completion'], queryFn: getProfileCompletion });
  const form = useForm<ProfileFormValues>({ resolver: zodResolver(profileSchema), defaultValues: { education: [], experience: [], gpa: '', expectedGraduationYear: '' } });
  const education = useFieldArray({ control: form.control, name: 'education' });
  const experience = useFieldArray({ control: form.control, name: 'experience' });

  useEffect(() => {
    if (!profile.data) return;
    form.reset({
      preferredName: profile.data.preferredName ?? '', phoneNumber: profile.data.phoneNumber ?? '', program: profile.data.program ?? '',
      degree: profile.data.degree ?? '', major: profile.data.major ?? '', gpa: profile.data.gpa ?? '',
      expectedGraduationTerm: profile.data.expectedGraduationTerm ?? '', expectedGraduationYear: profile.data.expectedGraduationYear ?? '',
      linkedInUrl: profile.data.linkedInUrl ?? '',
      education: profile.data.education.map((item) => ({ ...item, degree: item.degree ?? '', fieldOfStudy: item.fieldOfStudy ?? '', startDate: item.startDate ?? '', endDate: item.endDate ?? '' })),
      experience: profile.data.experience.map((item) => ({ ...item, description: item.description ?? '', startDate: item.startDate ?? '', endDate: item.endDate ?? '' })),
    });
  }, [profile.data, form]);

  const save = useMutation({
    mutationFn: updateProfile,
    onSuccess: (data) => {
      queryClient.setQueryData(['applicant-profile'], data);
      void queryClient.invalidateQueries({ queryKey: ['profile-completion'] });
      form.reset(form.getValues());
    },
  });

  if (profile.isPending) return <p role="status">Loading your profile…</p>;
  if (profile.isError) return <div className="error-banner" role="alert">Your profile could not be loaded.</div>;

  const submit = form.handleSubmit((values) => save.mutate({
    ...values,
    gpa: values.gpa === '' ? null : Number(values.gpa), expectedGraduationYear: values.expectedGraduationYear === '' ? null : Number(values.expectedGraduationYear),
    linkedInUrl: values.linkedInUrl || null,
  }));

  return (
    <form className="profile-workspace" onSubmit={(event) => void submit(event)} noValidate>
      <aside className="profile-summary" aria-label="Profile completion">
        <p className="role-label">Profile completion</p><strong className="completion-value">{completion.data?.percentage ?? 0}%</strong>
        <progress max="100" value={completion.data?.percentage ?? 0}>{completion.data?.percentage ?? 0}%</progress>
        {completion.data?.incompleteSections.length ? <p>Still needed: {completion.data.incompleteSections.join(', ')}</p> : <p>All profile sections are complete.</p>}
        <nav className="profile-section-nav" aria-label="GTA profile sections"><a href="#personal-information">Personal information</a><a href="#academic-information">Academic information</a><a href="#education">Education</a><a href="#experience">Experience</a><Link to="/applicant/documents">Resume and transcript</Link></nav>
      </aside>
      <div className="profile-form">
        <header><h2>My profile</h2><p>Keep your applicant information current. Fields marked required are used to calculate completion.</p></header>
        <fieldset id="personal-information"><legend>Personal information</legend><div className="form-grid">
          <Field label="Legal name" value={profile.data.displayName} readOnly />
          <Field label="University ID" value={profile.data.universityId ?? ''} readOnly />
          <Field label="Email" value={profile.data.email} readOnly />
          <Field label="Preferred name" error={form.formState.errors.preferredName?.message} {...form.register('preferredName')} />
          <Field label="Phone number" error={form.formState.errors.phoneNumber?.message} {...form.register('phoneNumber')} />
          <Field label="LinkedIn URL" error={form.formState.errors.linkedInUrl?.message} {...form.register('linkedInUrl')} />
        </div></fieldset>
        <fieldset id="academic-information"><legend>Academic information</legend><div className="form-grid">
          <Field label="Program" required error={form.formState.errors.program?.message} {...form.register('program')} />
          <Field label="Degree" required error={form.formState.errors.degree?.message} {...form.register('degree')} />
          <Field label="Major" required error={form.formState.errors.major?.message} {...form.register('major')} />
          <Field label="GPA" type="number" step="0.01" min="0" max="4" error={form.formState.errors.gpa?.message} {...form.register('gpa')} />
          <Field label="Expected graduation term" error={form.formState.errors.expectedGraduationTerm?.message} {...form.register('expectedGraduationTerm')} />
          <Field label="Expected graduation year" type="number" error={form.formState.errors.expectedGraduationYear?.message} {...form.register('expectedGraduationYear')} />
        </div></fieldset>
        <fieldset id="education"><legend>Education</legend>{education.fields.map((item, index) => <div className="repeatable-card" key={item.id}>
          <div className="form-grid"><Field label="Institution" required error={form.formState.errors.education?.[index]?.institution?.message} {...form.register(`education.${index}.institution`)} /><Field label="Degree" {...form.register(`education.${index}.degree`)} /><Field label="Field of study" {...form.register(`education.${index}.fieldOfStudy`)} /><Field label="Start date" type="date" {...form.register(`education.${index}.startDate`)} /><Field label="End date" type="date" error={form.formState.errors.education?.[index]?.endDate?.message} {...form.register(`education.${index}.endDate`)} /></div>
          <button className="text-button danger" type="button" onClick={() => education.remove(index)}>Remove education</button></div>)}
          <button className="secondary-button" type="button" onClick={() => education.append(emptyEducation)}>Add education</button>
        </fieldset>
        <fieldset id="experience"><legend>Experience</legend>{experience.fields.map((item, index) => <div className="repeatable-card" key={item.id}>
          <div className="form-grid"><Field label="Organization" required error={form.formState.errors.experience?.[index]?.organization?.message} {...form.register(`experience.${index}.organization`)} /><Field label="Title" required error={form.formState.errors.experience?.[index]?.title?.message} {...form.register(`experience.${index}.title`)} /><Field label="Start date" type="date" {...form.register(`experience.${index}.startDate`)} /><Field label="End date" type="date" error={form.formState.errors.experience?.[index]?.endDate?.message} {...form.register(`experience.${index}.endDate`)} /><label className="checkbox-field"><input type="checkbox" {...form.register(`experience.${index}.isGtaExperience`)} /> GTA experience</label></div>
          <label className="field full-width"><span>Description</span><textarea rows={3} {...form.register(`experience.${index}.description`)} /></label>
          <button className="text-button danger" type="button" onClick={() => experience.remove(index)}>Remove experience</button></div>)}
          <button className="secondary-button" type="button" onClick={() => experience.append(emptyExperience)}>Add experience</button>
        </fieldset>
        {save.isError && <div className="error-banner" role="alert">Profile changes could not be saved.</div>}
        {save.isSuccess && !form.formState.isDirty && <p className="success-message" role="status">Profile changes saved.</p>}
        <div className="sticky-actions"><span>{form.formState.isDirty ? 'Unsaved changes' : `Last saved ${new Date(profile.data.updatedAtUtc).toLocaleString()}`}</span><button className="button" disabled={save.isPending || !form.formState.isDirty} type="submit">{save.isPending ? 'Saving…' : 'Save changes'}</button></div>
      </div>
    </form>
  );
}

type FieldProps = InputHTMLAttributes<HTMLInputElement> & { label: string; error?: string | undefined; required?: boolean | undefined };
function Field({ label, error, required, ...props }: FieldProps) {
  const id = props.name ?? label.toLowerCase().replaceAll(' ', '-');
  return <label className="field" htmlFor={id}><span>{label}{required ? ' *' : ''}</span><input id={id} aria-invalid={Boolean(error)} aria-describedby={error ? `${id}-error` : undefined} {...props} />{error && <small className="field-error" id={`${id}-error`}>{error}</small>}</label>;
}
