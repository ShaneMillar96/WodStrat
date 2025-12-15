import React from 'react';
import { Badge, Button } from '../ui';
import type { BenchmarkRowData, BenchmarkCategory } from '../../types';
import { BENCHMARK_CATEGORY_COLORS } from '../../types';

export interface BenchmarkRowProps {
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
 * Table row component for displaying a single benchmark
 */
export const BenchmarkRow: React.FC<BenchmarkRowProps> = ({
  data,
  onAction,
  onDelete,
}) => {
  const { definition, athleteBenchmark, hasValue } = data;

  return (
    <tr className="hover:bg-gray-50">
      {/* Benchmark Name */}
      <td className="whitespace-nowrap px-4 py-3">
        <div>
          <div className="font-medium text-gray-900">{definition.name}</div>
          {definition.description && (
            <div className="text-xs text-gray-500 truncate max-w-xs" title={definition.description}>
              {definition.description}
            </div>
          )}
        </div>
      </td>

      {/* Category */}
      <td className="whitespace-nowrap px-4 py-3">
        <Badge variant={getCategoryVariant(definition.category)} size="sm">
          {definition.category}
        </Badge>
      </td>

      {/* Value */}
      <td className="whitespace-nowrap px-4 py-3 text-right">
        {hasValue && athleteBenchmark ? (
          <div>
            <span className="font-medium text-gray-900">
              {athleteBenchmark.formattedValue}
            </span>
            {athleteBenchmark.notes && (
              <div className="text-xs text-gray-500 truncate max-w-[150px]" title={athleteBenchmark.notes}>
                {athleteBenchmark.notes}
              </div>
            )}
          </div>
        ) : (
          <span className="text-gray-400">--</span>
        )}
      </td>

      {/* Date Recorded */}
      <td className="whitespace-nowrap px-4 py-3 text-sm text-gray-500">
        {hasValue && athleteBenchmark ? (
          new Date(athleteBenchmark.recordedAt).toLocaleDateString()
        ) : (
          <span className="text-gray-400">--</span>
        )}
      </td>

      {/* Actions */}
      <td className="whitespace-nowrap px-4 py-3 text-right">
        <div className="flex items-center justify-end gap-2">
          <Button
            variant={hasValue ? 'outline' : 'primary'}
            size="sm"
            onClick={() => onAction(data)}
          >
            {hasValue ? 'Edit' : 'Add'}
          </Button>
          {hasValue && onDelete && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => onDelete(data)}
              className="text-red-600 hover:text-red-700 hover:border-red-300"
            >
              Delete
            </Button>
          )}
        </div>
      </td>
    </tr>
  );
};
