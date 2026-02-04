import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Alert, Button, ConfirmDialog, SkeletonCard } from '../components/ui';
import {
  MovementList,
  WorkoutTypeTag,
  WorkoutMetadata,
  VolumeLoadSummary,
  TimeEstimateSummary,
} from '../components/workouts';
import { useWorkout } from '../hooks/useWorkout';
import { useWorkoutMutations } from '../hooks/useWorkouts';
import { useWorkoutVolumeLoad } from '../hooks/useVolumeLoad';
import { useTimeEstimate, useEmomFeasibility } from '../hooks/useTimeEstimate';
import { useAthleteContext } from '../contexts/AthleteContext';
import { ApiException } from '../services';

/**
 * WorkoutDetailPage - Display a single workout's full details
 *
 * Features:
 * - Shows workout type badge, metadata, original text, and movements
 * - Edit and Delete actions
 * - Delete confirmation dialog
 * - Loading and error states
 */
export const WorkoutDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const workoutId = id ? parseInt(id, 10) : null;

  // State
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Hooks
  const { workout, isLoading, error: queryError } = useWorkout(workoutId);
  const { deleteWorkout, isDeleting, deleteSuccess, error: mutationError } = useWorkoutMutations();
  const { athleteId } = useAthleteContext();

  // Check if workout has weighted movements
  const hasWeightedMovements = workout?.movements.some(m => m.loadValue !== null) ?? false;

  // Volume load hook - only enabled when we have an athlete and weighted movements
  const {
    volumeLoad,
    isLoading: isLoadingVolumeLoad,
    error: volumeLoadError,
    hasVolumeData,
  } = useWorkoutVolumeLoad(athleteId, workoutId, {
    enabled: hasWeightedMovements && !!athleteId,
  });

  // Time estimate hook - enabled when we have an athlete
  const {
    timeEstimate,
    isLoading: isLoadingTimeEstimate,
    error: timeEstimateError,
    hasEstimateData,
  } = useTimeEstimate(athleteId, workoutId, {
    enabled: !!athleteId,
  });

  // EMOM feasibility hook - only for EMOM workouts
  const {
    feasibility: emomFeasibility,
    isLoading: isLoadingEmom,
  } = useEmomFeasibility(athleteId, workoutId, workout?.workoutType, {
    enabled: !!athleteId && workout?.workoutType === 'Emom',
  });

  // Handle delete success
  useEffect(() => {
    if (deleteSuccess) {
      setSuccessMessage('Workout deleted successfully!');
      setTimeout(() => navigate('/workouts'), 1500);
    }
  }, [deleteSuccess, navigate]);

  // Handle mutation errors
  useEffect(() => {
    if (mutationError) {
      const errorMsg = mutationError instanceof ApiException
        ? mutationError.getUserMessage()
        : (mutationError as Error).message || 'Failed to delete workout';
      setErrorMessage(errorMsg);
    }
  }, [mutationError]);

  // Handlers
  const handleDelete = () => {
    setDeleteDialogOpen(true);
    setErrorMessage(null);
  };

  const handleConfirmDelete = async () => {
    if (!workoutId) return;
    try {
      await deleteWorkout(workoutId);
    } catch (err) {
      console.error('Failed to delete workout:', err);
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 lg:px-8">
        <SkeletonCard />
      </div>
    );
  }

  // Error state
  if (queryError || !workout) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 lg:px-8">
        <Alert variant="error" title="Error">
          {queryError?.getUserMessage() || 'Workout not found'}
        </Alert>
        <Link to="/workouts">
          <Button variant="outline" className="mt-4">Back to Workouts</Button>
        </Link>
      </div>
    );
  }

  const displayName = workout.name || workout.parsedDescription || 'Unnamed Workout';

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">{displayName}</h1>
        <Link to="/workouts">
          <Button variant="outline" size="sm">Back to Workouts</Button>
        </Link>
      </div>

      {/* Messages */}
      {successMessage && (
        <div className="mb-6">
          <Alert variant="success">{successMessage}</Alert>
        </div>
      )}
      {errorMessage && (
        <div className="mb-6">
          <Alert variant="error" dismissible onDismiss={() => setErrorMessage(null)}>
            {errorMessage}
          </Alert>
        </div>
      )}

      {/* Workout Details Card */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        {/* Type and Metadata */}
        <div className="mb-6 flex items-center gap-4 flex-wrap">
          <WorkoutTypeTag type={workout.workoutType} size="lg" />
          <WorkoutMetadata workout={workout} />
        </div>

        {/* Original Text */}
        {workout.originalText && (
          <div className="mb-6">
            <h3 className="text-sm font-medium text-gray-500 mb-2">Original Text</h3>
            <pre className="rounded bg-gray-50 p-3 text-sm text-gray-700 whitespace-pre-wrap font-mono">
              {workout.originalText}
            </pre>
          </div>
        )}

        {/* Movements */}
        <div className="mb-6">
          <h3 className="text-sm font-medium text-gray-500 mb-2">
            Movements ({workout.movements.length})
          </h3>
          <MovementList movements={workout.movements} showErrors={false} />
        </div>

        {/* Volume Load Analysis */}
        {hasWeightedMovements && (
          <div className="mb-6">
            {volumeLoadError ? (
              <Alert variant="info" title="Volume Load Analysis">
                Volume load analysis requires benchmark data. Add your strength benchmarks to see personalized load analysis.
              </Alert>
            ) : hasVolumeData && volumeLoad ? (
              <VolumeLoadSummary
                volumeLoad={volumeLoad}
                isLoading={isLoadingVolumeLoad}
              />
            ) : isLoadingVolumeLoad ? (
              <VolumeLoadSummary
                volumeLoad={{
                  workoutId: workoutId!,
                  workoutName: '',
                  movementVolumes: [],
                  totalVolumeLoad: 0,
                  totalVolumeLoadFormatted: '',
                  overallAssessment: '',
                  calculatedAt: new Date().toISOString(),
                }}
                isLoading={true}
              />
            ) : !athleteId ? (
              <Alert variant="info" title="Volume Load Analysis">
                Create an athlete profile to see personalized volume load analysis for this workout.
              </Alert>
            ) : null}
          </div>
        )}

        {/* Time Estimate Analysis */}
        <div className="mb-6">
          {timeEstimateError ? (
            <Alert variant="info" title="Time Estimate">
              Time estimates require benchmark data. Add your benchmarks to see personalized time predictions.
            </Alert>
          ) : hasEstimateData && timeEstimate ? (
            <TimeEstimateSummary
              timeEstimate={timeEstimate}
              emomFeasibility={emomFeasibility}
              timeCapSeconds={workout.timeCapSeconds}
              isLoading={isLoadingTimeEstimate}
              isLoadingEmom={isLoadingEmom}
            />
          ) : isLoadingTimeEstimate ? (
            <TimeEstimateSummary
              timeEstimate={{
                workoutId: workoutId!,
                workoutName: '',
                workoutType: workout.workoutType,
                estimateType: 'Time',
                minEstimate: 0,
                maxEstimate: 0,
                formattedRange: '',
                confidenceLevel: 'Low',
                factorsSummary: '',
                restRecommendations: [],
                calculatedAt: new Date().toISOString(),
              }}
              isLoading={true}
            />
          ) : !athleteId ? (
            <Alert variant="info" title="Time Estimate">
              Create an athlete profile to see personalized time estimates for this workout.
            </Alert>
          ) : null}
        </div>

        {/* Timestamps */}
        <div className="text-xs text-gray-400 border-t border-gray-100 pt-4">
          <p>Created: {new Date(workout.createdAt).toLocaleString()}</p>
          <p>Updated: {new Date(workout.updatedAt).toLocaleString()}</p>
        </div>

        {/* Actions */}
        <div className="mt-6 flex justify-end gap-3 border-t border-gray-200 pt-4">
          <Link to={`/workouts/${workout.id}/edit`}>
            <Button variant="outline">Edit</Button>
          </Link>
          <Button variant="outline" onClick={handleDelete}>Delete</Button>
        </div>
      </div>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={deleteDialogOpen}
        onClose={() => !isDeleting && setDeleteDialogOpen(false)}
        onConfirm={handleConfirmDelete}
        title="Delete Workout"
        message={`Are you sure you want to delete "${displayName}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};
