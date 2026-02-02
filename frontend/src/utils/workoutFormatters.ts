import type { TimeDomain, RepScheme, RepSchemeType } from '../types/workoutPreview';
import type { ParsedWorkout, ParsedMovement } from '../types/workout';

/**
 * Calculate time domain from time cap in seconds
 * @param timeCapSeconds - Time cap in seconds
 * @returns Time domain classification
 */
export function calculateTimeDomain(timeCapSeconds: number | null): TimeDomain {
  if (!timeCapSeconds) return 'medium';
  if (timeCapSeconds < 600) return 'short';      // < 10 minutes
  if (timeCapSeconds <= 1200) return 'medium';   // 10-20 minutes
  return 'long';                                  // > 20 minutes
}

/**
 * Calculate confidence score from parsed workout
 * @param workout - The parsed workout
 * @returns Confidence score between 0 and 1
 */
export function calculateConfidence(workout: ParsedWorkout): number {
  if (workout.isValid && workout.errors.length === 0) {
    // Check for unrecognized movements
    const unrecognizedCount = workout.movements.filter(m => !m.movementDefinitionId).length;
    const totalMovements = workout.movements.length;
    if (totalMovements === 0) return 0.5;
    return Math.max(0.6, 1 - (unrecognizedCount / totalMovements) * 0.4);
  }
  // Has errors - lower confidence
  const errorPenalty = Math.min(workout.errors.length * 0.15, 0.5);
  return Math.max(0.3, 0.8 - errorPenalty);
}

/**
 * Determine confidence level from score
 * @param score - Confidence score (0-1)
 * @returns Level classification
 */
export function getConfidenceLevel(score: number): 'high' | 'medium' | 'low' {
  if (score > 0.8) return 'high';
  if (score >= 0.6) return 'medium';
  return 'low';
}

/**
 * Detect rep scheme from movements
 * @param movements - Array of parsed movements
 * @param _roundCount - Number of rounds (if any) - reserved for future use
 * @returns Detected rep scheme or null
 */
export function detectRepScheme(
  movements: ParsedMovement[],
  _roundCount: number | null
): RepScheme | null {
  // Collect rep counts from movements
  const reps = movements
    .map(m => m.repCount)
    .filter((r): r is number => r !== null);

  if (reps.length === 0) return null;

  // Check if all same (fixed)
  if (reps.every(r => r === reps[0])) {
    return { type: 'fixed', reps };
  }

  // Check for descending (21-15-9)
  const isDescending = reps.every((r, i) => i === 0 || r < reps[i - 1]);
  if (isDescending) {
    return { type: 'descending', reps };
  }

  // Check for ascending (9-15-21)
  const isAscending = reps.every((r, i) => i === 0 || r > reps[i - 1]);
  if (isAscending) {
    return { type: 'ascending', reps };
  }

  // Custom pattern
  return { type: 'custom', reps };
}

/**
 * Format duration in seconds to human-readable string
 * @param seconds - Duration in seconds
 * @returns Formatted string like "20:00" or "1:30"
 */
export function formatDuration(seconds: number | null): string {
  if (!seconds) return '';
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}:${secs.toString().padStart(2, '0')}`;
}

/**
 * Get human-readable rep scheme label
 * @param type - Rep scheme type
 * @returns Label like "Descending Ladder"
 */
export function getRepSchemeLabel(type: RepSchemeType): string {
  const labels: Record<RepSchemeType, string> = {
    descending: 'Descending Ladder',
    ascending: 'Ascending Ladder',
    fixed: 'Fixed Reps',
    custom: 'Custom Pattern',
  };
  return labels[type];
}

/**
 * Get count of unrecognized movements
 * @param movements - Array of parsed movements
 * @returns Count of unrecognized movements
 */
export function getUnrecognizedCount(movements: ParsedMovement[]): number {
  return movements.filter(m => !m.movementDefinitionId).length;
}

/**
 * Format rep scheme as a string
 * @param scheme - Rep scheme
 * @returns Formatted string like "21-15-9"
 */
export function formatRepScheme(scheme: RepScheme): string {
  return scheme.reps.join('-');
}
