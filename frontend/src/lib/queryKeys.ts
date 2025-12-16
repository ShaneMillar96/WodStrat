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
};
