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
   * Get all benchmarks for an athlete
   * @param athleteId - The athlete's unique identifier
   * @returns List of athlete's benchmark results
   */
  getAthleteBenchmarks: async (athleteId: number): Promise<AthleteBenchmark[]> => {
    return api.get<AthleteBenchmark[]>(`/athletes/${athleteId}/benchmarks`);
  },

  /**
   * Get benchmark summary for an athlete
   * @param athleteId - The athlete's unique identifier
   * @returns Summary with completion stats and all benchmarks
   */
  getSummary: async (athleteId: number): Promise<BenchmarkSummary> => {
    return api.get<BenchmarkSummary>(`/athletes/${athleteId}/benchmarks/summary`);
  },

  /**
   * Create a new benchmark result for an athlete
   * @param athleteId - The athlete's unique identifier
   * @param data - The benchmark data to create
   * @returns The created benchmark result
   */
  create: async (
    athleteId: number,
    data: CreateBenchmarkRequest
  ): Promise<AthleteBenchmark> => {
    return api.post<AthleteBenchmark, CreateBenchmarkRequest>(
      `/athletes/${athleteId}/benchmarks`,
      data
    );
  },

  /**
   * Update an existing benchmark result
   * @param athleteId - The athlete's unique identifier
   * @param benchmarkId - The benchmark result's unique identifier
   * @param data - The benchmark data to update
   * @returns The updated benchmark result
   */
  update: async (
    athleteId: number,
    benchmarkId: number,
    data: UpdateBenchmarkRequest
  ): Promise<AthleteBenchmark> => {
    return api.put<AthleteBenchmark, UpdateBenchmarkRequest>(
      `/athletes/${athleteId}/benchmarks/${benchmarkId}`,
      data
    );
  },

  /**
   * Delete a benchmark result
   * @param athleteId - The athlete's unique identifier
   * @param benchmarkId - The benchmark result's unique identifier
   */
  delete: async (athleteId: number, benchmarkId: number): Promise<void> => {
    return api.delete(`/athletes/${athleteId}/benchmarks/${benchmarkId}`);
  },
};
