import { z } from 'zod';

/**
 * Helper to calculate age from date of birth
 */
function calculateAge(dateOfBirth: Date): number {
  const today = new Date();
  let age = today.getFullYear() - dateOfBirth.getFullYear();
  const monthDiff = today.getMonth() - dateOfBirth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dateOfBirth.getDate())) {
    age--;
  }
  return age;
}

/**
 * Gender values for validation (matching backend)
 */
const genderValues = ['Male', 'Female', 'Other', 'PreferNotToSay'] as const;

/**
 * Experience level enum values for validation (matching backend)
 */
const experienceLevelValues = ['Beginner', 'Intermediate', 'Advanced'] as const;

/**
 * Athlete goal enum values for validation (matching backend AthleteGoal.cs)
 */
const athleteGoalValues = [
  'ImprovePacing',
  'PrepareForOpen',
  'CompetitionPrep',
  'BuildStrength',
  'ImproveConditioning',
  'WeightManagement',
  'GeneralFitness',
] as const;

/**
 * Zod schema for athlete profile form
 * Matches backend FluentValidation rules
 */
export const athleteSchema = z.object({
  /**
   * Name: required, 1-100 characters
   */
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must be 100 characters or less'),

  /**
   * Date of Birth: optional, but if provided must be:
   * - A valid past date
   * - Age between 13 and 120 years
   */
  dateOfBirth: z
    .string()
    .optional()
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const date = new Date(val);
        return !isNaN(date.getTime());
      },
      { message: 'Invalid date format' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const date = new Date(val);
        return date < new Date();
      },
      { message: 'Date of birth must be in the past' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const date = new Date(val);
        const age = calculateAge(date);
        return age >= 13;
      },
      { message: 'You must be at least 13 years old' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const date = new Date(val);
        const age = calculateAge(date);
        return age <= 120;
      },
      { message: 'Age cannot exceed 120 years' }
    ),

  /**
   * Gender: optional enum
   */
  gender: z
    .enum(genderValues)
    .optional(),

  /**
   * Height: optional, 50-300 cm
   */
  heightCm: z
    .string()
    .optional()
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return !isNaN(num);
      },
      { message: 'Height must be a valid number' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return num >= 50;
      },
      { message: 'Height must be at least 50 cm' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return num <= 300;
      },
      { message: 'Height must be 300 cm or less' }
    ),

  /**
   * Weight: optional, 20-500 kg
   */
  weightKg: z
    .string()
    .optional()
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return !isNaN(num);
      },
      { message: 'Weight must be a valid number' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return num >= 20;
      },
      { message: 'Weight must be at least 20 kg' }
    )
    .refine(
      (val) => {
        if (!val || val === '') return true;
        const num = parseFloat(val);
        return num <= 500;
      },
      { message: 'Weight must be 500 kg or less' }
    ),

  /**
   * Experience Level: required enum
   */
  experienceLevel: z.enum(experienceLevelValues, {
    message: 'Please select your experience level',
  }),

  /**
   * Primary Goal: required enum
   */
  primaryGoal: z.enum(athleteGoalValues, {
    message: 'Please select your primary goal',
  }),
});

/**
 * Type inferred from the schema
 */
export type AthleteSchemaType = z.infer<typeof athleteSchema>;
