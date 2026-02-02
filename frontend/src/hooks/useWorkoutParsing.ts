import { useMutation } from '@tanstack/react-query';
import { useState, useCallback, useMemo } from 'react';
import { workoutService } from '../services/workoutService';
import type { ParseWorkoutResponse, ParsingIssue, ParsedWorkout } from '../types';

/**
 * Hook for parsing workout text with enhanced error handling
 * Returns mutation state plus error/warning management
 */
export function useWorkoutParsing() {
  const [dismissedWarnings, setDismissedWarnings] = useState(false);

  const mutation = useMutation({
    mutationFn: (text: string) => workoutService.parseWithErrors(text),
  });

  const response = mutation.data as ParseWorkoutResponse | undefined;

  // Transform parsedWorkout to include errors and isValid from root-level response
  // This fixes the type mismatch between API response and frontend expectations
  const transformedParsedWorkout: ParsedWorkout | null = useMemo(() => {
    if (!response?.parsedWorkout) {
      return null;
    }

    // Convert ParsingIssue[] (API format) to ParsingError[] (ParsedWorkout format)
    const parsingErrors = (response.errors ?? []).map(issue => ({
      errorType: issue.code,
      message: issue.message,
      lineNumber: issue.line ?? 0,
      originalText: null,
    }));

    return {
      ...response.parsedWorkout,
      errors: parsingErrors,
      isValid: response.success && parsingErrors.length === 0 && (response.parsedWorkout.movements?.length ?? 0) > 0,
    };
  }, [response]);

  // Reset dismissed state when new parse occurs
  const parseWorkout = useCallback(async (text: string) => {
    setDismissedWarnings(false);
    return mutation.mutateAsync(text);
  }, [mutation]);

  // Dismiss all warnings
  const dismissWarnings = useCallback(() => {
    setDismissedWarnings(true);
  }, []);

  // Get visible warnings (not dismissed)
  const visibleWarnings: ParsingIssue[] = dismissedWarnings ? [] : (response?.warnings ?? []);

  // Check if parsing was successful (no blocking errors)
  const isParseSuccessful = response?.success ?? false;

  // Check if there are blocking errors
  const hasBlockingErrors = (response?.errors?.length ?? 0) > 0;

  return {
    // Mutation function
    parseWorkout,

    // Response data
    parseResponse: response,
    parsedWorkout: transformedParsedWorkout,
    parseConfidence: response?.parseConfidence ?? null,

    // Errors and warnings
    parsingErrors: response?.errors ?? [],
    parsingWarnings: visibleWarnings,
    hasBlockingErrors,
    isParseSuccessful,

    // Warning management
    dismissWarnings,
    warningsDismissed: dismissedWarnings,

    // Loading state
    isParsing: mutation.isPending,

    // Success state (mutation completed, may still have errors)
    parseComplete: mutation.isSuccess,

    // Network/API error
    parseError: mutation.error,

    // Reset
    resetParsing: () => {
      setDismissedWarnings(false);
      mutation.reset();
    },
  };
}
