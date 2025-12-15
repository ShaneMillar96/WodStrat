import React from 'react';
import { Label } from './Label';

export interface FormFieldProps {
  /** Field label */
  label: string;
  /** Unique ID for the form control */
  htmlFor: string;
  /** Whether the field is required */
  required?: boolean;
  /** Error message to display */
  error?: string;
  /** Hint text to display below the field */
  hint?: string;
  /** Form control element */
  children: React.ReactNode;
  /** Additional class names */
  className?: string;
}

/**
 * Form field wrapper component with label, hint, and error message
 */
export const FormField: React.FC<FormFieldProps> = ({
  label,
  htmlFor,
  required = false,
  error,
  hint,
  children,
  className = '',
}) => {
  const errorId = error ? `${htmlFor}-error` : undefined;
  const hintId = hint ? `${htmlFor}-hint` : undefined;

  return (
    <div className={`space-y-1 ${className}`}>
      <Label htmlFor={htmlFor} required={required}>
        {label}
      </Label>
      {/* Clone child to add aria attributes */}
      {React.isValidElement(children)
        ? React.cloneElement(children as React.ReactElement<Record<string, unknown>>, {
            id: htmlFor,
            'aria-describedby': [hintId, errorId].filter(Boolean).join(' ') || undefined,
            'aria-invalid': error ? 'true' : undefined,
          })
        : children}
      {hint && !error && (
        <p id={hintId} className="text-sm text-gray-500">
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className="text-sm text-red-600" role="alert">
          {error}
        </p>
      )}
    </div>
  );
};
