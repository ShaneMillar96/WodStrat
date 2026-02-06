import React from 'react';
import { Alert } from './Alert';

export interface SectionErrorProps {
  /** Section title */
  title: string;
  /** Error message (optional, shows generic message if not provided) */
  message?: string;
  /** Whether to show retry button */
  showRetry?: boolean;
  /** Retry callback */
  onRetry?: () => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * SectionError - Error display for strategy sections
 */
export const SectionError: React.FC<SectionErrorProps> = ({
  title,
  message,
  showRetry = false,
  onRetry,
  className = '',
}) => {
  return (
    <Alert variant="info" title={title} className={className}>
      <p>{message || 'This data requires benchmark information. Add your benchmarks to see personalized analysis.'}</p>
      {showRetry && onRetry && (
        <button
          onClick={onRetry}
          className="mt-2 text-sm font-medium text-blue-700 hover:text-blue-800 underline"
        >
          Try again
        </button>
      )}
    </Alert>
  );
};
