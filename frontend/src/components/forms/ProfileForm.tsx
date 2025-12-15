import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  FormField,
  Input,
  Select,
  RadioGroup,
  Button,
} from '../ui';
import { athleteSchema, type AthleteSchemaType } from '../../schemas/athleteSchema';
import type {
  Athlete,
  AthleteFormData,
  Gender,
  ExperienceLevel,
  AthleteGoal,
} from '../../types';
import {
  GENDER_LABELS,
  EXPERIENCE_LEVEL_LABELS,
  EXPERIENCE_LEVEL_DESCRIPTIONS,
  ATHLETE_GOAL_LABELS,
} from '../../types';

export interface ProfileFormProps {
  /** Existing athlete data for edit mode */
  athlete?: Athlete | null;
  /** Callback when form is submitted */
  onSubmit: (data: AthleteFormData) => void;
  /** Whether the form is submitting */
  isSubmitting?: boolean;
  /** Additional class names */
  className?: string;
}

/**
 * Convert form schema data to AthleteFormData
 */
function schemaToFormData(data: AthleteSchemaType): AthleteFormData {
  return {
    name: data.name,
    dateOfBirth: data.dateOfBirth || '',
    gender: (data.gender as Gender) || '',
    heightCm: data.heightCm || '',
    weightKg: data.weightKg || '',
    experienceLevel: data.experienceLevel as ExperienceLevel,
    primaryGoal: data.primaryGoal as AthleteGoal,
  };
}

/**
 * Get default form values from athlete data or empty defaults
 */
function getDefaultValues(athlete?: Athlete | null): AthleteSchemaType {
  if (athlete) {
    return {
      name: athlete.name,
      dateOfBirth: '', // DOB not returned from API for privacy
      gender: athlete.gender || undefined,
      heightCm: athlete.heightCm?.toString() || '',
      weightKg: athlete.weightKg?.toString() || '',
      experienceLevel: athlete.experienceLevel,
      primaryGoal: athlete.primaryGoal,
    };
  }
  return {
    name: '',
    dateOfBirth: '',
    gender: undefined,
    heightCm: '',
    weightKg: '',
    experienceLevel: 'Beginner' as const,
    primaryGoal: 'GeneralFitness' as const,
  };
}

/**
 * Gender select options
 */
const genderOptions = Object.entries(GENDER_LABELS).map(([value, label]) => ({
  value,
  label,
}));

/**
 * Experience level radio options
 */
const experienceLevelOptions = Object.entries(EXPERIENCE_LEVEL_LABELS).map(
  ([value, label]) => ({
    value,
    label,
    description: EXPERIENCE_LEVEL_DESCRIPTIONS[value as ExperienceLevel],
  })
);

/**
 * Primary goal select options
 */
const primaryGoalOptions = Object.entries(ATHLETE_GOAL_LABELS).map(
  ([value, label]) => ({
    value,
    label,
  })
);

/**
 * Profile form component with all athlete fields
 */
export const ProfileForm: React.FC<ProfileFormProps> = ({
  athlete,
  onSubmit,
  isSubmitting = false,
  className = '',
}) => {
  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors, isDirty },
  } = useForm<AthleteSchemaType>({
    resolver: zodResolver(athleteSchema),
    defaultValues: getDefaultValues(athlete),
    mode: 'onBlur',
  });

  // Reset form when athlete data changes (e.g., after loading)
  useEffect(() => {
    if (athlete) {
      reset(getDefaultValues(athlete));
    }
  }, [athlete, reset]);

  const handleFormSubmit = (data: AthleteSchemaType) => {
    onSubmit(schemaToFormData(data));
  };

  const handleReset = () => {
    reset(getDefaultValues(athlete));
  };

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className={`space-y-6 ${className}`}
      noValidate
    >
      {/* Name Field */}
      <FormField
        label="Name"
        htmlFor="name"
        required
        error={errors.name?.message}
      >
        <Input
          {...register('name')}
          placeholder="Enter your name"
          error={!!errors.name}
          disabled={isSubmitting}
        />
      </FormField>

      {/* Date of Birth Field */}
      <FormField
        label="Date of Birth"
        htmlFor="dateOfBirth"
        error={errors.dateOfBirth?.message}
        hint="Optional - used to calculate your age"
      >
        <Input
          {...register('dateOfBirth')}
          type="date"
          max={new Date().toISOString().split('T')[0]}
          error={!!errors.dateOfBirth}
          disabled={isSubmitting}
        />
      </FormField>

      {/* Gender Field */}
      <FormField
        label="Gender"
        htmlFor="gender"
        error={errors.gender?.message}
      >
        <Controller
          name="gender"
          control={control}
          render={({ field }) => (
            <Select
              {...field}
              value={field.value || ''}
              options={genderOptions}
              placeholder="Select gender (optional)"
              error={!!errors.gender}
              disabled={isSubmitting}
            />
          )}
        />
      </FormField>

      {/* Height and Weight Row */}
      <div className="grid gap-6 sm:grid-cols-2">
        {/* Height Field */}
        <FormField
          label="Height (cm)"
          htmlFor="heightCm"
          error={errors.heightCm?.message}
          hint="Optional - between 50-300 cm"
        >
          <Input
            {...register('heightCm')}
            type="number"
            min="50"
            max="300"
            step="0.1"
            placeholder="e.g., 175"
            error={!!errors.heightCm}
            disabled={isSubmitting}
          />
        </FormField>

        {/* Weight Field */}
        <FormField
          label="Weight (kg)"
          htmlFor="weightKg"
          error={errors.weightKg?.message}
          hint="Optional - between 20-500 kg"
        >
          <Input
            {...register('weightKg')}
            type="number"
            min="20"
            max="500"
            step="0.1"
            placeholder="e.g., 70"
            error={!!errors.weightKg}
            disabled={isSubmitting}
          />
        </FormField>
      </div>

      {/* Experience Level Field */}
      <FormField
        label="Experience Level"
        htmlFor="experienceLevel"
        required
        error={errors.experienceLevel?.message}
      >
        <Controller
          name="experienceLevel"
          control={control}
          render={({ field }) => (
            <RadioGroup
              name="experienceLevel"
              options={experienceLevelOptions}
              value={field.value}
              onChange={field.onChange}
              error={!!errors.experienceLevel}
              disabled={isSubmitting}
            />
          )}
        />
      </FormField>

      {/* Primary Goal Field */}
      <FormField
        label="Primary Goal"
        htmlFor="primaryGoal"
        required
        error={errors.primaryGoal?.message}
      >
        <Controller
          name="primaryGoal"
          control={control}
          render={({ field }) => (
            <Select
              {...field}
              value={field.value || ''}
              options={primaryGoalOptions}
              placeholder="Select your primary goal"
              error={!!errors.primaryGoal}
              disabled={isSubmitting}
            />
          )}
        />
      </FormField>

      {/* Form Actions */}
      <div className="flex flex-col-reverse gap-3 border-t border-gray-200 pt-6 sm:flex-row sm:justify-end">
        <Button
          type="button"
          variant="outline"
          onClick={handleReset}
          disabled={isSubmitting || !isDirty}
        >
          Reset
        </Button>
        <Button
          type="submit"
          variant="primary"
          loading={isSubmitting}
        >
          {athlete ? 'Update Profile' : 'Create Profile'}
        </Button>
      </div>
    </form>
  );
};
