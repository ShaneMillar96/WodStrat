import React, { forwardRef } from 'react';

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  /** Options for the select dropdown */
  options: SelectOption[];
  /** Placeholder text when no value is selected */
  placeholder?: string;
  /** Whether the select has an error */
  error?: boolean;
  /** Additional class names */
  className?: string;
}

/**
 * Select dropdown component with error state support
 */
export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ options, placeholder, error = false, className = '', ...props }, ref) => {
    const baseStyles =
      'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0 appearance-none bg-white bg-no-repeat';
    const normalStyles =
      'border-gray-300 focus:border-primary-500 focus:ring-primary-500';
    const errorStyles =
      'border-red-500 focus:border-red-500 focus:ring-red-500';
    const disabledStyles = 'disabled:bg-gray-100 disabled:cursor-not-allowed';

    // Custom dropdown arrow using background image
    const arrowStyles =
      "bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20fill%3D%22none%22%20viewBox%3D%220%200%2020%2020%22%3E%3Cpath%20stroke%3D%22%236B7280%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%20stroke-width%3D%221.5%22%20d%3D%22m6%208%204%204%204-4%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.25rem_1.25rem] bg-[right_0.5rem_center] pr-10";

    return (
      <select
        ref={ref}
        className={`${baseStyles} ${arrowStyles} ${error ? errorStyles : normalStyles} ${disabledStyles} ${className}`}
        aria-invalid={error ? 'true' : 'false'}
        {...props}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    );
  }
);

Select.displayName = 'Select';
