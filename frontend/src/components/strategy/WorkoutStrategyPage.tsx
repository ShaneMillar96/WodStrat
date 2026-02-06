import React from 'react';
import { StrategyHeader } from './StrategyHeader';
import { DifficultyBreakdownChart } from './DifficultyBreakdownChart';
import { QuickMetricsRow } from './QuickMetricsRow';
import { StrategyTabContainer } from './StrategyTabContainer';
import { MissingBenchmarksCTA } from './MissingBenchmarksCTA';
import { OverviewTab, PacingTab, VolumeTab, TimingTab } from './tabs';
import { Alert, Button } from '../ui';
import type { WorkoutStrategyResult } from '../../hooks/useWorkoutStrategy';
import type { Workout } from '../../types/workout';
import type { StrategyTabId } from '../../types/strategyPage';
import type { EmomFeasibility } from '../../types/timeEstimate';

export interface WorkoutStrategyPageProps {
  /** Workout data */
  workout: Workout;
  /** Strategy data from useWorkoutStrategy hook */
  strategy: WorkoutStrategyResult;
  /** EMOM feasibility data (optional, for EMOM workouts) */
  emomFeasibility?: EmomFeasibility[];
  /** Callback to go back */
  onBack?: () => void;
  /** Callback to navigate to add benchmarks page */
  onAddBenchmarks?: () => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * WorkoutStrategyPage - Full workout strategy display with tabbed layout
 *
 * Features:
 * - StrategyHeader with workout name, difficulty, and confidence
 * - DifficultyBreakdownChart showing factor contributions
 * - QuickMetricsRow with key metrics at a glance
 * - Tabbed navigation (Overview, Pacing, Volume, Timing)
 * - MissingBenchmarksCTA when benchmarks are missing
 * - Conditional rendering based on workout type
 */
export const WorkoutStrategyPage: React.FC<WorkoutStrategyPageProps> = ({
  workout,
  strategy,
  emomFeasibility,
  onBack,
  onAddBenchmarks,
  className = '',
}) => {
  const { data, loading } = strategy;
  const workoutName = workout.name || workout.parsedDescription || 'Unnamed Workout';

  // Check if we have any data at all
  const hasAnyData = data.insights || data.timeEstimate || data.volumeLoad || data.pacing;
  const allDataMissing = !loading.isAnyLoading && !hasAnyData;

  // Extract additional time estimate fields that may be available
  const timeEstimateExtended = data.timeEstimate as typeof data.timeEstimate & {
    benchmarkCoverageCount?: number;
    totalMovementCount?: number;
    averagePercentile?: number | null;
    emomFeasibility?: EmomFeasibility[];
  };

  // Get EMOM feasibility from props or from time estimate
  const emomData = emomFeasibility || timeEstimateExtended?.emomFeasibility;

  // Render tab content based on active tab
  const renderTabContent = (activeTab: StrategyTabId) => {
    switch (activeTab) {
      case 'overview':
        return (
          <OverviewTab
            insights={data.insights}
            pacing={data.pacing}
            isLoading={loading.isInsightsLoading || loading.isPacingLoading}
          />
        );
      case 'pacing':
        return (
          <PacingTab
            pacing={data.pacing}
            isLoading={loading.isPacingLoading}
          />
        );
      case 'volume':
        return (
          <VolumeTab
            volumeLoad={data.volumeLoad}
            isLoading={loading.isVolumeLoadLoading}
          />
        );
      case 'timing':
        return (
          <TimingTab
            timeEstimate={data.timeEstimate}
            emomFeasibility={emomData}
            workoutType={workout.workoutType}
            benchmarkCoverageCount={timeEstimateExtended?.benchmarkCoverageCount}
            totalMovementCount={timeEstimateExtended?.totalMovementCount}
            averagePercentile={timeEstimateExtended?.averagePercentile}
            isLoading={loading.isTimeEstimateLoading}
          />
        );
      default:
        return null;
    }
  };

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

      {/* Difficulty Breakdown Chart */}
      {data.insights?.difficultyScore?.breakdown && (
        <DifficultyBreakdownChart
          breakdown={data.insights.difficultyScore.breakdown}
          totalScore={data.insights.difficultyScore.score}
        />
      )}

      {/* Quick Metrics Row */}
      <QuickMetricsRow
        timeEstimate={data.timeEstimate}
        volumeLoad={data.volumeLoad}
        benchmarkCoverageCount={timeEstimateExtended?.benchmarkCoverageCount}
        totalMovementCount={timeEstimateExtended?.totalMovementCount}
        averagePercentile={timeEstimateExtended?.averagePercentile}
        isLoading={loading.isTimeEstimateLoading || loading.isVolumeLoadLoading}
      />

      {/* Tabbed Content Area */}
      <StrategyTabContainer defaultTab="overview">
        {renderTabContent}
      </StrategyTabContainer>

      {/* Missing Benchmarks CTA */}
      {data.insights?.strategyConfidence?.missingBenchmarks &&
        data.insights.strategyConfidence.missingBenchmarks.length > 0 && (
          <MissingBenchmarksCTA
            missingBenchmarks={data.insights.strategyConfidence.missingBenchmarks}
            confidenceLevel={data.insights.strategyConfidence.level}
            onAddBenchmarks={onAddBenchmarks}
          />
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
