import { api, setAuthToken, clearAuthToken } from './api';
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types';

/**
 * Authentication API service
 * Provides methods for login, register, and logout operations
 */
export const authService = {
  /**
   * Register a new user
   * @param data - Registration data (email, password, confirmPassword)
   * @returns Auth response with JWT token
   */
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.postNoAuth<AuthResponse, RegisterRequest>(
      '/auth/register',
      data
    );
    setAuthToken(response.token);
    return response;
  },

  /**
   * Login with email and password
   * @param data - Login credentials
   * @returns Auth response with JWT token
   */
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.postNoAuth<AuthResponse, LoginRequest>(
      '/auth/login',
      data
    );
    setAuthToken(response.token);
    return response;
  },

  /**
   * Logout the current user
   * Clears the stored token
   */
  logout: (): void => {
    clearAuthToken();
  },
};
