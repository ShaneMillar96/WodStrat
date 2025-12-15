import React, { forwardRef } from 'react';

export interface RadioOption {
  value: string;
  label: string;
  description?: string;
}

export interface RadioGroupProps {
  /** Unique name for the radio group */
  name: string;
  /** Options for the radio buttons */
  options: RadioOption[];
  /** Currently selected value */
  value?: string;
  /** Callback when value changes */
  onChange?: (value: string) => void;
  /** Whether the radio group has an error */
  error?: boolean;
  /** Whether the radio group is disabled */
  disabled?: boolean;
  /** Additional class names */
  className?: string;
}

/**
 * Radio button group component with optional descriptions
 */
export const RadioGroup = forwardRef<HTMLDivElement, RadioGroupProps>(
  (
    {
      name,
      options,
      value,
      onChange,
      error = false,
      disabled = false,
      className = '',
    },
    ref
  ) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      onChange?.(e.target.value);
    };

    return (
      <div ref={ref} className={`space-y-3 ${className}`} role="radiogroup">
        {options.map((option) => (
          <label
            key={option.value}
            className={`flex cursor-pointer items-start rounded-lg border p-4 transition-colors ${
              value === option.value
                ? 'border-primary-500 bg-primary-50 ring-1 ring-primary-500'
                : error
                  ? 'border-red-300 hover:border-red-400'
                  : 'border-gray-200 hover:border-gray-300'
            } ${disabled ? 'cursor-not-allowed opacity-50' : ''}`}
          >
            <input
              type="radio"
              name={name}
              value={option.value}
              checked={value === option.value}
              onChange={handleChange}
              disabled={disabled}
              className="mt-0.5 h-4 w-4 border-gray-300 text-primary-600 focus:ring-primary-500"
              aria-describedby={
                option.description ? `${name}-${option.value}-description` : undefined
              }
            />
            <div className="ml-3">
              <span
                className={`block text-sm font-medium ${
                  value === option.value ? 'text-primary-900' : 'text-gray-900'
                }`}
              >
                {option.label}
              </span>
              {option.description && (
                <span
                  id={`${name}-${option.value}-description`}
                  className={`mt-1 block text-sm ${
                    value === option.value ? 'text-primary-700' : 'text-gray-500'
                  }`}
                >
                  {option.description}
                </span>
              )}
            </div>
          </label>
        ))}
      </div>
    );
  }
);

RadioGroup.displayName = 'RadioGroup';
