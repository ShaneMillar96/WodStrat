import React from 'react';
import type { MovementCategoryIconProps } from '../../../types/workoutPreview';
import { CATEGORY_ICONS, CATEGORY_BG_COLORS, SIZE_STYLES } from '../../../utils/workoutStyles';

/**
 * Icon component for displaying movement category with emoji
 */
export const MovementCategoryIcon: React.FC<MovementCategoryIconProps> = ({
  category,
  size = 'md',
  className = '',
}) => {
  if (!category) return null;

  const icon = CATEGORY_ICONS[category] || '\u{1F4AA}'; // Default to flexed bicep
  const bgColor = CATEGORY_BG_COLORS[category] || 'bg-gray-100';
  const sizeStyle = SIZE_STYLES[size];

  // Size-based icon container dimensions
  const containerSizes = {
    sm: 'w-5 h-5',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
  };

  return (
    <span
      className={`inline-flex items-center justify-center rounded-full ${bgColor} ${containerSizes[size]} ${className}`}
      title={category}
      role="img"
      aria-label={category}
    >
      <span className={sizeStyle.icon}>{icon}</span>
    </span>
  );
};
