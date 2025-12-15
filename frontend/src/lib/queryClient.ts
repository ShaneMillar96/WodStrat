import { QueryClient } from '@tanstack/react-query';

/**
 * Create and configure the TanStack Query client
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Data is considered fresh for 5 minutes
      staleTime: 5 * 60 * 1000,
      // Cache data for 10 minutes
      gcTime: 10 * 60 * 1000,
      // Retry failed requests once
      retry: 1,
      // Don't refetch on window focus (can be enabled later)
      refetchOnWindowFocus: false,
    },
    mutations: {
      // Don't retry mutations by default
      retry: false,
    },
  },
});
