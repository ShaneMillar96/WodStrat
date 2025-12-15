/**
 * Benchmark category enum values matching backend BenchmarkCategory.cs
 */
export type BenchmarkCategory = 'Cardio' | 'Strength' | 'Gymnastics' | 'HeroWod';

/**
 * Benchmark metric type enum values matching backend BenchmarkMetricType.cs
 */
export type BenchmarkMetricType = 'Time' | 'Reps' | 'Weight' | 'Pace';

/**
 * Benchmark definition as returned from the API
 */
export interface BenchmarkDefinition {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  category: BenchmarkCategory;
  metricType: BenchmarkMetricType;
  unit: string;
  displayOrder: number;
}

/**
 * Athlete benchmark result as returned from the API
 */
export interface AthleteBenchmark {
  id: number;
  athleteId: number;
  benchmarkDefinitionId: number;
  benchmarkName: string;
  benchmarkCategory: BenchmarkCategory;
  value: number;
  formattedValue: string;
  unit: string;
  recordedAt: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * Benchmark summary response from the API
 */
export interface BenchmarkSummary {
  athleteId: number;
  totalBenchmarks: number;
  meetsMinimumRequirement: boolean;
  minimumRequired: number;
  benchmarksByCategory: Record<string, number>;
  benchmarks: AthleteBenchmark[];
}

/**
 * Request payload for creating a new benchmark result
 */
export interface CreateBenchmarkRequest {
  benchmarkDefinitionId: number;
  value: number;
  recordedAt: string;
  notes?: string | null;
}

/**
 * Request payload for updating an existing benchmark result
 */
export interface UpdateBenchmarkRequest {
  value: number;
  recordedAt: string;
  notes?: string | null;
}

/**
 * Form data for benchmark entry form
 * Uses string types for form inputs
 */
export interface BenchmarkFormData {
  benchmarkDefinitionId: string;
  value: string;
  recordedAt: string;
  notes: string;
}

/**
 * Combined view model for displaying a benchmark row
 * Merges definition with athlete's recorded value (if any)
 */
export interface BenchmarkRowData {
  definition: BenchmarkDefinition;
  athleteBenchmark: AthleteBenchmark | null;
  hasValue: boolean;
}

/**
 * Display labels for benchmark category options
 */
export const BENCHMARK_CATEGORY_LABELS: Record<BenchmarkCategory, string> = {
  Cardio: 'Cardio',
  Strength: 'Strength',
  Gymnastics: 'Gymnastics',
  HeroWod: 'Hero WODs',
};

/**
 * Display labels for benchmark metric type
 */
export const BENCHMARK_METRIC_LABELS: Record<BenchmarkMetricType, string> = {
  Time: 'Time',
  Reps: 'Reps',
  Weight: 'Weight',
  Pace: 'Pace',
};

/**
 * Category badge color mapping
 */
export const BENCHMARK_CATEGORY_COLORS: Record<BenchmarkCategory, string> = {
  Cardio: 'blue',
  Strength: 'red',
  Gymnastics: 'green',
  HeroWod: 'purple',
};

/**
 * All benchmark categories for filtering
 */
export const ALL_BENCHMARK_CATEGORIES: BenchmarkCategory[] = [
  'Cardio',
  'Strength',
  'Gymnastics',
  'HeroWod',
];

/**
 * Total number of predefined benchmarks
 */
export const TOTAL_BENCHMARK_COUNT = 23;

/**
 * Minimum number of benchmarks required
 */
export const MINIMUM_BENCHMARK_REQUIREMENT = 3;
