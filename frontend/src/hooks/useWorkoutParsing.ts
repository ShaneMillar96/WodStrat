import { useMutation } from '@tanstack/react-query';
import { workoutService } from '../services/workoutService';
import type { ParsedWorkout } from '../types/workout';

/**
 * Hook for parsing workout text
 * Returns a mutation for parsing workout text into structured data
 */
export function useWorkoutParsing() {
  const mutation = useMutation({
    mutationFn: (text: string) => workoutService.parse(text),
  });

  return {
    // Mutation function
    parseWorkout: mutation.mutateAsync,

    // Parsed result
    parsedWorkout: mutation.data as ParsedWorkout | undefined,

    // Loading state
    isParsing: mutation.isPending,

    // Success state
    parseSuccess: mutation.isSuccess,

    // Error
    parseError: mutation.error,

    // Reset
    resetParsing: mutation.reset,
  };
}
