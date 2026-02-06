import React, { useState } from 'react';
import type { MovementPacing } from '../../types/pacing';
import type { MovementVolumeLoad } from '../../types/volumeLoad';
import { PacingBadge, SectionSkeleton } from '../ui';
import { LoadClassificationBadge } from '../workouts';

interface MovementRow {
  movementName: string;
  pacing?: MovementPacing;
  volume?: MovementVolumeLoad;
}

export interface MovementBreakdownTableProps {
  /** Pacing data for movements */
  pacingData?: MovementPacing[];
  /** Volume load data for movements */
  volumeData?: MovementVolumeLoad[];
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * MovementBreakdownTable - Combined table showing pacing and volume for each movement
 */
export const MovementBreakdownTable: React.FC<MovementBreakdownTableProps> = ({
  pacingData = [],
  volumeData = [],
  isLoading = false,
  className = '',
}) => {
  const [expandedRow, setExpandedRow] = useState<string | null>(null);

  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={5} className={className} />;
  }

  // Combine pacing and volume data by movement name
  const movementMap = new Map<string, MovementRow>();

  pacingData.forEach(p => {
    movementMap.set(p.movementName, {
      movementName: p.movementName,
      pacing: p,
    });
  });

  volumeData.forEach(v => {
    const existing = movementMap.get(v.movementName);
    if (existing) {
      existing.volume = v;
    } else {
      movementMap.set(v.movementName, {
        movementName: v.movementName,
        volume: v,
      });
    }
  });

  const rows = Array.from(movementMap.values());

  if (rows.length === 0) {
    return null;
  }

  const toggleRow = (movementName: string) => {
    setExpandedRow(expandedRow === movementName ? null : movementName);
  };

  return (
    <div className={`rounded-lg border border-gray-200 bg-white overflow-hidden ${className}`}>
      <div className="p-4 border-b border-gray-200 bg-gray-50">
        <h3 className="font-semibold text-gray-900">Movement Breakdown</h3>
      </div>

      <div className="divide-y divide-gray-200">
        {rows.map((row) => (
          <div key={row.movementName}>
            {/* Row header - clickable */}
            <button
              onClick={() => toggleRow(row.movementName)}
              className="w-full px-4 py-3 flex items-center justify-between hover:bg-gray-50 transition-colors text-left"
              aria-expanded={expandedRow === row.movementName}
              aria-controls={`movement-details-${row.movementName}`}
            >
              <div className="flex items-center gap-3 min-w-0 flex-1">
                <span className="font-medium text-gray-900 truncate">
                  {row.movementName}
                </span>
              </div>

              <div className="flex items-center gap-3 flex-shrink-0">
                {row.pacing && (
                  <PacingBadge pacingLevel={row.pacing.pacingLevel} size="sm" showLabel={false} />
                )}
                {row.volume && (
                  <>
                    <span className="text-sm font-medium text-gray-900">
                      {row.volume.volumeLoadFormatted}
                    </span>
                    <LoadClassificationBadge
                      classification={row.volume.loadClassification}
                      size="sm"
                      showLabel={false}
                    />
                  </>
                )}
                <svg
                  className={`h-5 w-5 text-gray-400 transition-transform ${
                    expandedRow === row.movementName ? 'rotate-180' : ''
                  }`}
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </div>
            </button>

            {/* Expanded content */}
            {expandedRow === row.movementName && (
              <div
                id={`movement-details-${row.movementName}`}
                className="px-4 pb-4 bg-gray-50 space-y-3"
              >
                {row.pacing && (
                  <div>
                    <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                      Pacing
                    </div>
                    <p className="text-sm text-gray-700">{row.pacing.guidanceText}</p>
                    {row.pacing.isCardio && row.pacing.targetPace ? (
                      <div className="flex items-baseline gap-2 mt-1">
                        <span className="text-sm font-semibold text-primary-700">
                          {row.pacing.targetPace.displayPrimary}
                        </span>
                        {row.pacing.targetPace.displaySecondary && (
                          <span className="text-xs text-gray-500">
                            ({row.pacing.targetPace.displaySecondary})
                          </span>
                        )}
                      </div>
                    ) : (
                      row.pacing.recommendedSets.length > 0 && (
                        <p className="text-sm text-primary-700 mt-1">
                          Recommended sets: {row.pacing.recommendedSets.join('-')}
                        </p>
                      )
                    )}
                  </div>
                )}
                {row.volume && (
                  <div>
                    <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                      Volume
                    </div>
                    <p className="text-sm text-gray-700">
                      {row.volume.weight} {row.volume.weightUnit} x {row.volume.reps} reps
                      {row.volume.rounds > 1 && ` x ${row.volume.rounds} rounds`}
                    </p>
                    {row.volume.tip && (
                      <p className="text-sm text-primary-700 mt-1">{row.volume.tip}</p>
                    )}
                  </div>
                )}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};
