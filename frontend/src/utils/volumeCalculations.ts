import type { MovementVolume } from '../types/workoutPreview';
import type { ParsedMovement } from '../types/workout';

/**
 * Calculate total volume for each movement
 * @param movements - Array of parsed movements
 * @param roundCount - Number of rounds (multiplier)
 * @returns Array of movement volumes
 */
export function calculateVolumes(
  movements: ParsedMovement[],
  roundCount: number | null
): MovementVolume[] {
  const multiplier = roundCount || 1;

  return movements
    .filter(m => m.movementDefinitionId !== null) // Only recognized movements
    .map(m => ({
      movementName: m.movementName || m.originalText,
      totalReps: (m.repCount || 0) * multiplier,
      totalDistance: m.distanceValue ? m.distanceValue * multiplier : undefined,
      totalCalories: m.calories ? m.calories * multiplier : undefined,
    }));
}

/**
 * Get the maximum volume value for scaling bars
 * @param volumes - Array of movement volumes
 * @returns Maximum rep count
 */
export function getMaxVolume(volumes: MovementVolume[]): number {
  if (volumes.length === 0) return 0;
  return Math.max(...volumes.map(v => v.totalReps));
}

/**
 * Calculate percentage for volume bar width
 * @param value - Current value
 * @param max - Maximum value
 * @returns Percentage (0-100)
 */
export function calculateVolumePercentage(value: number, max: number): number {
  if (max === 0) return 0;
  return Math.round((value / max) * 100);
}

/**
 * Format volume number with appropriate suffix
 * @param value - Volume value
 * @returns Formatted string like "45 reps" or "135 cal"
 */
export function formatVolumeValue(value: number, unit?: string): string {
  if (unit) {
    return `${value} ${unit}`;
  }
  return `${value} reps`;
}
