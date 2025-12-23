import React, { useState, useRef, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Textarea, FormField, Alert } from '../ui';
import { workoutInputSchema, type WorkoutInputSchemaType } from '../../schemas/workoutSchema';
import { ParsingErrorList } from './ParsingErrorList';
import { ParsingWarningBanner } from './ParsingWarningBanner';
import type { ParsingIssue } from '../../types/parsingError';

export interface WorkoutInputFormProps {
  onSubmit: (data: WorkoutInputSchemaType) => void;
  isSubmitting?: boolean;
  error?: string | null;
  /** Parsing errors to display */
  parsingErrors?: ParsingIssue[];
  /** Parsing warnings to display */
  parsingWarnings?: ParsingIssue[];
  /** Callback when a line number is clicked */
  onLineClick?: (lineNumber: number) => void;
  /** Callback when "Apply fix" is clicked for a suggestion */
  onApplySuggestion?: (issue: ParsingIssue, lineNumber: number | null) => void;
  /** Callback when warnings are dismissed */
  onDismissWarnings?: () => void;
  /** Parse confidence score (0-1) */
  parseConfidence?: number | null;
}

/**
 * Example workout text for placeholder
 */
const EXAMPLE_WORKOUT = `20 min AMRAP
10 Pull-ups
15 Push-ups
20 Air Squats`;

/**
 * Parse confidence indicator component
 */
interface ParseConfidenceIndicatorProps {
  confidence: number;
}

const ParseConfidenceIndicator: React.FC<ParseConfidenceIndicatorProps> = ({ confidence }) => {
  // Determine color based on confidence level
  let colorClass = 'bg-red-500';
  let textColorClass = 'text-red-700';
  if (confidence > 0.8) {
    colorClass = 'bg-green-500';
    textColorClass = 'text-green-700';
  } else if (confidence > 0.5) {
    colorClass = 'bg-yellow-500';
    textColorClass = 'text-yellow-700';
  }

  const percentage = Math.round(confidence * 100);

  return (
    <div className="flex items-center gap-2">
      <span className="text-sm text-gray-500">Parse confidence:</span>
      <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden max-w-[200px]">
        <div
          className={`h-full transition-all duration-300 ${colorClass}`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      <span className={`text-sm font-medium ${textColorClass}`}>{percentage}%</span>
    </div>
  );
};

/**
 * Form component for inputting workout text
 */
export const WorkoutInputForm: React.FC<WorkoutInputFormProps> = ({
  onSubmit,
  isSubmitting = false,
  error = null,
  parsingErrors = [],
  parsingWarnings = [],
  onLineClick,
  onApplySuggestion,
  onDismissWarnings,
  parseConfidence = null,
}) => {
  // Track focused line for potential future visual feedback
  const [, setFocusedLine] = useState<number | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    getValues,
  } = useForm<WorkoutInputSchemaType>({
    resolver: zodResolver(workoutInputSchema),
    defaultValues: {
      text: '',
    },
  });

  // Get the register result and merge with our ref
  const { ref: registerRef, ...registerRest } = register('text');

  // Merge refs
  const setTextareaRef = useCallback(
    (element: HTMLTextAreaElement | null) => {
      textareaRef.current = element;
      registerRef(element);
    },
    [registerRef]
  );

  /**
   * Scroll textarea to specific line
   */
  const scrollToLine = useCallback(
    (lineNumber: number) => {
      if (!textareaRef.current) return;

      const text = textareaRef.current.value;
      const lines = text.split('\n');

      // Calculate character index at the start of the target line
      let charIndex = 0;
      for (let i = 0; i < lineNumber - 1 && i < lines.length; i++) {
        charIndex += lines[i].length + 1; // +1 for newline character
      }

      // Focus the textarea
      textareaRef.current.focus();

      // Set cursor to the beginning of the line
      textareaRef.current.setSelectionRange(charIndex, charIndex);

      // Update focused line state
      setFocusedLine(lineNumber);

      // Clear focus highlight after a delay
      setTimeout(() => setFocusedLine(null), 2000);

      // Call external handler if provided
      if (onLineClick) {
        onLineClick(lineNumber);
      }
    },
    [onLineClick]
  );

  /**
   * Apply suggestion (replace text at line)
   */
  const applySuggestion = useCallback(
    (issue: ParsingIssue, lineNumber: number | null) => {
      if (!textareaRef.current || !issue.suggestion) return;

      const text = getValues('text');
      const lines = text.split('\n');

      if (lineNumber !== null && lineNumber > 0 && lineNumber <= lines.length) {
        // Replace the entire line with the suggestion
        // Note: This is a simple implementation - the actual replacement
        // logic may need to be more sophisticated based on API format
        lines[lineNumber - 1] = issue.suggestion;
        const newText = lines.join('\n');
        setValue('text', newText, { shouldValidate: true });

        // Focus on the updated line
        scrollToLine(lineNumber);
      }

      // Call external handler if provided
      if (onApplySuggestion) {
        onApplySuggestion(issue, lineNumber);
      }
    },
    [getValues, setValue, scrollToLine, onApplySuggestion]
  );

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <FormField
        label="Workout Text"
        htmlFor="workout-text"
        error={errors.text?.message}
        required
      >
        <Textarea
          {...registerRest}
          ref={setTextareaRef}
          placeholder={EXAMPLE_WORKOUT}
          rows={8}
          error={!!errors.text}
          disabled={isSubmitting}
          className="font-mono"
        />
      </FormField>

      <div className="text-sm text-gray-500">
        <p>Paste your workout description. Supported formats:</p>
        <ul className="list-disc list-inside mt-1 space-y-0.5">
          <li>AMRAP (e.g., "20 min AMRAP")</li>
          <li>For Time (e.g., "For Time:" or "3 Rounds For Time")</li>
          <li>EMOM (e.g., "10 min EMOM" or "EMOM 20")</li>
          <li>Intervals (e.g., "5 rounds of 3 min work / 1 min rest")</li>
        </ul>
      </div>

      {/* Parse confidence indicator */}
      {parseConfidence !== null && parseConfidence !== undefined && (
        <ParseConfidenceIndicator confidence={parseConfidence} />
      )}

      {/* Parsing errors */}
      {parsingErrors && parsingErrors.length > 0 && (
        <ParsingErrorList
          errors={parsingErrors}
          onLineClick={scrollToLine}
          onApplySuggestion={applySuggestion}
        />
      )}

      {/* Parsing warnings */}
      {parsingWarnings && parsingWarnings.length > 0 && (
        <ParsingWarningBanner
          warnings={parsingWarnings}
          onDismiss={onDismissWarnings}
          onDismissWarning={() => {
            // Individual warning dismissal handled internally by the banner
          }}
          canAutoFix={parsingWarnings.some((w) => w.suggestion)}
        />
      )}

      {/* Existing error alert */}
      {error && <Alert variant="error">{error}</Alert>}

      <div className="flex justify-end">
        <Button
          type="submit"
          variant="primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Parsing...' : 'Parse Workout'}
        </Button>
      </div>
    </form>
  );
};
