import { z } from 'zod';

const optionalText = z.string().trim().max(250).optional();
const validDateRange = (value: { startDate?: string | undefined; endDate?: string | undefined }) => !value.startDate || !value.endDate || value.startDate <= value.endDate;

const educationSchema = z.object({
  id: z.string().uuid().optional(), institution: z.string().trim().min(1, 'Institution is required.').max(200), degree: optionalText,
  fieldOfStudy: optionalText, startDate: z.string().optional(), endDate: z.string().optional(),
}).refine(validDateRange, { message: 'Start date cannot be after end date.', path: ['endDate'] });

const experienceSchema = z.object({
  id: z.string().uuid().optional(), organization: z.string().trim().min(1, 'Organization is required.').max(200),
  title: z.string().trim().min(1, 'Title is required.').max(200), description: z.string().trim().max(2000).optional(),
  startDate: z.string().optional(), endDate: z.string().optional(), isGtaExperience: z.boolean(),
}).refine(validDateRange, { message: 'Start date cannot be after end date.', path: ['endDate'] });

export const profileSchema = z.object({
  preferredName: optionalText, phoneNumber: z.string().trim().max(30).optional(), program: optionalText, degree: optionalText,
  major: optionalText, gpa: z.union([z.literal(''), z.coerce.number().min(0).max(4)]), expectedGraduationTerm: optionalText,
  expectedGraduationYear: z.union([z.literal(''), z.coerce.number().int().min(2000).max(2100)]),
  linkedInUrl: z.union([z.literal(''), z.url('Enter a valid URL.')]), education: z.array(educationSchema), experience: z.array(experienceSchema),
});

export type ProfileFormValues = z.input<typeof profileSchema>;
