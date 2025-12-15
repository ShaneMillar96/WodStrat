import React from 'react';
import { Badge, Button } from '../ui';
import type { BenchmarkRowData, BenchmarkCategory } from '../../types';
import { BENCHMARK_CATEGORY_COLORS } from '../../types';

export interface BenchmarkCardProps {
  /** Row data with definition and optional athlete benchmark */
  data: BenchmarkRowData;
  /** Callback when Add/Edit button is clicked */
  onAction: (data: BenchmarkRowData) => void;
  /** Callback when Delete is clicked (only if has value) */
  onDelete?: (data: BenchmarkRowData) => void;
}

/**
 * Get badge variant for category
 */
function getCategoryVariant(category: BenchmarkCategory): 'blue' | 'red' | 'green' | 'purple' {
  return BENCHMARK_CATEGORY_COLORS[category] as 'blue' | 'red' | 'green' | 'purple';
}

/**
 * Card component for displaying a benchmark on mobile
 */
export const BenchmarkCard: React.FC<BenchmarkCardProps> = ({
  data,
  onAction,
  onDelete,
}) => {
  const { definition, athleteBenchmark, hasValue } = data;

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
      {/* Header with name and category */}
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0 flex-1">
          <h3 className="font-medium text-gray-900 truncate">{definition.name}</h3>
          {definition.description && (
            <p className="mt-0.5 text-xs text-gray-500 line-clamp-2">
              {definition.description}
            </p>
          )}
        </div>
        <Badge variant={getCategoryVariant(definition.category)} size="sm">
          {definition.category}
        </Badge>
      </div>

      {/* Value and Date */}
      <div className="mt-3 flex items-end justify-between">
        <div>
          {hasValue && athleteBenchmark ? (
            <>
              <div className="text-lg font-semibold text-gray-900">
                {athleteBenchmark.formattedValue}
              </div>
              <div className="text-xs text-gray-500">
                {new Date(athleteBenchmark.recordedAt).toLocaleDateString()}
              </div>
              {athleteBenchmark.notes && (
                <div className="mt-1 text-xs text-gray-500 line-clamp-1">
                  {athleteBenchmark.notes}
                </div>
              )}
            </>
          ) : (
            <div className="text-gray-400">Not recorded</div>
          )}
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2">
          {hasValue && onDelete && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => onDelete(data)}
              className="text-red-600 hover:text-red-700 hover:border-red-300"
              aria-label={`Delete ${definition.name}`}
            >
              <svg
                className="h-4 w-4"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth="1.5"
                stroke="currentColor"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"
                />
              </svg>
            </Button>
          )}
          <Button
            variant={hasValue ? 'outline' : 'primary'}
            size="sm"
            onClick={() => onAction(data)}
          >
            {hasValue ? 'Edit' : 'Add'}
          </Button>
        </div>
      </div>
    </div>
  );
};
