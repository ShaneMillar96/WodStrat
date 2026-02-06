import { api } from './api';
import type { StrategyInsightsResult } from '../types/strategyInsights';

/**
 * Strategy Insights API service
 * Provides methods for fetching comprehensive strategy insights
 */
export const strategyInsightsService = {
  /**
   * Get strategy insights for a specific workout
   * @param athleteId - The athlete's unique identifier
   * @param workoutId - The workout's unique identifier
   * @returns Strategy insights for the workout
   */
  getStrategyInsights: async (
    athleteId: number,
    workoutId: number
  ): Promise<StrategyInsightsResult> => {
    return api.get<StrategyInsightsResult>(
      `/athletes/${athleteId}/workouts/${workoutId}/strategy-insights`
    );
  },
};
