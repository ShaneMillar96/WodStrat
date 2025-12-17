import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Textarea, FormField, Alert } from '../ui';
import { workoutInputSchema, type WorkoutInputSchemaType } from '../../schemas/workoutSchema';

export interface WorkoutInputFormProps {
  onSubmit: (data: WorkoutInputSchemaType) => void;
  isSubmitting?: boolean;
  error?: string | null;
}

/**
 * Example workout text for placeholder
 */
const EXAMPLE_WORKOUT = `20 min AMRAP
10 Pull-ups
15 Push-ups
20 Air Squats`;

/**
 * Form component for inputting workout text
 */
export const WorkoutInputForm: React.FC<WorkoutInputFormProps> = ({
  onSubmit,
  isSubmitting = false,
  error = null,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<WorkoutInputSchemaType>({
    resolver: zodResolver(workoutInputSchema),
    defaultValues: {
      text: '',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <FormField
        label="Workout Text"
        htmlFor="workout-text"
        error={errors.text?.message}
        required
      >
        <Textarea
          {...register('text')}
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

      {error && (
        <Alert variant="error">{error}</Alert>
      )}

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
