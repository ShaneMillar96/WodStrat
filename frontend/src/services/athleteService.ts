import { api, ApiException } from './api';
import type { Athlete, CreateAthleteRequest, UpdateAthleteRequest } from '../types';

/**
 * Athlete API service
 * Provides methods for athlete profile CRUD operations
 * Note: Uses session-based identification via JWT token
 */
export const athleteService = {
  /**
   * Get the current user's athlete profile
   * @returns The athlete profile or null if not created
   */
  getMe: async (): Promise<Athlete | null> => {
    try {
      return await api.get<Athlete>('/athletes/me');
    } catch (error) {
      // Return null if athlete doesn't exist (404)
      if (error instanceof ApiException && error.isNotFound()) {
        return null;
      }
      throw error;
    }
  },

  /**
   * Get an athlete by ID (for backward compatibility)
   * @param id - The athlete's unique identifier
   * @returns The athlete profile
   */
  getById: async (id: number): Promise<Athlete> => {
    return api.get<Athlete>(`/athletes/${id}`);
  },

  /**
   * Create a new athlete profile for the current user
   * @param data - The athlete data to create
   * @returns The created athlete profile
   */
  create: async (data: CreateAthleteRequest): Promise<Athlete> => {
    return api.post<Athlete, CreateAthleteRequest>('/athletes', data);
  },

  /**
   * Update the current user's athlete profile
   * @param data - The athlete data to update
   * @returns The updated athlete profile
   */
  update: async (data: UpdateAthleteRequest): Promise<Athlete> => {
    return api.put<Athlete, UpdateAthleteRequest>('/athletes/me', data);
  },

  /**
   * Delete the current user's athlete profile
   */
  delete: async (): Promise<void> => {
    return api.delete('/athletes/me');
  },
};
