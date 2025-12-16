import { api } from './api';
import type {
  BenchmarkDefinition,
  AthleteBenchmark,
  BenchmarkSummary,
  CreateBenchmarkRequest,
  UpdateBenchmarkRequest,
} from '../types';

/**
 * Benchmark API service
 * Provides methods for benchmark CRUD operations
 * Note: Uses session-based identification via JWT token
 */
export const benchmarkService = {
  /**
   * Get all benchmark definitions
   * @returns List of all benchmark definitions
   */
  getDefinitions: async (): Promise<BenchmarkDefinition[]> => {
    return api.get<BenchmarkDefinition[]>('/benchmarks/definitions');
  },

  /**
   * Get all benchmarks for the current user's athlete
   * @returns List of athlete's benchmark results
   */
  getMyBenchmarks: async (): Promise<AthleteBenchmark[]> => {
    return api.get<AthleteBenchmark[]>('/benchmarks');
  },

  /**
   * Get benchmark summary for the current user's athlete
   * @returns Summary with completion stats and all benchmarks
   */
  getMySummary: async (): Promise<BenchmarkSummary> => {
    return api.get<BenchmarkSummary>('/benchmarks/summary');
  },

  /**
   * Create a new benchmark result
   * @param data - The benchmark data to create
   * @returns The created benchmark result
   */
  create: async (data: CreateBenchmarkRequest): Promise<AthleteBenchmark> => {
    return api.post<AthleteBenchmark, CreateBenchmarkRequest>(
      '/benchmarks',
      data
    );
  },

  /**
   * Update an existing benchmark result
   * @param benchmarkId - The benchmark result's unique identifier
   * @param data - The benchmark data to update
   * @returns The updated benchmark result
   */
  update: async (
    benchmarkId: number,
    data: UpdateBenchmarkRequest
  ): Promise<AthleteBenchmark> => {
    return api.put<AthleteBenchmark, UpdateBenchmarkRequest>(
      `/benchmarks/${benchmarkId}`,
      data
    );
  },

  /**
   * Delete a benchmark result
   * @param benchmarkId - The benchmark result's unique identifier
   */
  delete: async (benchmarkId: number): Promise<void> => {
    return api.delete(`/benchmarks/${benchmarkId}`);
  },
};
