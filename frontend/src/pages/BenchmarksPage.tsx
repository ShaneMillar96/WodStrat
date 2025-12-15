import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Alert, Button, ConfirmDialog } from '../components/ui';
import {
  BenchmarkSummary,
  BenchmarkFilters,
  BenchmarkTable,
  BenchmarkModal,
  type FilterValue,
} from '../components/benchmarks';
import { useAthleteBenchmarks, useBenchmarkMutations } from '../hooks';
import { ApiException } from '../services';
import type { BenchmarkSchemaType } from '../schemas/benchmarkSchema';
import { convertFormValueToNumber } from '../schemas/benchmarkSchema';
import type { BenchmarkRowData } from '../types';

/**
 * Loading skeleton for the benchmarks page header
 */
const HeaderSkeleton: React.FC = () => (
  <div className="animate-pulse">
    <div className="h-8 w-48 rounded bg-gray-200" />
    <div className="mt-2 h-4 w-64 rounded bg-gray-200" />
  </div>
);

/**
 * Error display component
 */
const ErrorDisplay: React.FC<{
  error: ApiException | Error;
  onRetry?: () => void;
}> = ({ error, onRetry }) => {
  const isNotFound = error instanceof ApiException && error.isNotFound();
  const message =
    error instanceof ApiException
      ? error.getUserMessage()
      : error.message || 'An unexpected error occurred';

  return (
    <div className="space-y-4">
      <Alert variant="error" title={isNotFound ? 'Athlete Not Found' : 'Error'}>
        {message}
      </Alert>
      {onRetry && !isNotFound && (
        <Button variant="outline" onClick={onRetry}>
          Try Again
        </Button>
      )}
      {isNotFound && (
        <Link to="/profile/new">
          <Button variant="primary">Create Profile First</Button>
        </Link>
      )}
    </div>
  );
};

/**
 * Benchmarks page container component
 * Displays all benchmarks and allows recording/editing results
 */
export const BenchmarksPage: React.FC = () => {
  const { athleteId: athleteIdParam } = useParams<{ athleteId: string }>();
  const athleteId = athleteIdParam ? Number(athleteIdParam) : undefined;
  const isValidAthleteId = athleteId !== undefined && !isNaN(athleteId) && athleteId > 0;

  // State
  const [categoryFilter, setCategoryFilter] = useState<FilterValue>('All');
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedBenchmark, setSelectedBenchmark] = useState<BenchmarkRowData | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [benchmarkToDelete, setBenchmarkToDelete] = useState<BenchmarkRowData | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Hooks
  const {
    benchmarkRows,
    recordedCount,
    totalCount,
    meetsMinimumRequirement,
    minimumRequired,
    benchmarksByCategory,
    isLoading,
    error,
    refetchAll,
  } = useAthleteBenchmarks(
    isValidAthleteId ? athleteId : undefined,
    categoryFilter
  );

  const {
    createBenchmark,
    updateBenchmark,
    deleteBenchmark,
    isCreating,
    isUpdating,
    isDeleting,
    createSuccess,
    updateSuccess,
    deleteSuccess,
    error: mutationError,
    resetMutationState,
  } = useBenchmarkMutations(athleteId!);

  // Handle success messages
  useEffect(() => {
    if (createSuccess) {
      setSuccessMessage('Benchmark recorded successfully!');
      setModalOpen(false);
      setSelectedBenchmark(null);
    }
    if (updateSuccess) {
      setSuccessMessage('Benchmark updated successfully!');
      setModalOpen(false);
      setSelectedBenchmark(null);
    }
    if (deleteSuccess) {
      setSuccessMessage('Benchmark deleted successfully!');
      setDeleteDialogOpen(false);
      setBenchmarkToDelete(null);
    }
  }, [createSuccess, updateSuccess, deleteSuccess]);

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

  // Action handlers
  const handleAddEdit = (data: BenchmarkRowData) => {
    setSelectedBenchmark(data);
    setModalOpen(true);
    setErrorMessage(null);
  };

  const handleDelete = (data: BenchmarkRowData) => {
    setBenchmarkToDelete(data);
    setDeleteDialogOpen(true);
    setErrorMessage(null);
  };

  const handleModalClose = () => {
    if (!isCreating && !isUpdating) {
      setModalOpen(false);
      setSelectedBenchmark(null);
      setErrorMessage(null);
    }
  };

  const handleFormSubmit = async (formData: BenchmarkSchemaType) => {
    setErrorMessage(null);

    try {
      if (!selectedBenchmark) return;

      const numericValue = convertFormValueToNumber(
        formData.value,
        selectedBenchmark.definition.metricType
      );

      if (selectedBenchmark.athleteBenchmark) {
        // Update existing
        await updateBenchmark(selectedBenchmark.athleteBenchmark.id, {
          value: numericValue,
          recordedAt: formData.recordedAt,
          notes: formData.notes || null,
        });
      } else {
        // Create new
        await createBenchmark({
          benchmarkDefinitionId: Number(formData.benchmarkDefinitionId),
          value: numericValue,
          recordedAt: formData.recordedAt,
          notes: formData.notes || null,
        });
      }
    } catch (err) {
      // Error is handled by the mutation error effect
      console.error('Failed to save benchmark:', err);
    }
  };

  const handleConfirmDelete = async () => {
    if (!benchmarkToDelete?.athleteBenchmark) return;

    try {
      await deleteBenchmark(benchmarkToDelete.athleteBenchmark.id);
    } catch (err) {
      console.error('Failed to delete benchmark:', err);
    }
  };

  const handleDeleteDialogClose = () => {
    if (!isDeleting) {
      setDeleteDialogOpen(false);
      setBenchmarkToDelete(null);
    }
  };

  // Invalid athlete ID
  if (!isValidAthleteId) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <Alert variant="error" title="Invalid Athlete">
          Please provide a valid athlete ID.
        </Alert>
        <div className="mt-4">
          <Link to="/profile/new">
            <Button variant="primary">Create Profile</Button>
          </Link>
        </div>
      </div>
    );
  }

  // Error state
  if (error && !benchmarkRows.length) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <ErrorDisplay error={error} onRetry={refetchAll} />
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
                Benchmarks
              </h1>
              <Link to={`/profile/${athleteId}`}>
                <Button variant="outline" size="sm">
                  Back to Profile
                </Button>
              </Link>
            </div>
            <p className="mt-2 text-sm text-gray-600">
              Track your benchmark results to get personalized workout recommendations.
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

      {/* Summary */}
      <BenchmarkSummary
        recordedCount={recordedCount}
        totalCount={totalCount}
        meetsMinimumRequirement={meetsMinimumRequirement}
        minimumRequired={minimumRequired}
        isLoading={isLoading}
      />

      {/* Category Filters */}
      <BenchmarkFilters
        value={categoryFilter}
        onChange={setCategoryFilter}
        countsByCategory={benchmarksByCategory}
        totalRecorded={recordedCount}
      />

      {/* Benchmark Table/Cards */}
      <BenchmarkTable
        rows={benchmarkRows}
        onAction={handleAddEdit}
        onDelete={handleDelete}
        isLoading={isLoading}
        emptyMessage={
          categoryFilter === 'All'
            ? 'No benchmarks available'
            : `No ${categoryFilter} benchmarks found`
        }
      />

      {/* Add/Edit Modal */}
      <BenchmarkModal
        isOpen={modalOpen}
        onClose={handleModalClose}
        onSubmit={handleFormSubmit}
        definition={selectedBenchmark?.definition ?? null}
        existingBenchmark={selectedBenchmark?.athleteBenchmark}
        isSubmitting={isCreating || isUpdating}
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={deleteDialogOpen}
        onClose={handleDeleteDialogClose}
        onConfirm={handleConfirmDelete}
        title="Delete Benchmark"
        message={`Are you sure you want to delete your ${benchmarkToDelete?.definition.name} result? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};
