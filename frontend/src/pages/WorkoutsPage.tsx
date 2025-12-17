import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Alert, Button, ConfirmDialog } from '../components/ui';
import { WorkoutsList } from '../components/workouts';
import { useWorkouts } from '../hooks/useWorkouts';
import { ApiException } from '../services';
import type { Workout } from '../types/workout';

/**
 * Loading skeleton for the page header
 */
const HeaderSkeleton: React.FC = () => (
  <div className="animate-pulse">
    <div className="h-8 w-48 rounded bg-gray-200" />
    <div className="mt-2 h-4 w-64 rounded bg-gray-200" />
  </div>
);

/**
 * Workouts list page
 * Displays all user workouts with CRUD operations
 */
export const WorkoutsPage: React.FC = () => {
  const navigate = useNavigate();

  // State
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [workoutToDelete, setWorkoutToDelete] = useState<Workout | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Hooks
  const {
    workouts,
    isLoading,
    queryError,
    refetch,
    deleteWorkout,
    isDeleting,
    deleteSuccess,
    error: mutationError,
    resetMutationState,
  } = useWorkouts();

  // Handle delete success
  useEffect(() => {
    if (deleteSuccess) {
      setSuccessMessage('Workout deleted successfully!');
      setDeleteDialogOpen(false);
      setWorkoutToDelete(null);
    }
  }, [deleteSuccess]);

  // Clear success message after delay
  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => {
        setSuccessMessage(null);
        resetMutationState();
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [successMessage, resetMutationState]);

  // Handle mutation errors
  useEffect(() => {
    if (mutationError) {
      const errorMsg =
        mutationError instanceof ApiException
          ? mutationError.getUserMessage()
          : (mutationError as Error).message || 'An error occurred';
      setErrorMessage(errorMsg);
    }
  }, [mutationError]);

  // Handlers
  const handleView = (workout: Workout) => {
    navigate(`/workouts/${workout.id}`);
  };

  const handleEdit = (workout: Workout) => {
    navigate(`/workouts/${workout.id}/edit`);
  };

  const handleDelete = (workout: Workout) => {
    setWorkoutToDelete(workout);
    setDeleteDialogOpen(true);
    setErrorMessage(null);
  };

  const handleConfirmDelete = async () => {
    if (!workoutToDelete) return;
    try {
      await deleteWorkout(workoutToDelete.id);
    } catch (err) {
      console.error('Failed to delete workout:', err);
    }
  };

  const handleDeleteDialogClose = () => {
    if (!isDeleting) {
      setDeleteDialogOpen(false);
      setWorkoutToDelete(null);
    }
  };

  // Error state
  if (queryError && workouts.length === 0) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <Alert variant="error" title="Error">
          {queryError.getUserMessage()}
        </Alert>
        <Button variant="outline" onClick={() => refetch()} className="mt-4">
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="mb-6">
        {isLoading ? (
          <HeaderSkeleton />
        ) : (
          <>
            <div className="flex items-center justify-between">
              <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">
                Workouts
              </h1>
              <Link to="/workouts/new">
                <Button variant="primary">New Workout</Button>
              </Link>
            </div>
            <p className="mt-2 text-sm text-gray-600">
              Parse and save your workouts for strategy analysis.
            </p>
          </>
        )}
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="mb-6">
          <Alert
            variant="success"
            dismissible
            onDismiss={() => setSuccessMessage(null)}
          >
            {successMessage}
          </Alert>
        </div>
      )}

      {/* Error Message */}
      {errorMessage && (
        <div className="mb-6">
          <Alert
            variant="error"
            dismissible
            onDismiss={() => setErrorMessage(null)}
          >
            {errorMessage}
          </Alert>
        </div>
      )}

      {/* Workouts List */}
      <WorkoutsList
        workouts={workouts}
        onView={handleView}
        onEdit={handleEdit}
        onDelete={handleDelete}
        isLoading={isLoading}
        emptyMessage="No workouts yet. Create your first workout to get started!"
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={deleteDialogOpen}
        onClose={handleDeleteDialogClose}
        onConfirm={handleConfirmDelete}
        title="Delete Workout"
        message={`Are you sure you want to delete "${workoutToDelete?.name || workoutToDelete?.parsedDescription || 'this workout'}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};
