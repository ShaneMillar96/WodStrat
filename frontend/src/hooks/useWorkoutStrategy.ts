import { useMemo } from 'react';
import { usePacing } from './usePacing';
import { useWorkoutVolumeLoad } from './useVolumeLoad';
import { useTimeEstimate } from './useTimeEstimate';
import { useStrategyInsights } from './useStrategyInsights';
import type { WorkoutPacingResult } from '../types/pacing';
import type { WorkoutVolumeLoadResult } from '../types/volumeLoad';
import type { TimeEstimateResult } from '../types/timeEstimate';
import type { StrategyInsightsResult } from '../types/strategyInsights';
import type { ApiException } from '../services';

export interface WorkoutStrategyData {
  pacing: WorkoutPacingResult | undefined;
  volumeLoad: WorkoutVolumeLoadResult | undefined;
  timeEstimate: TimeEstimateResult | undefined;
  insights: StrategyInsightsResult | undefined;
}

export interface WorkoutStrategyLoadingState {
  isPacingLoading: boolean;
  isVolumeLoadLoading: boolean;
  isTimeEstimateLoading: boolean;
  isInsightsLoading: boolean;
  isAnyLoading: boolean;
  isAllLoaded: boolean;
}

export interface WorkoutStrategyErrors {
  pacingError: ApiException | null;
  volumeLoadError: ApiException | null;
  timeEstimateError: ApiException | null;
  insightsError: ApiException | null;
  hasAnyError: boolean;
}

export interface WorkoutStrategyResult {
  data: WorkoutStrategyData;
  loading: WorkoutStrategyLoadingState;
  errors: WorkoutStrategyErrors;
  refetchAll: () => void;
}

/**
 * Composite hook that fetches all strategy-related data for a workout
 * Combines pacing, volume load, time estimate, and strategy insights
 *
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param options - Optional configuration
 */
export function useWorkoutStrategy(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  options: {
    enabled?: boolean;
    hasWeightedMovements?: boolean;
  } = {}
): WorkoutStrategyResult {
  const { enabled = true, hasWeightedMovements = true } = options;

  // Fetch all four data sources in parallel
  const {
    pacing,
    isLoading: isPacingLoading,
    error: pacingError,
    refetch: refetchPacing,
  } = usePacing(athleteId, workoutId, { enabled });

  const {
    volumeLoad,
    isLoading: isVolumeLoadLoading,
    error: volumeLoadError,
    refetch: refetchVolumeLoad,
  } = useWorkoutVolumeLoad(athleteId, workoutId, {
    enabled: enabled && hasWeightedMovements
  });

  const {
    timeEstimate,
    isLoading: isTimeEstimateLoading,
    error: timeEstimateError,
    refetch: refetchTimeEstimate,
  } = useTimeEstimate(athleteId, workoutId, { enabled });

  const {
    insights,
    isLoading: isInsightsLoading,
    error: insightsError,
    refetch: refetchInsights,
  } = useStrategyInsights(athleteId, workoutId, { enabled });

  // Memoize the data object
  const data = useMemo<WorkoutStrategyData>(() => ({
    pacing,
    volumeLoad,
    timeEstimate,
    insights,
  }), [pacing, volumeLoad, timeEstimate, insights]);

  // Memoize loading states
  const loading = useMemo<WorkoutStrategyLoadingState>(() => ({
    isPacingLoading,
    isVolumeLoadLoading,
    isTimeEstimateLoading,
    isInsightsLoading,
    isAnyLoading: isPacingLoading || isVolumeLoadLoading || isTimeEstimateLoading || isInsightsLoading,
    isAllLoaded: !isPacingLoading && !isVolumeLoadLoading && !isTimeEstimateLoading && !isInsightsLoading,
  }), [isPacingLoading, isVolumeLoadLoading, isTimeEstimateLoading, isInsightsLoading]);

  // Memoize errors
  const errors = useMemo<WorkoutStrategyErrors>(() => ({
    pacingError,
    volumeLoadError,
    timeEstimateError,
    insightsError,
    hasAnyError: !!(pacingError || volumeLoadError || timeEstimateError || insightsError),
  }), [pacingError, volumeLoadError, timeEstimateError, insightsError]);

  // Refetch all data sources
  const refetchAll = () => {
    refetchPacing();
    refetchVolumeLoad();
    refetchTimeEstimate();
    refetchInsights();
  };

  return {
    data,
    loading,
    errors,
    refetchAll,
  };
}
