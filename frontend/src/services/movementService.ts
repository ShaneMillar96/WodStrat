import { api } from './api';
import type { MovementDefinition, MovementLookupResult } from '../types/movement';

/**
 * Movement API service
 * Provides methods for movement definition queries and lookups
 */
export const movementService = {
  /**
   * Get all movement definitions
   * @param category - Optional category filter (Weightlifting, Gymnastics, Cardio, Strongman)
   * @returns List of movement definitions
   */
  getAll: async (category?: string): Promise<MovementDefinition[]> => {
    const endpoint = category
      ? `/movements?category=${encodeURIComponent(category)}`
      : '/movements';
    return api.get<MovementDefinition[]>(endpoint);
  },

  /**
   * Get a movement definition by canonical name
   * @param canonicalName - The canonical name (e.g., "toes_to_bar", "pull_up")
   * @returns The movement definition
   * @throws ApiException if not found (404)
   */
  getByCanonicalName: async (canonicalName: string): Promise<MovementDefinition> => {
    return api.get<MovementDefinition>(`/movements/${encodeURIComponent(canonicalName)}`);
  },

  /**
   * Lookup a movement by alias
   * Performs direct alias-to-movement lookup.
   * @param alias - The alias to lookup (e.g., "T2B", "C2B", "HSPU")
   * @returns The matched movement definition
   * @throws ApiException if not found (404)
   */
  lookup: async (alias: string): Promise<MovementDefinition> => {
    return api.get<MovementDefinition>(
      `/movements/lookup?alias=${encodeURIComponent(alias)}`
    );
  },

  /**
   * Search for a movement by alias or name
   * Searches across aliases, display names, and canonical names.
   * Returns null if no match is found (unlike lookup which throws 404).
   * @param query - The search query
   * @returns The matched movement definition, or null if not found
   */
  search: async (query: string): Promise<MovementLookupResult> => {
    return api.get<MovementLookupResult>(
      `/movements/search?q=${encodeURIComponent(query)}`
    );
  },
};
