import { api } from './api';
import type { Athlete, CreateAthleteRequest, UpdateAthleteRequest } from '../types';

/**
 * Athlete API service
 * Provides methods for athlete profile CRUD operations
 */
export const athleteService = {
  /**
   * Get an athlete by ID
   * @param id - The athlete's unique identifier
   * @returns The athlete profile
   */
  getById: async (id: number): Promise<Athlete> => {
    return api.get<Athlete>(`/athletes/${id}`);
  },

  /**
   * Create a new athlete profile
   * @param data - The athlete data to create
   * @returns The created athlete profile
   */
  create: async (data: CreateAthleteRequest): Promise<Athlete> => {
    return api.post<Athlete, CreateAthleteRequest>('/athletes', data);
  },

  /**
   * Update an existing athlete profile
   * @param id - The athlete's unique identifier
   * @param data - The athlete data to update
   * @returns The updated athlete profile
   */
  update: async (id: number, data: UpdateAthleteRequest): Promise<Athlete> => {
    return api.put<Athlete, UpdateAthleteRequest>(`/athletes/${id}`, data);
  },

  /**
   * Delete an athlete profile
   * @param id - The athlete's unique identifier
   */
  delete: async (id: number): Promise<void> => {
    return api.delete(`/athletes/${id}`);
  },
};
