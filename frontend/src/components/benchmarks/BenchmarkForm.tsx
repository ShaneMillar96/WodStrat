import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, Input, Textarea, Button } from '../ui';
import { BenchmarkValueInput } from './BenchmarkValueInput';
import {
  benchmarkSchema,
  type BenchmarkSchemaType,
  convertNumberToFormValue,
} from '../../schemas/benchmarkSchema';
import type { BenchmarkDefinition, AthleteBenchmark } from '../../types';

export interface BenchmarkFormProps {
  /** The benchmark definition being recorded/edited */
  definition: BenchmarkDefinition;
  /** Existing benchmark data for edit mode */
  existingBenchmark?: AthleteBenchmark | null;
  /** Callback when form is submitted */
  onSubmit: (data: BenchmarkSchemaType) => void;
  /** Callback when cancel is clicked */
  onCancel: () => void;
  /** Whether the form is submitting */
  isSubmitting?: boolean;
}

/**
 * Get default form values
 */
function getDefaultValues(
  definition: BenchmarkDefinition,
  existingBenchmark?: AthleteBenchmark | null
): BenchmarkSchemaType {
  if (existingBenchmark) {
    return {
      benchmarkDefinitionId: String(definition.id),
      value: convertNumberToFormValue(existingBenchmark.value, definition.metricType),
      recordedAt: existingBenchmark.recordedAt.split('T')[0], // Get date part only
      notes: existingBenchmark.notes || '',
    };
  }

  return {
    benchmarkDefinitionId: String(definition.id),
    value: '',
    recordedAt: new Date().toISOString().split('T')[0], // Today's date
    notes: '',
  };
}

/**
 * Form component for adding/editing a benchmark result
 */
export const BenchmarkForm: React.FC<BenchmarkFormProps> = ({
  definition,
  existingBenchmark,
  onSubmit,
  onCancel,
  isSubmitting = false,
}) => {
  const isEditMode = !!existingBenchmark;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<BenchmarkSchemaType>({
    resolver: zodResolver(benchmarkSchema),
    defaultValues: getDefaultValues(definition, existingBenchmark),
    mode: 'onBlur',
  });

  // Reset form when definition or existing benchmark changes
  useEffect(() => {
    reset(getDefaultValues(definition, existingBenchmark));
  }, [definition, existingBenchmark, reset]);

  const handleFormSubmit = (data: BenchmarkSchemaType) => {
    onSubmit(data);
  };

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className="space-y-4"
      noValidate
    >
      {/* Hidden benchmark definition ID */}
      <input
        type="hidden"
        {...register('benchmarkDefinitionId')}
      />

      {/* Benchmark Info (read-only display) */}
      <div className="rounded-md bg-gray-50 p-3">
        <div className="text-sm font-medium text-gray-900">
          {definition.name}
        </div>
        {definition.description && (
          <div className="mt-1 text-xs text-gray-500">
            {definition.description}
          </div>
        )}
        <div className="mt-1 text-xs text-gray-500">
          Category: {definition.category} | Unit: {definition.unit}
        </div>
      </div>

      {/* Value Input */}
      <FormField
        label="Value"
        htmlFor="value"
        required
        error={errors.value?.message}
        hint={
          definition.metricType === 'Time' || definition.metricType === 'Pace'
            ? 'Enter as mm:ss (e.g., 3:45) or total seconds'
            : `Enter value in ${definition.unit}`
        }
      >
        <Controller
          name="value"
          control={control}
          render={({ field }) => (
            <BenchmarkValueInput
              id="value"
              metricType={definition.metricType}
              unit={definition.unit}
              value={field.value}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={!!errors.value}
              disabled={isSubmitting}
            />
          )}
        />
      </FormField>

      {/* Date Recorded */}
      <FormField
        label="Date Recorded"
        htmlFor="recordedAt"
        required
        error={errors.recordedAt?.message}
      >
        <Input
          {...register('recordedAt')}
          type="date"
          max={new Date().toISOString().split('T')[0]}
          error={!!errors.recordedAt}
          disabled={isSubmitting}
        />
      </FormField>

      {/* Notes */}
      <FormField
        label="Notes"
        htmlFor="notes"
        error={errors.notes?.message}
        hint="Optional - add any context about this result (max 500 characters)"
      >
        <Textarea
          {...register('notes')}
          placeholder="e.g., PR, felt good, after injury..."
          maxLength={500}
          error={!!errors.notes}
          disabled={isSubmitting}
          rows={2}
        />
      </FormField>

      {/* Form Actions */}
      <div className="flex flex-col-reverse gap-3 pt-4 sm:flex-row sm:justify-end">
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
          loading={isSubmitting}
        >
          {isEditMode ? 'Update' : 'Save'}
        </Button>
      </div>
    </form>
  );
};
