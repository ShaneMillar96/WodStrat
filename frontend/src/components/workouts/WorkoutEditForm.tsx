import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button, Input, Select, FormField, Alert } from '../ui';
import { convertTimeCapToMinutes } from '../../schemas/workoutSchema';
import type { ParsedWorkout, WorkoutType, CreateWorkoutMovementRequest } from '../../types/workout';
import { ALL_WORKOUT_TYPES, WORKOUT_TYPE_LABELS } from '../../types/workout';

/**
 * Simplified form schema for workout edit (without movements array)
 * Movements are handled separately since they come from the parser
 */
const workoutEditFormSchema = z.object({
  name: z
    .string()
    .max(100, 'Name must be 100 characters or less')
    .optional(),
  workoutType: z.enum(['Amrap', 'ForTime', 'Emom', 'Intervals', 'Rounds'], {
    message: 'Please select a valid workout type',
  }),
  timeCapMinutes: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number(val) > 0),
      { message: 'Time cap must be a positive number' }
    ),
  roundCount: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number.isInteger(Number(val)) && Number(val) > 0),
      { message: 'Round count must be a positive integer' }
    ),
  intervalDurationSeconds: z
    .string()
    .optional()
    .refine(
      (val) => !val || (!isNaN(Number(val)) && Number.isInteger(Number(val)) && Number(val) > 0),
      { message: 'Interval duration must be a positive integer' }
    ),
});

type WorkoutEditFormData = z.infer<typeof workoutEditFormSchema>;

export interface WorkoutEditFormProps {
  parsedWorkout: ParsedWorkout;
  onSubmit: (data: WorkoutEditFormData & { movements: CreateWorkoutMovementRequest[] }) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  error?: string | null;
}

/**
 * Form component for editing a parsed workout before saving
 */
export const WorkoutEditForm: React.FC<WorkoutEditFormProps> = ({
  parsedWorkout,
  onSubmit,
  onCancel,
  isSubmitting = false,
  error = null,
}) => {
  // Convert parsed workout to form default values
  const defaultMovements: CreateWorkoutMovementRequest[] = parsedWorkout.movements
    .filter(m => m.movementDefinitionId !== null)
    .map(m => ({
      movementDefinitionId: m.movementDefinitionId!,
      sequenceOrder: m.sequenceOrder,
      repCount: m.repCount ?? (m.repSchemeReps?.reduce((sum, r) => sum + r, 0) || null),
      loadValue: m.loadValue,
      loadUnit: m.loadUnit,
      distanceValue: m.distanceValue,
      distanceUnit: m.distanceUnit,
      calories: m.calories,
      durationSeconds: m.durationSeconds,
      notes: m.notes,
    }));

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<WorkoutEditFormData>({
    resolver: zodResolver(workoutEditFormSchema),
    defaultValues: {
      name: '',
      workoutType: parsedWorkout.workoutType,
      timeCapMinutes: convertTimeCapToMinutes(parsedWorkout.timeCapSeconds),
      roundCount: parsedWorkout.roundCount?.toString() ?? '',
      intervalDurationSeconds: parsedWorkout.intervalDurationSeconds?.toString() ?? '',
    },
  });

  const workoutType = watch('workoutType') as WorkoutType;

  // Workout type options for select
  const workoutTypeOptions = ALL_WORKOUT_TYPES.map(type => ({
    value: type,
    label: WORKOUT_TYPE_LABELS[type],
  }));

  // Determine which fields to show based on workout type
  const showTimeCap = workoutType === 'Amrap' || workoutType === 'ForTime';
  const showRoundCount = workoutType === 'ForTime' || workoutType === 'Rounds';
  const showIntervalDuration = workoutType === 'Emom' || workoutType === 'Intervals';

  // Handle form submission with movements added
  const handleFormSubmit = (data: WorkoutEditFormData) => {
    onSubmit({
      ...data,
      movements: defaultMovements,
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      {/* Workout Name */}
      <FormField
        label="Workout Name"
        htmlFor="workout-name"
        error={errors.name?.message}
        hint="Optional - e.g., 'Cindy', 'Monday WOD'"
      >
        <Input
          {...register('name')}
          placeholder="Enter workout name (optional)"
          error={!!errors.name}
          disabled={isSubmitting}
        />
      </FormField>

      {/* Workout Type */}
      <FormField
        label="Workout Type"
        htmlFor="workout-type"
        error={errors.workoutType?.message}
        required
      >
        <Select
          {...register('workoutType')}
          options={workoutTypeOptions}
          error={!!errors.workoutType}
          disabled={isSubmitting}
        />
      </FormField>

      {/* Time Cap (for AMRAP, For Time) */}
      {showTimeCap && (
        <FormField
          label="Time Cap (minutes)"
          htmlFor="time-cap"
          error={errors.timeCapMinutes?.message}
        >
          <Input
            {...register('timeCapMinutes')}
            type="number"
            min="1"
            placeholder="e.g., 20"
            error={!!errors.timeCapMinutes}
            disabled={isSubmitting}
          />
        </FormField>
      )}

      {/* Round Count (for For Time, Rounds) */}
      {showRoundCount && (
        <FormField
          label="Number of Rounds"
          htmlFor="round-count"
          error={errors.roundCount?.message}
        >
          <Input
            {...register('roundCount')}
            type="number"
            min="1"
            placeholder="e.g., 5"
            error={!!errors.roundCount}
            disabled={isSubmitting}
          />
        </FormField>
      )}

      {/* Interval Duration (for EMOM, Intervals) */}
      {showIntervalDuration && (
        <FormField
          label="Interval Duration (seconds)"
          htmlFor="interval-duration"
          error={errors.intervalDurationSeconds?.message}
        >
          <Input
            {...register('intervalDurationSeconds')}
            type="number"
            min="1"
            placeholder="e.g., 60"
            error={!!errors.intervalDurationSeconds}
            disabled={isSubmitting}
          />
        </FormField>
      )}

      {/* Movements Summary */}
      <div className="rounded-lg border border-gray-200 p-4 bg-gray-50">
        <h4 className="font-medium text-gray-900 mb-2">
          Movements ({defaultMovements.length})
        </h4>
        <div className="text-sm text-gray-600 space-y-1">
          {parsedWorkout.movements.map((m, i) => (
            <div key={i} className={m.movementDefinitionId ? '' : 'text-red-600'}>
              {m.sequenceOrder}. {m.repCount && `${m.repCount} `}
              {m.movementName || m.originalText}
              {m.loadFormatted && ` (${m.loadFormatted})`}
              {!m.movementDefinitionId && ' - Unrecognized'}
            </div>
          ))}
        </div>
        {parsedWorkout.movements.some(m => !m.movementDefinitionId) && (
          <p className="text-sm text-yellow-600 mt-2">
            Note: Unrecognized movements will be excluded from the saved workout.
          </p>
        )}
      </div>

      {error && (
        <Alert variant="error">{error}</Alert>
      )}

      {/* Form Actions */}
      <div className="flex justify-end gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="primary"
          disabled={isSubmitting || defaultMovements.length === 0}
        >
          {isSubmitting ? 'Saving...' : 'Save Workout'}
        </Button>
      </div>
    </form>
  );
};
