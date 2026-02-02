import React, { useState, useMemo } from 'react';
import type { ParsedWorkoutPreviewProps } from '../../../types/workoutPreview';
import type { ParsingIssue } from '../../../types/parsingError';
import { WorkoutHeader } from './WorkoutHeader';
import { WorkoutMetadata } from './WorkoutMetadata';
import { RepSchemeDisplay } from './RepSchemeDisplay';
import { MovementList } from './MovementList';
import { VolumeSummary } from './VolumeSummary';
import { WarningsList } from './WarningsList';
import {
  calculateTimeDomain,
  calculateConfidence,
  detectRepScheme,
  getUnrecognizedCount,
} from '../../../utils/workoutFormatters';
import { calculateVolumes } from '../../../utils/volumeCalculations';

/**
 * Main container component for the enhanced parsed workout preview
 * Displays all workout information in a structured layout with visual elements
 */
export const ParsedWorkoutPreview: React.FC<ParsedWorkoutPreviewProps> = ({
  workout,
  onSave,
  onEdit,
  onAcceptSuggestion,
  showRawText = false,
  compact = false,
  className = '',
}) => {
  // State for raw text visibility
  const [isRawTextExpanded, setIsRawTextExpanded] = useState(false);

  // Calculate derived values
  const timeDomain = useMemo(
    () => calculateTimeDomain(workout.timeCapSeconds),
    [workout.timeCapSeconds]
  );

  const confidence = useMemo(
    () => calculateConfidence(workout),
    [workout]
  );

  const repScheme = useMemo(
    () => detectRepScheme(workout.movements, workout.roundCount),
    [workout.movements, workout.roundCount]
  );

  const volumes = useMemo(
    () => calculateVolumes(workout.movements, workout.roundCount),
    [workout.movements, workout.roundCount]
  );

  const unrecognizedCount = useMemo(
    () => getUnrecognizedCount(workout.movements),
    [workout.movements]
  );

  // Convert errors to ParsingIssue format for WarningsList
  // The existing errors array uses a different format, so we adapt it
  const warnings: ParsingIssue[] = useMemo(() => {
    return workout.errors.map(error => ({
      code: error.errorType,
      message: error.message,
      suggestion: null,
      line: error.lineNumber || null,
      severity: 'warning' as const, // Treat existing errors as warnings for display
    }));
  }, [workout.errors]);

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Header Section */}
      <WorkoutHeader
        workoutType={workout.workoutType}
        timeDomain={timeDomain}
        confidence={confidence}
        name={null}
        parsedDescription={workout.parsedDescription}
        errorCount={workout.errors.length}
        unrecognizedCount={unrecognizedCount}
        totalMovements={workout.movements.length}
      />

      {/* Metadata Section */}
      <WorkoutMetadata
        timeCapFormatted={workout.timeCapFormatted}
        roundCount={workout.roundCount}
        intervalDurationFormatted={workout.intervalDurationFormatted}
        compact={compact}
      />

      {/* Rep Scheme Display (if detected and not fixed single rep) */}
      {repScheme && repScheme.reps.length > 1 && (
        <RepSchemeDisplay scheme={repScheme} compact={compact} />
      )}

      {/* Main Content: Movements and Volume Summary */}
      <div className={`${compact ? '' : 'md:flex md:gap-6'}`}>
        {/* Movements List */}
        <div className={`${compact ? '' : 'md:flex-1'}`}>
          <MovementList
            movements={workout.movements}
            repScheme={repScheme}
            showErrors={true}
            showCategoryIcons={!compact}
          />
        </div>

        {/* Volume Summary (hidden in compact mode) */}
        {!compact && volumes.length > 0 && (
          <div className="mt-6 md:mt-0 md:w-72 md:flex-shrink-0">
            <VolumeSummary volumes={volumes} showBars={true} />
          </div>
        )}
      </div>

      {/* Warnings Section */}
      {warnings.length > 0 && (
        <WarningsList
          warnings={warnings}
          onApplySuggestion={onAcceptSuggestion}
        />
      )}

      {/* Raw Text Section (collapsible) */}
      {showRawText && (
        <div className="border border-gray-200 rounded-lg overflow-hidden">
          <button
            type="button"
            onClick={() => setIsRawTextExpanded(!isRawTextExpanded)}
            className="w-full flex items-center justify-between px-4 py-3 bg-gray-50 hover:bg-gray-100 transition-colors text-sm font-medium text-gray-700 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
            aria-expanded={isRawTextExpanded}
          >
            <span>Raw Input Text</span>
            <svg
              className={`w-5 h-5 transform transition-transform ${isRawTextExpanded ? 'rotate-180' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          {isRawTextExpanded && (
            <div className="p-4 bg-gray-50 border-t border-gray-200">
              <pre className="text-sm text-gray-600 whitespace-pre-wrap font-mono">
                {workout.originalText}
              </pre>
            </div>
          )}
        </div>
      )}

      {/* Action Buttons */}
      {(onSave || onEdit) && (
        <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
          {onEdit && (
            <button
              type="button"
              onClick={onEdit}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Edit
            </button>
          )}
          {onSave && (
            <button
              type="button"
              onClick={onSave}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Save Workout
            </button>
          )}
        </div>
      )}
    </div>
  );
};
