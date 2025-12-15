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
    lists: () => [...queryKeys.athletes.all, 'list'] as const,
    list: (filters: Record<string, unknown>) =>
      [...queryKeys.athletes.lists(), filters] as const,
    details: () => [...queryKeys.athletes.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.athletes.details(), id] as const,
  },
};
