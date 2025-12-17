import { api } from './api';
import type {
  Workout,
  ParsedWorkout,
  ParseWorkoutRequest,
  CreateWorkoutRequest,
  UpdateWorkoutRequest,
  MovementDefinition,
} from '../types/workout';

/**
 * Workout API service
 * Provides methods for workout CRUD operations and parsing
 * Note: Uses session-based identification via JWT token
 */
export const workoutService = {
  /**
   * Parse workout text into structured data (preview without saving)
   * @param text - The raw workout text to parse
   * @returns Parsed workout structure with detected type, movements, and any errors
   */
  parse: async (text: string): Promise<ParsedWorkout> => {
    return api.post<ParsedWorkout, ParseWorkoutRequest>('/workouts/parse', { text });
  },

  /**
   * Create a new workout
   * @param data - The workout data to create
   * @returns The created workout
   */
  create: async (data: CreateWorkoutRequest): Promise<Workout> => {
    return api.post<Workout, CreateWorkoutRequest>('/workouts', data);
  },

  /**
   * Get all workouts for the current user
   * @returns List of user's workouts
   */
  getAll: async (): Promise<Workout[]> => {
    return api.get<Workout[]>('/workouts');
  },

  /**
   * Get a specific workout by ID
   * @param id - The workout's unique identifier
   * @returns The workout
   */
  getById: async (id: number): Promise<Workout> => {
    return api.get<Workout>(`/workouts/${id}`);
  },

  /**
   * Update an existing workout
   * @param id - The workout's unique identifier
   * @param data - The updated workout data
   * @returns The updated workout
   */
  update: async (id: number, data: UpdateWorkoutRequest): Promise<Workout> => {
    return api.put<Workout, UpdateWorkoutRequest>(`/workouts/${id}`, data);
  },

  /**
   * Delete a workout
   * @param id - The workout's unique identifier
   */
  delete: async (id: number): Promise<void> => {
    return api.delete(`/workouts/${id}`);
  },
};

/**
 * Movement API service
 * Provides methods for movement definition queries
 */
export const movementService = {
  /**
   * Get all movement definitions
   * @param category - Optional category filter
   * @returns List of movement definitions
   */
  getAll: async (category?: string): Promise<MovementDefinition[]> => {
    const endpoint = category ? `/movements?category=${category}` : '/movements';
    return api.get<MovementDefinition[]>(endpoint);
  },

  /**
   * Get a movement definition by canonical name
   * @param canonicalName - The canonical name
   * @returns The movement definition
   */
  getByCanonicalName: async (canonicalName: string): Promise<MovementDefinition> => {
    return api.get<MovementDefinition>(`/movements/${canonicalName}`);
  },

  /**
   * Search for a movement by alias or name
   * @param query - The search term
   * @returns The matched movement definition or null
   */
  search: async (query: string): Promise<MovementDefinition | null> => {
    return api.get<MovementDefinition | null>(`/movements/search?query=${encodeURIComponent(query)}`);
  },
};
