import React from 'react';
import type { UnrecognizedMovementBadgeProps } from '../../../types/workoutPreview';

/**
 * Badge indicating a movement was not recognized by the parser
 */
export const UnrecognizedMovementBadge: React.FC<UnrecognizedMovementBadgeProps> = ({
  originalText,
  className = '',
}) => {
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800 ${className}`}
      title={`"${originalText}" was not recognized`}
    >
      Unrecognized
    </span>
  );
};
