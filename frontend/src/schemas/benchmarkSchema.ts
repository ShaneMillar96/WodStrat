import { z } from 'zod';

/**
 * Parse time string (mm:ss or m:ss) to total seconds
 * @param timeStr - Time string in mm:ss format
 * @returns Total seconds or null if invalid
 */
export function parseTimeToSeconds(timeStr: string): number | null {
  // Handle already numeric input
  const numericValue = parseFloat(timeStr);
  if (!isNaN(numericValue) && !timeStr.includes(':')) {
    return numericValue > 0 ? numericValue : null;
  }

  // Handle mm:ss format
  const timeMatch = timeStr.match(/^(\d{1,3}):(\d{2})$/);
  if (timeMatch) {
    const minutes = parseInt(timeMatch[1], 10);
    const seconds = parseInt(timeMatch[2], 10);
    if (seconds < 60) {
      return minutes * 60 + seconds;
    }
  }

  return null;
}

/**
 * Format seconds to mm:ss display format
 * @param seconds - Total seconds
 * @returns Formatted time string
 */
export function formatSecondsToTime(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = Math.round(seconds % 60);
  return `${mins}:${secs.toString().padStart(2, '0')}`;
}

/**
 * Zod schema for benchmark form
 * Handles validation for benchmark entry/edit
 */
export const benchmarkSchema = z.object({
  /**
   * Benchmark definition ID: required
   */
  benchmarkDefinitionId: z
    .string()
    .min(1, 'Please select a benchmark'),

  /**
   * Value: required, positive number or time format
   * Will be converted to number before submission
   */
  value: z
    .string()
    .min(1, 'Value is required')
    .refine(
      (val) => {
        // Allow time format (mm:ss)
        if (val.includes(':')) {
          return parseTimeToSeconds(val) !== null;
        }
        // Allow positive numbers
        const num = parseFloat(val);
        return !isNaN(num) && num > 0;
      },
      { message: 'Enter a valid positive number or time (mm:ss)' }
    ),

  /**
   * Recorded date: required, not in future
   */
  recordedAt: z
    .string()
    .min(1, 'Date is required')
    .refine(
      (val) => {
        const date = new Date(val);
        return !isNaN(date.getTime());
      },
      { message: 'Invalid date format' }
    )
    .refine(
      (val) => {
        const date = new Date(val);
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        return date <= today;
      },
      { message: 'Date cannot be in the future' }
    ),

  /**
   * Notes: optional, max 500 characters
   * Using string type with empty default for form compatibility
   */
  notes: z
    .string()
    .max(500, 'Notes must be 500 characters or less'),
});

/**
 * Type inferred from the schema
 */
export type BenchmarkSchemaType = z.infer<typeof benchmarkSchema>;

/**
 * Convert form value string to numeric value based on metric type
 * @param valueStr - The string value from the form
 * @param metricType - The type of metric (Time, Reps, Weight, Pace)
 * @returns Numeric value in the appropriate unit
 */
export function convertFormValueToNumber(
  valueStr: string,
  metricType: string
): number {
  if (metricType === 'Time' || metricType === 'Pace') {
    // For time/pace, parse mm:ss to seconds
    const seconds = parseTimeToSeconds(valueStr);
    return seconds ?? parseFloat(valueStr);
  }
  // For reps and weight, just parse as number
  return parseFloat(valueStr);
}

/**
 * Convert numeric value to form display string based on metric type
 * @param value - The numeric value
 * @param metricType - The type of metric
 * @returns Display string for the form input
 */
export function convertNumberToFormValue(
  value: number,
  metricType: string
): string {
  if (metricType === 'Time' || metricType === 'Pace') {
    return formatSecondsToTime(value);
  }
  return value.toString();
}
