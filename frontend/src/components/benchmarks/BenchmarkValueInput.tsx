import React, { useState, useEffect } from 'react';
import { Input } from '../ui';
import type { BenchmarkMetricType } from '../../types';
import {
  parseTimeToSeconds,
  formatSecondsToTime,
} from '../../schemas/benchmarkSchema';

export interface BenchmarkValueInputProps {
  /** The metric type determining input format */
  metricType: BenchmarkMetricType;
  /** The unit label */
  unit: string;
  /** Current value (as string for form) */
  value: string;
  /** Callback when value changes */
  onChange: (value: string) => void;
  /** Callback when input loses focus */
  onBlur?: () => void;
  /** Whether there's an error */
  error?: boolean;
  /** Whether the input is disabled */
  disabled?: boolean;
  /** Input name for form */
  name?: string;
  /** Input ID */
  id?: string;
}

/**
 * Smart input component for benchmark values
 * Handles different input formats based on metric type:
 * - Time: Accepts mm:ss or seconds, displays mm:ss
 * - Pace: Accepts mm:ss or seconds, displays mm:ss/500m
 * - Reps: Number input
 * - Weight: Number input with kg suffix
 */
export const BenchmarkValueInput: React.FC<BenchmarkValueInputProps> = ({
  metricType,
  unit,
  value,
  onChange,
  onBlur,
  error = false,
  disabled = false,
  name,
  id,
}) => {
  const [displayValue, setDisplayValue] = useState(value);

  // Sync display value when external value changes
  useEffect(() => {
    setDisplayValue(value);
  }, [value]);

  const isTimeInput = metricType === 'Time' || metricType === 'Pace';

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setDisplayValue(newValue);
    onChange(newValue);
  };

  const handleBlur = () => {
    if (isTimeInput) {
      // Try to normalize time format
      const seconds = parseTimeToSeconds(displayValue);
      if (seconds !== null) {
        const formatted = formatSecondsToTime(seconds);
        setDisplayValue(formatted);
        onChange(formatted);
      }
    }
    onBlur?.();
  };

  const getPlaceholder = (): string => {
    switch (metricType) {
      case 'Time':
        return 'e.g., 3:45 or 225';
      case 'Pace':
        return 'e.g., 1:45 or 105';
      case 'Reps':
        return 'e.g., 25';
      case 'Weight':
        return 'e.g., 100';
      default:
        return '';
    }
  };

  const getHintText = (): string => {
    switch (metricType) {
      case 'Time':
        return 'Enter as mm:ss or total seconds';
      case 'Pace':
        return 'Enter as mm:ss or total seconds';
      case 'Reps':
        return `Enter number of ${unit}`;
      case 'Weight':
        return `Enter weight in ${unit}`;
      default:
        return '';
    }
  };

  const getSuffix = (): string => {
    switch (metricType) {
      case 'Pace':
        return '/500m';
      case 'Weight':
        return ` ${unit}`;
      case 'Reps':
        return ` ${unit}`;
      default:
        return '';
    }
  };

  return (
    <div className="relative">
      <Input
        id={id}
        name={name}
        type={isTimeInput ? 'text' : 'number'}
        value={displayValue}
        onChange={handleChange}
        onBlur={handleBlur}
        placeholder={getPlaceholder()}
        error={error}
        disabled={disabled}
        min={isTimeInput ? undefined : 0}
        step={metricType === 'Weight' ? '0.5' : '1'}
        aria-describedby={`${id}-hint`}
        className={getSuffix() ? 'pr-16' : ''}
      />
      {getSuffix() && (
        <span className="absolute inset-y-0 right-0 flex items-center pr-3 text-sm text-gray-500">
          {getSuffix()}
        </span>
      )}
      <p id={`${id}-hint`} className="sr-only">
        {getHintText()}
      </p>
    </div>
  );
};
