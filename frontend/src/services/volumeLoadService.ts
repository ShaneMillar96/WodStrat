import { api } from './api';
import type {
  WorkoutVolumeLoadResult,
  CalculateVolumeLoadRequest,
} from '../types/volumeLoad';

/**
 * Volume Load API service
 * Provides methods for fetching and calculating volume load data
 */
export const volumeLoadService = {
  /**
   * Get volume load analysis for a specific workout
   * @param athleteId - The athlete's unique identifier
   * @param workoutId - The workout's unique identifier
   * @returns Volume load analysis for the workout
   */
  getWorkoutVolumeLoad: async (
    athleteId: number,
    workoutId: number
  ): Promise<WorkoutVolumeLoadResult> => {
    return api.get<WorkoutVolumeLoadResult>(
      `/athletes/${athleteId}/workouts/${workoutId}/volume-load`
    );
  },

  /**
   * Calculate volume load with request body
   * Alternative endpoint for flexibility
   * @param request - The volume load calculation request
   * @returns Volume load analysis for the workout
   */
  calculateVolumeLoad: async (
    request: CalculateVolumeLoadRequest
  ): Promise<WorkoutVolumeLoadResult> => {
    return api.post<WorkoutVolumeLoadResult, CalculateVolumeLoadRequest>(
      '/volume-load/calculate',
      request
    );
  },
};
