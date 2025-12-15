import React from 'react';

export interface LabelProps extends React.LabelHTMLAttributes<HTMLLabelElement> {
  /** Whether the associated field is required */
  required?: boolean;
  /** Additional class names */
  className?: string;
  /** Label content */
  children: React.ReactNode;
}

/**
 * Form label component with optional required indicator
 */
export const Label: React.FC<LabelProps> = ({
  required = false,
  className = '',
  children,
  ...props
}) => {
  return (
    <label
      className={`block text-sm font-medium text-gray-700 ${className}`}
      {...props}
    >
      {children}
      {required && (
        <span className="ml-1 text-red-500" aria-hidden="true">
          *
        </span>
      )}
    </label>
  );
};
