// Re-export movement types from dedicated movement.ts file for backward compatibility
export type { MovementCategory, MovementDefinition } from './movement';
export {
  MOVEMENT_CATEGORY_LABELS,
  MOVEMENT_CATEGORY_COLORS,
  ALL_MOVEMENT_CATEGORIES,
} from './movement';

/**
 * Workout type enum values matching backend WorkoutType.cs
 */
export type WorkoutType = 'Amrap' | 'ForTime' | 'Emom' | 'Intervals' | 'Rounds';

/**
 * Load unit enum values matching backend LoadUnit.cs
 */
export type LoadUnit = 'Kg' | 'Lb' | 'Pood';

/**
 * Distance unit enum values matching backend DistanceUnit.cs
 */
export type DistanceUnit = 'M' | 'Km' | 'Ft' | 'Mi' | 'Cal';

/**
 * Parsing error as returned from the API
 */
export interface ParsingError {
  errorType: string;
  message: string;
  lineNumber: number;
  originalText: string | null;
}

/**
 * Parsed movement as returned from the API
 */
export interface ParsedMovement {
  sequenceOrder: number;
  originalText: string;
  movementDefinitionId: number | null;
  movementName: string | null;
  movementCanonicalName: string | null;
  movementCategory: string | null;
  repCount: number | null;
  loadValue: number | null;
  loadValueFemale: number | null;
  loadUnit: string | null;
  loadFormatted: string | null;
  distanceValue: number | null;
  distanceUnit: string | null;
  distanceFormatted: string | null;
  calories: number | null;
  caloriesFemale: number | null;
  durationSeconds: number | null;
  durationFormatted: string | null;
  notes: string | null;
  repSchemeReps: number[] | null;
  repSchemeType: string | null;
}

/**
 * Parsed workout response from the API
 */
export interface ParsedWorkout {
  originalText: string;
  parsedDescription: string | null;
  workoutType: WorkoutType;
  timeCapSeconds: number | null;
  timeCapFormatted: string | null;
  roundCount: number | null;
  intervalDurationSeconds: number | null;
  intervalDurationFormatted: string | null;
  movements: ParsedMovement[];
  errors: ParsingError[];
  isValid: boolean;
  repSchemeReps: number[] | null;
  repSchemeType: string | null;
}

/**
 * Workout movement as returned from the API
 */
export interface WorkoutMovement {
  id: number;
  movementDefinitionId: number;
  movementName: string;
  movementCategory: string;
  sequenceOrder: number;
  repCount: number | null;
  loadValue: number | null;
  loadUnit: string | null;
  loadFormatted: string | null;
  distanceValue: number | null;
  distanceUnit: string | null;
  distanceFormatted: string | null;
  calories: number | null;
  durationSeconds: number | null;
  durationFormatted: string | null;
  notes: string | null;
}

/**
 * Workout as returned from the API
 */
export interface Workout {
  id: number;
  name: string | null;
  workoutType: WorkoutType;
  originalText: string;
  parsedDescription: string | null;
  timeCapSeconds: number | null;
  timeCapFormatted: string | null;
  roundCount: number | null;
  intervalDurationSeconds: number | null;
  intervalDurationFormatted: string | null;
  movements: WorkoutMovement[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Request payload for parsing workout text
 */
export interface ParseWorkoutRequest {
  text: string;
}

/**
 * Request payload for creating a workout movement
 */
export interface CreateWorkoutMovementRequest {
  movementDefinitionId: number;
  sequenceOrder: number;
  repCount?: number | null;
  loadValue?: number | null;
  loadUnit?: string | null;
  distanceValue?: number | null;
  distanceUnit?: string | null;
  calories?: number | null;
  durationSeconds?: number | null;
  notes?: string | null;
}

/**
 * Request payload for creating a workout
 */
export interface CreateWorkoutRequest {
  name?: string | null;
  workoutType: string;
  originalText: string;
  timeCapSeconds?: number | null;
  roundCount?: number | null;
  intervalDurationSeconds?: number | null;
  movements: CreateWorkoutMovementRequest[];
}

/**
 * Request payload for updating a workout
 */
export interface UpdateWorkoutRequest {
  name?: string | null;
  workoutType: string;
  timeCapSeconds?: number | null;
  roundCount?: number | null;
  intervalDurationSeconds?: number | null;
  movements?: CreateWorkoutMovementRequest[] | null;
}

/**
 * Form data for workout input
 */
export interface WorkoutInputFormData {
  text: string;
}

/**
 * Form data for workout edit
 */
export interface WorkoutEditFormData {
  name: string;
  workoutType: string;
  timeCapMinutes: string;
  roundCount: string;
  intervalDurationSeconds: string;
}

/**
 * Display labels for workout type options
 */
export const WORKOUT_TYPE_LABELS: Record<WorkoutType, string> = {
  Amrap: 'AMRAP',
  ForTime: 'For Time',
  Emom: 'EMOM',
  Intervals: 'Intervals',
  Rounds: 'Rounds',
};

/**
 * Badge color mapping for workout types
 */
export const WORKOUT_TYPE_COLORS: Record<WorkoutType, string> = {
  Amrap: 'blue',
  ForTime: 'green',
  Emom: 'purple',
  Intervals: 'yellow',
  Rounds: 'red',
};

/**
 * All workout types for filtering/selection
 */
export const ALL_WORKOUT_TYPES: WorkoutType[] = [
  'Amrap',
  'ForTime',
  'Emom',
  'Intervals',
  'Rounds',
];
