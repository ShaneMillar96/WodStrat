import React, { forwardRef } from 'react';

export interface TextareaProps
  extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  /** Whether the textarea has an error */
  error?: boolean;
  /** Additional class names */
  className?: string;
}

/**
 * Textarea component with error state support
 */
export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ error = false, className = '', rows = 3, ...props }, ref) => {
    const baseStyles =
      'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0 resize-y';
    const normalStyles =
      'border-gray-300 focus:border-primary-500 focus:ring-primary-500';
    const errorStyles =
      'border-red-500 focus:border-red-500 focus:ring-red-500';
    const disabledStyles = 'disabled:bg-gray-100 disabled:cursor-not-allowed';

    return (
      <textarea
        ref={ref}
        rows={rows}
        className={`${baseStyles} ${error ? errorStyles : normalStyles} ${disabledStyles} ${className}`}
        aria-invalid={error ? 'true' : 'false'}
        {...props}
      />
    );
  }
);

Textarea.displayName = 'Textarea';
