import { api } from './api';
import type {
  TimeEstimateResult,
  EmomFeasibility,
  CalculateTimeEstimateRequest,
} from '../types/timeEstimate';

/**
 * Time Estimate API service
 * Provides methods for fetching and calculating time estimates
 */
export const timeEstimateService = {
  /**
   * Get time estimate for a specific workout
   * @param athleteId - The athlete's unique identifier
   * @param workoutId - The workout's unique identifier
   * @returns Time estimate result for the workout
   */
  getTimeEstimate: async (
    athleteId: number,
    workoutId: number
  ): Promise<TimeEstimateResult> => {
    return api.get<TimeEstimateResult>(
      `/athletes/${athleteId}/workouts/${workoutId}/time-estimate`
    );
  },

  /**
   * Calculate time estimate with request body
   * Alternative endpoint for flexibility
   * @param request - The time estimate calculation request
   * @returns Time estimate result for the workout
   */
  calculateTimeEstimate: async (
    request: CalculateTimeEstimateRequest
  ): Promise<TimeEstimateResult> => {
    return api.post<TimeEstimateResult, CalculateTimeEstimateRequest>(
      '/time-estimate/calculate',
      request
    );
  },

  /**
   * Get EMOM feasibility analysis for a workout
   * @param athleteId - The athlete's unique identifier
   * @param workoutId - The workout's unique identifier
   * @returns Array of per-minute feasibility data
   */
  getEmomFeasibility: async (
    athleteId: number,
    workoutId: number
  ): Promise<EmomFeasibility[]> => {
    return api.get<EmomFeasibility[]>(
      `/athletes/${athleteId}/workouts/${workoutId}/emom-feasibility`
    );
  },
};
