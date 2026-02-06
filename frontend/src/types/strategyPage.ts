/**
 * Tab identifiers for the Strategy Page
 */
export type StrategyTabId = 'overview' | 'pacing' | 'volume' | 'timing';

/**
 * Tab configuration for strategy page
 */
export interface StrategyTab {
  id: StrategyTabId;
  label: string;
  icon?: string;
}

/**
 * Combined movement analysis for unified display
 */
export interface MovementAnalysis {
  movementName: string;
  movementDefinitionId: number;
  /** Pacing data */
  pacingLevel?: 'Light' | 'Moderate' | 'Heavy';
  pacingGuidance?: string;
  pacingPercentile?: number | null;
  pacingBenchmark?: string | null;
  recommendedSets?: number[];
  /** Volume data */
  volumeLoad?: number;
  volumeLoadFormatted?: string;
  loadClassification?: 'High' | 'Moderate' | 'Low';
  volumePercentile?: number | null;
  volumeBenchmark?: string | null;
  volumeTip?: string;
  recommendedWeight?: number | null;
  recommendedWeightFormatted?: string | null;
}

/**
 * Props for quick metrics display
 */
export interface QuickMetrics {
  timeEstimate?: string;
  totalVolume?: string;
  benchmarkCoverage?: string;
  averagePercentile?: number | null;
}

/**
 * Strategy tabs configuration array
 */
export const STRATEGY_TABS: StrategyTab[] = [
  { id: 'overview', label: 'Overview' },
  { id: 'pacing', label: 'Pacing' },
  { id: 'volume', label: 'Volume' },
  { id: 'timing', label: 'Timing' },
];
