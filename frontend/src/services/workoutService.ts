import { api } from './api';
import type {
  Workout,
  ParsedWorkout,
  ParseWorkoutRequest,
  CreateWorkoutRequest,
  UpdateWorkoutRequest,
} from '../types/workout';
import type { ParseWorkoutResponse } from '../types/parsingError';

/**
 * Workout API service
 * Provides methods for workout CRUD operations and parsing
 * Note: Uses session-based identification via JWT token
 */
export const workoutService = {
  /**
   * Parse workout text into structured data (legacy - returns ParsedWorkout directly)
   * @deprecated Use parseWithErrors for enhanced error handling
   * @param text - The raw workout text to parse
   * @returns Parsed workout structure with detected type, movements, and any errors
   */
  parse: async (text: string): Promise<ParsedWorkout> => {
    return api.post<ParsedWorkout, ParseWorkoutRequest>('/workouts/parse', { text });
  },

  /**
   * Parse workout text with enhanced error response
   * Returns detailed errors, warnings, suggestions, and confidence score
   * @param text - The raw workout text to parse
   * @returns Enhanced parsing response with errors, warnings, and parsed workout
   */
  parseWithErrors: async (text: string): Promise<ParseWorkoutResponse> => {
    return api.post<ParseWorkoutResponse, ParseWorkoutRequest>('/workouts/parse', { text });
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
