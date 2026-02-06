/**
 * Query key factory for consistent key management
 * Following TanStack Query best practices for key organization
 */
export const queryKeys = {
  /**
   * All athlete-related queries
   */
  athletes: {
    all: ['athletes'] as const,
    me: () => [...queryKeys.athletes.all, 'me'] as const,
    lists: () => [...queryKeys.athletes.all, 'list'] as const,
    list: (filters: Record<string, unknown>) =>
      [...queryKeys.athletes.lists(), filters] as const,
    details: () => [...queryKeys.athletes.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.athletes.details(), id] as const,
  },

  /**
   * All benchmark-related queries
   */
  benchmarks: {
    all: ['benchmarks'] as const,
    definitions: () => [...queryKeys.benchmarks.all, 'definitions'] as const,
    myBenchmarks: () => [...queryKeys.benchmarks.all, 'my'] as const,
    mySummary: () => [...queryKeys.benchmarks.all, 'my', 'summary'] as const,
    // Legacy keys (for backward compatibility during migration)
    athleteBenchmarks: (athleteId: number) =>
      [...queryKeys.benchmarks.all, 'athlete', athleteId] as const,
    summary: (athleteId: number) =>
      [...queryKeys.benchmarks.all, 'summary', athleteId] as const,
  },

  /**
   * All workout-related queries
   */
  workouts: {
    all: ['workouts'] as const,
    lists: () => [...queryKeys.workouts.all, 'list'] as const,
    list: (filters?: Record<string, unknown>) =>
      [...queryKeys.workouts.lists(), filters] as const,
    details: () => [...queryKeys.workouts.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.workouts.details(), id] as const,
    parse: () => [...queryKeys.workouts.all, 'parse'] as const,
  },

  /**
   * All movement-related queries
   */
  movements: {
    all: ['movements'] as const,
    lists: () => [...queryKeys.movements.all, 'list'] as const,
    list: (category?: string) =>
      [...queryKeys.movements.lists(), { category }] as const,
    details: () => [...queryKeys.movements.all, 'detail'] as const,
    detail: (canonicalName: string) =>
      [...queryKeys.movements.details(), canonicalName] as const,
    // Lookup by alias (returns single result or 404)
    lookup: (alias: string) =>
      [...queryKeys.movements.all, 'lookup', alias] as const,
    // Search query (returns result or null)
    search: (query: string) =>
      [...queryKeys.movements.all, 'search', query] as const,
  },

  /**
   * All volume load-related queries
   */
  volumeLoad: {
    all: ['volumeLoad'] as const,
    workout: (athleteId: number, workoutId: number) =>
      [...queryKeys.volumeLoad.all, 'workout', athleteId, workoutId] as const,
  },

  /**
   * All time estimate-related queries
   */
  timeEstimate: {
    all: ['timeEstimate'] as const,
    workout: (athleteId: number, workoutId: number) =>
      [...queryKeys.timeEstimate.all, 'workout', athleteId, workoutId] as const,
    emomFeasibility: (athleteId: number, workoutId: number) =>
      [...queryKeys.timeEstimate.all, 'emom', athleteId, workoutId] as const,
  },

  /**
   * All pacing-related queries
   */
  pacing: {
    all: ['pacing'] as const,
    workout: (athleteId: number, workoutId: number) =>
      [...queryKeys.pacing.all, 'workout', athleteId, workoutId] as const,
  },

  /**
   * All strategy insights-related queries
   */
  strategyInsights: {
    all: ['strategyInsights'] as const,
    workout: (athleteId: number, workoutId: number) =>
      [...queryKeys.strategyInsights.all, 'workout', athleteId, workoutId] as const,
  },
};
