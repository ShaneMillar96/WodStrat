import { z } from 'zod';

/**
 * Valid workout types
 */
const VALID_WORKOUT_TYPES = ['Amrap', 'ForTime', 'Emom', 'Intervals', 'Rounds'] as const;

/**
 * Valid load units
 */
const VALID_LOAD_UNITS = ['Kg', 'Lb', 'Pood'] as const;

/**
 * Valid distance units
 */
const VALID_DISTANCE_UNITS = ['M', 'Km', 'Ft', 'Mi', 'Cal'] as const;

/**
 * Zod schema for workout text input form
 * Used for parsing workout text
 */
export const workoutInputSchema = z.object({
  /**
   * Raw workout text: required, not empty
   */
  text: z
    .string()
    .min(1, 'Workout text is required')
    .max(10000, 'Workout text must not exceed 10,000 characters'),
});

/**
 * Type inferred from the input schema
 */
export type WorkoutInputSchemaType = z.infer<typeof workoutInputSchema>;

/**
 * Zod schema for workout movement form
 */
export const workoutMovementSchema = z.object({
  movementDefinitionId: z
    .number()
    .int()
    .positive('Movement is required'),

  sequenceOrder: z
    .number()
    .int()
    .positive('Sequence order must be positive'),

  repCount: z
    .number()
    .int()
    .positive('Rep count must be positive')
    .nullable()
    .optional(),

  loadValue: z
    .number()
    .positive('Load value must be positive')
    .nullable()
    .optional(),

  loadUnit: z
    .enum(VALID_LOAD_UNITS)
    .nullable()
    .optional(),

  distanceValue: z
    .number()
    .positive('Distance value must be positive')
    .nullable()
    .optional(),

  distanceUnit: z
    .enum(VALID_DISTANCE_UNITS)
    .nullable()
    .optional(),

  calories: z
    .number()
    .int()
    .positive('Calories must be positive')
    .nullable()
    .optional(),

  durationSeconds: z
    .number()
    .int()
    .positive('Duration must be positive')
    .nullable()
    .optional(),

  notes: z
    .string()
    .max(200, 'Notes must be 200 characters or less')
    .nullable()
    .optional(),
});

/**
 * Zod schema for workout edit form
 * Used when editing a parsed workout before saving
 */
export const workoutEditSchema = z.object({
  /**
   * Workout name: optional, max 100 characters
   */
  name: z
    .string()
    .max(100, 'Name must be 100 characters or less')
    .optional()
    .transform(val => val?.trim() || undefined),

  /**
   * Workout type: required, valid enum
   */
  workoutType: z
    .enum(VALID_WORKOUT_TYPES, {
      message: 'Please select a valid workout type',
    }),

  /**
   * Time cap in minutes: optional, positive number (stored as string for form)
   */
  timeCapMinutes: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number(val) > 0),
      { message: 'Time cap must be a positive number' }
    ),

  /**
   * Round count: optional, positive integer (stored as string for form)
   */
  roundCount: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number.isInteger(Number(val)) && Number(val) > 0),
      { message: 'Round count must be a positive integer' }
    ),

  /**
   * Interval duration in seconds: optional, positive integer (stored as string for form)
   */
  intervalDurationSeconds: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number.isInteger(Number(val)) && Number(val) > 0),
      { message: 'Interval duration must be a positive integer' }
    ),

  /**
   * Movements: required, at least one movement
   */
  movements: z
    .array(workoutMovementSchema)
    .min(1, 'At least one movement is required'),
});

/**
 * Type inferred from the edit schema
 */
export type WorkoutEditSchemaType = z.infer<typeof workoutEditSchema>;

/**
 * Convert time cap minutes (form string) to seconds (API)
 * @param minutes - Time cap in minutes as string
 * @returns Time cap in seconds or null
 */
export function convertTimeCapToSeconds(minutes: string | undefined): number | null {
  if (!minutes) return null;
  const num = Number(minutes);
  return !isNaN(num) && num > 0 ? Math.round(num * 60) : null;
}

/**
 * Convert time cap seconds (API) to minutes (form string)
 * @param seconds - Time cap in seconds
 * @returns Time cap in minutes as string
 */
export function convertTimeCapToMinutes(seconds: number | null): string {
  if (!seconds) return '';
  return String(Math.round(seconds / 60));
}

/**
 * Format seconds to mm:ss display
 * @param totalSeconds - Total seconds
 * @returns Formatted time string
 */
export function formatSecondsDisplay(totalSeconds: number | null): string {
  if (!totalSeconds) return '';
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}
