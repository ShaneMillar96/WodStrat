import React from 'react';
import { StrategyHeader } from './StrategyHeader';
import { MetricsRow } from './MetricsRow';
import { KeyFocusSection } from './KeyFocusSection';
import { AlertsSection } from './AlertsSection';
import { MovementBreakdownTable } from './MovementBreakdownTable';
import { Alert, Button } from '../ui';
import type { WorkoutStrategyResult } from '../../hooks/useWorkoutStrategy';
import type { Workout } from '../../types/workout';

export interface WorkoutStrategyPageProps {
  /** Workout data */
  workout: Workout;
  /** Strategy data from useWorkoutStrategy hook */
  strategy: WorkoutStrategyResult;
  /** Callback to go back */
  onBack?: () => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * WorkoutStrategyPage - Full workout strategy display
 * Combines all strategy sections into a complete page
 */
export const WorkoutStrategyPage: React.FC<WorkoutStrategyPageProps> = ({
  workout,
  strategy,
  onBack,
  className = '',
}) => {
  const { data, loading, errors } = strategy;
  const workoutName = workout.name || workout.parsedDescription || 'Unnamed Workout';

  // Check if we have any data at all
  const hasAnyData = data.insights || data.timeEstimate || data.volumeLoad || data.pacing;
  const allDataMissing = !loading.isAnyLoading && !hasAnyData;

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Back button */}
      {onBack && (
        <div>
          <Button variant="outline" size="sm" onClick={onBack}>
            &larr; Back to Workout
          </Button>
        </div>
      )}

      {/* No data warning */}
      {allDataMissing && (
        <Alert variant="info" title="Strategy Data Unavailable">
          Create an athlete profile and add benchmark data to see personalized workout strategies.
        </Alert>
      )}

      {/* Header with difficulty and confidence */}
      <StrategyHeader
        workoutName={workoutName}
        insights={data.insights}
        description={workout.originalText}
        isLoading={loading.isInsightsLoading}
      />

      {/* Time and Volume metrics row */}
      <MetricsRow
        timeEstimate={data.timeEstimate}
        volumeLoad={data.volumeLoad}
        isTimeEstimateLoading={loading.isTimeEstimateLoading}
        isVolumeLoadLoading={loading.isVolumeLoadLoading}
        timeEstimateError={errors.timeEstimateError}
        volumeLoadError={errors.volumeLoadError}
      />

      {/* Key Focus Section */}
      <KeyFocusSection
        movements={data.insights?.keyFocusMovements}
        isLoading={loading.isInsightsLoading}
      />

      {/* Alerts Section */}
      <AlertsSection
        alerts={data.insights?.riskAlerts}
        isLoading={loading.isInsightsLoading}
      />

      {/* Movement Breakdown Table */}
      <MovementBreakdownTable
        pacingData={data.pacing?.movementPacing}
        volumeData={data.volumeLoad?.movementVolumes}
        isLoading={loading.isPacingLoading || loading.isVolumeLoadLoading}
      />

      {/* Overall strategy notes */}
      {data.pacing?.overallStrategyNotes && (
        <div className="rounded-lg border border-gray-200 bg-white p-5">
          <h3 className="font-semibold text-gray-900 mb-2">Strategy Notes</h3>
          <p className="text-gray-700">{data.pacing.overallStrategyNotes}</p>
        </div>
      )}

      {/* Calculation timestamps */}
      {(data.insights?.calculatedAt || data.timeEstimate?.calculatedAt) && (
        <div className="text-xs text-gray-400 text-right">
          Last calculated: {new Date(data.insights?.calculatedAt || data.timeEstimate?.calculatedAt || '').toLocaleString()}
        </div>
      )}
    </div>
  );
};
