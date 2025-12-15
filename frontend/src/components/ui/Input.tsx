import React, { forwardRef } from 'react';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  /** Whether the input has an error */
  error?: boolean;
  /** Additional class names */
  className?: string;
}

/**
 * Text input component with error state support
 */
export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ error = false, className = '', type = 'text', ...props }, ref) => {
    const baseStyles =
      'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0';
    const normalStyles =
      'border-gray-300 focus:border-primary-500 focus:ring-primary-500';
    const errorStyles =
      'border-red-500 focus:border-red-500 focus:ring-red-500';
    const disabledStyles = 'disabled:bg-gray-100 disabled:cursor-not-allowed';

    return (
      <input
        ref={ref}
        type={type}
        className={`${baseStyles} ${error ? errorStyles : normalStyles} ${disabledStyles} ${className}`}
        aria-invalid={error ? 'true' : 'false'}
        {...props}
      />
    );
  }
);

Input.displayName = 'Input';
