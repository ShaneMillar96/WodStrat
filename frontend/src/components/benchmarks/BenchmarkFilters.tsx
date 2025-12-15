import React from 'react';
import { Tabs, type TabOption } from '../ui';
import type { BenchmarkCategory } from '../../types';
import { BENCHMARK_CATEGORY_LABELS, ALL_BENCHMARK_CATEGORIES } from '../../types';

export type FilterValue = BenchmarkCategory | 'All';

export interface BenchmarkFiltersProps {
  /** Currently selected filter */
  value: FilterValue;
  /** Callback when filter changes */
  onChange: (value: FilterValue) => void;
  /** Counts by category for badges */
  countsByCategory?: Record<string, number>;
  /** Total recorded count for "All" tab */
  totalRecorded?: number;
  /** Additional class names */
  className?: string;
}

/**
 * Category filter tabs for benchmark list
 */
export const BenchmarkFilters: React.FC<BenchmarkFiltersProps> = ({
  value,
  onChange,
  countsByCategory = {},
  totalRecorded,
  className = '',
}) => {
  // Build tab options
  const options: TabOption<FilterValue>[] = [
    {
      value: 'All' as FilterValue,
      label: 'All',
      count: totalRecorded,
    },
    ...ALL_BENCHMARK_CATEGORIES.map((category) => ({
      value: category as FilterValue,
      label: BENCHMARK_CATEGORY_LABELS[category],
      count: countsByCategory[category] ?? 0,
    })),
  ];

  return (
    <div className={`mb-4 overflow-x-auto ${className}`}>
      <Tabs
        options={options}
        value={value}
        onChange={onChange}
        variant="underline"
      />
    </div>
  );
};
