import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Alert, Button } from '../components/ui';
import {
  WorkoutInputForm,
  ParsedWorkoutPreview,
  WorkoutEditForm,
} from '../components/workouts';
import { useWorkoutParsing } from '../hooks/useWorkoutParsing';
import { useWorkoutMutations } from '../hooks/useWorkouts';
import { ApiException } from '../services';
import type { WorkoutInputSchemaType } from '../schemas/workoutSchema';
import { convertTimeCapToSeconds } from '../schemas/workoutSchema';
import type { CreateWorkoutMovementRequest } from '../types/workout';

/**
 * Form data structure from WorkoutEditForm
 */
interface WorkoutEditFormData {
  name?: string;
  workoutType: 'Amrap' | 'ForTime' | 'Emom' | 'Intervals' | 'Rounds';
  timeCapMinutes?: string;
  roundCount?: string;
  intervalDurationSeconds?: string;
  movements: CreateWorkoutMovementRequest[];
}

type PageStep = 'input' | 'preview' | 'edit';

/**
 * Workout input page
 * Allows users to paste workout text, preview parsed result, and save
 */
export const WorkoutInputPage: React.FC = () => {
  const navigate = useNavigate();

  // State
  const [step, setStep] = useState<PageStep>('input');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Hooks
  const {
    parseWorkout,
    parsedWorkout,
    isParsing,
    parseError,
    resetParsing,
  } = useWorkoutParsing();

  const {
    createWorkout,
    isCreating,
    createSuccess,
    error: createError,
    resetMutationState,
  } = useWorkoutMutations();

  // Handle create success
  useEffect(() => {
    if (createSuccess) {
      setSuccessMessage('Workout saved successfully!');
      setTimeout(() => {
        navigate('/workouts');
      }, 1500);
    }
  }, [createSuccess, navigate]);

  // Handle errors
  useEffect(() => {
    if (parseError) {
      const errorMsg =
        parseError instanceof ApiException
          ? parseError.getUserMessage()
          : (parseError as Error).message || 'Failed to parse workout';
      setErrorMessage(errorMsg);
    }
  }, [parseError]);

  useEffect(() => {
    if (createError) {
      const errorMsg =
        createError instanceof ApiException
          ? createError.getUserMessage()
          : (createError as Error).message || 'Failed to save workout';
      setErrorMessage(errorMsg);
    }
  }, [createError]);

  // Handlers
  const handleParse = async (data: WorkoutInputSchemaType) => {
    setErrorMessage(null);
    try {
      await parseWorkout(data.text);
      setStep('preview');
    } catch (err) {
      // Error handled by effect
    }
  };

  const handleContinueToEdit = () => {
    setStep('edit');
  };

  const handleBackToInput = () => {
    setStep('input');
    resetParsing();
    resetMutationState();
    setErrorMessage(null);
  };

  const handleBackToPreview = () => {
    setStep('preview');
    setErrorMessage(null);
  };

  const handleSave = async (data: WorkoutEditFormData) => {
    if (!parsedWorkout) return;

    setErrorMessage(null);
    try {
      await createWorkout({
        name: data.name || null,
        workoutType: data.workoutType,
        originalText: parsedWorkout.originalText,
        timeCapSeconds: convertTimeCapToSeconds(data.timeCapMinutes),
        roundCount: data.roundCount ? Number(data.roundCount) : null,
        intervalDurationSeconds: data.intervalDurationSeconds
          ? Number(data.intervalDurationSeconds)
          : null,
        movements: data.movements.map((m, index) => ({
          ...m,
          sequenceOrder: index + 1,
        })),
      });
    } catch (err) {
      // Error handled by effect
    }
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">
            {step === 'input' && 'New Workout'}
            {step === 'preview' && 'Preview Workout'}
            {step === 'edit' && 'Confirm & Save'}
          </h1>
          <Link to="/workouts">
            <Button variant="outline" size="sm">
              Back to Workouts
            </Button>
          </Link>
        </div>

        {/* Step indicator */}
        <div className="mt-4 flex items-center gap-2">
          <StepIndicator number={1} label="Input" active={step === 'input'} completed={step !== 'input'} />
          <div className="h-px w-8 bg-gray-300" />
          <StepIndicator number={2} label="Preview" active={step === 'preview'} completed={step === 'edit'} />
          <div className="h-px w-8 bg-gray-300" />
          <StepIndicator number={3} label="Save" active={step === 'edit'} completed={false} />
        </div>
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="mb-6">
          <Alert variant="success">{successMessage}</Alert>
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

      {/* Step Content */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        {step === 'input' && (
          <WorkoutInputForm
            onSubmit={handleParse}
            isSubmitting={isParsing}
          />
        )}

        {step === 'preview' && parsedWorkout && (
          <div className="space-y-6">
            <ParsedWorkoutPreview parsedWorkout={parsedWorkout} />

            <div className="flex justify-between border-t border-gray-200 pt-4">
              <Button variant="outline" onClick={handleBackToInput}>
                Back to Input
              </Button>
              <Button
                variant="primary"
                onClick={handleContinueToEdit}
                disabled={!parsedWorkout.isValid && parsedWorkout.movements.filter(m => m.movementDefinitionId).length === 0}
              >
                Continue to Save
              </Button>
            </div>
          </div>
        )}

        {step === 'edit' && parsedWorkout && (
          <WorkoutEditForm
            parsedWorkout={parsedWorkout}
            onSubmit={handleSave}
            onCancel={handleBackToPreview}
            isSubmitting={isCreating}
          />
        )}
      </div>
    </div>
  );
};

/**
 * Step indicator component
 */
const StepIndicator: React.FC<{
  number: number;
  label: string;
  active: boolean;
  completed: boolean;
}> = ({ number, label, active, completed }) => (
  <div className="flex items-center gap-2">
    <div
      className={`flex h-6 w-6 items-center justify-center rounded-full text-xs font-medium ${
        active
          ? 'bg-primary-600 text-white'
          : completed
          ? 'bg-green-600 text-white'
          : 'bg-gray-200 text-gray-600'
      }`}
    >
      {completed ? '\u2713' : number}
    </div>
    <span className={`text-sm ${active ? 'font-medium text-gray-900' : 'text-gray-500'}`}>
      {label}
    </span>
  </div>
);
