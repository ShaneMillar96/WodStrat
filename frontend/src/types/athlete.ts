/**
 * Gender values matching backend validation
 */
export type Gender = 'Male' | 'Female' | 'Other' | 'PreferNotToSay';

/**
 * Experience level enum values matching backend
 */
export type ExperienceLevel = 'Beginner' | 'Intermediate' | 'Advanced';

/**
 * Athlete goal enum values matching backend AthleteGoal.cs
 */
export type AthleteGoal =
  | 'ImprovePacing'
  | 'PrepareForOpen'
  | 'CompetitionPrep'
  | 'BuildStrength'
  | 'ImproveConditioning'
  | 'WeightManagement'
  | 'GeneralFitness';

/**
 * Athlete entity as returned from the API
 */
export interface Athlete {
  id: string;
  name: string;
  age: number | null;
  gender: Gender | null;
  heightCm: number | null;
  weightKg: number | null;
  experienceLevel: ExperienceLevel;
  primaryGoal: AthleteGoal;
  createdAt: string;
  updatedAt: string;
}

/**
 * Form data for athlete profile form
 * Uses string types for form inputs
 */
export interface AthleteFormData {
  name: string;
  dateOfBirth: string;
  gender: Gender | '';
  heightCm: string;
  weightKg: string;
  experienceLevel: ExperienceLevel | '';
  primaryGoal: AthleteGoal | '';
}

/**
 * Request payload for creating a new athlete
 */
export interface CreateAthleteRequest {
  name: string;
  dateOfBirth?: string | null;
  gender?: Gender | null;
  heightCm?: number | null;
  weightKg?: number | null;
  experienceLevel: ExperienceLevel;
  primaryGoal: AthleteGoal;
}

/**
 * Request payload for updating an existing athlete
 */
export interface UpdateAthleteRequest {
  name: string;
  dateOfBirth?: string | null;
  gender?: Gender | null;
  heightCm?: number | null;
  weightKg?: number | null;
  experienceLevel: ExperienceLevel;
  primaryGoal: AthleteGoal;
}

/**
 * Display labels for gender options
 */
export const GENDER_LABELS: Record<Gender, string> = {
  Male: 'Male',
  Female: 'Female',
  Other: 'Other',
  PreferNotToSay: 'Prefer not to say',
};

/**
 * Display labels for experience level options
 */
export const EXPERIENCE_LEVEL_LABELS: Record<ExperienceLevel, string> = {
  Beginner: 'Beginner',
  Intermediate: 'Intermediate',
  Advanced: 'Advanced',
};

/**
 * Descriptions for experience level options (matching backend descriptions)
 */
export const EXPERIENCE_LEVEL_DESCRIPTIONS: Record<ExperienceLevel, string> = {
  Beginner: 'Less than 1 year of functional fitness',
  Intermediate: '1-3 years of experience',
  Advanced: '3+ years, competition experience',
};

/**
 * Display labels for athlete goal options (matching backend descriptions)
 */
export const ATHLETE_GOAL_LABELS: Record<AthleteGoal, string> = {
  ImprovePacing: 'Better workout pacing and consistency',
  PrepareForOpen: 'CrossFit Open preparation',
  CompetitionPrep: 'General competition preparation',
  BuildStrength: 'Focus on strength development',
  ImproveConditioning: 'Cardio/engine development',
  WeightManagement: 'Body composition goals',
  GeneralFitness: 'Overall health and fitness',
};
