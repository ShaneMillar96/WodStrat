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
};
