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
 * Determine rep scheme type from array of reps
 * @param reps - Array of rep counts
 * @returns The type of rep scheme
 */
function determineRepSchemeType(reps: number[]): RepSchemeType {
  // All same (fixed)
  if (reps.every(r => r === reps[0])) {
    return 'fixed';
  }
  // Descending (21-15-9)
  if (reps.every((r, i) => i === 0 || r < reps[i - 1])) {
    return 'descending';
  }
  // Ascending (9-15-21)
  if (reps.every((r, i) => i === 0 || r > reps[i - 1])) {
    return 'ascending';
  }
  // Custom pattern
  return 'custom';
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
  // First, check if movements have repSchemeReps from the backend
  // Use the first movement's repSchemeReps as the workout-level scheme
  const movementWithScheme = movements.find(m => m.repSchemeReps && m.repSchemeReps.length > 0);
  if (movementWithScheme && movementWithScheme.repSchemeReps) {
    const reps = movementWithScheme.repSchemeReps;
    const type = determineRepSchemeType(reps);
    return { type, reps };
  }

  // Fallback: Collect rep counts from movements (for workouts without backend rep scheme)
  const reps = movements
    .map(m => m.repCount)
    .filter((r): r is number => r !== null);

  if (reps.length === 0) return null;

  const type = determineRepSchemeType(reps);
  return { type, reps };
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
