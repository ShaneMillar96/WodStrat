/**
 * Workout type enum values matching backend WorkoutType.cs
 */
export type WorkoutType = 'Amrap' | 'ForTime' | 'Emom' | 'Intervals' | 'Rounds';

/**
 * Movement category enum values matching backend MovementCategory.cs
 */
export type MovementCategory = 'Weightlifting' | 'Gymnastics' | 'Cardio' | 'Strongman';

/**
 * Load unit enum values matching backend LoadUnit.cs
 */
export type LoadUnit = 'Kg' | 'Lb' | 'Pood';

/**
 * Distance unit enum values matching backend DistanceUnit.cs
 */
export type DistanceUnit = 'M' | 'Km' | 'Ft' | 'Mi' | 'Cal';

/**
 * Movement definition as returned from the API
 */
export interface MovementDefinition {
  id: number;
  canonicalName: string;
  displayName: string;
  category: MovementCategory;
  description: string | null;
  aliases: string[];
}

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
 * Display labels for movement category options
 */
export const MOVEMENT_CATEGORY_LABELS: Record<MovementCategory, string> = {
  Weightlifting: 'Weightlifting',
  Gymnastics: 'Gymnastics',
  Cardio: 'Cardio',
  Strongman: 'Strongman',
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
 * Badge color mapping for movement categories
 */
export const MOVEMENT_CATEGORY_COLORS: Record<MovementCategory, string> = {
  Weightlifting: 'red',
  Gymnastics: 'green',
  Cardio: 'blue',
  Strongman: 'purple',
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

/**
 * All movement categories for filtering
 */
export const ALL_MOVEMENT_CATEGORIES: MovementCategory[] = [
  'Weightlifting',
  'Gymnastics',
  'Cardio',
  'Strongman',
];
