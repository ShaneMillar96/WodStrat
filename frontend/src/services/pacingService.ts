import { api } from './api';
import type { WorkoutPacingResult } from '../types/pacing';

/**
 * Pacing API service
 * Provides methods for fetching workout pacing data
 */
export const pacingService = {
  /**
   * Get pacing analysis for a specific workout
   * @param athleteId - The athlete's unique identifier
   * @param workoutId - The workout's unique identifier
   * @returns Pacing analysis for the workout
   */
  getWorkoutPacing: async (
    athleteId: number,
    workoutId: number
  ): Promise<WorkoutPacingResult> => {
    return api.get<WorkoutPacingResult>(
      `/athletes/${athleteId}/workouts/${workoutId}/pacing`
    );
  },
};
