import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useAuthContext } from '../contexts';
import { authService, ApiException } from '../services';
import type { LoginRequest, RegisterRequest } from '../types';

/**
 * Hook for authentication operations
 * Provides login, register, and logout functionality with loading/error states
 */
export function useAuth() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { login: setAuthToken, logout: clearAuth, isAuthenticated, user, isLoading: isInitializing } = useAuthContext();

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<ApiException | null>(null);

  const login = useCallback(async (data: LoginRequest) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await authService.login(data);
      setAuthToken(response.token);
      navigate('/profile');
    } catch (err) {
      const apiError = err instanceof ApiException
        ? err
        : new ApiException({ title: 'Login failed', status: 500 });
      setError(apiError);
      throw apiError;
    } finally {
      setIsLoading(false);
    }
  }, [setAuthToken, navigate]);

  const register = useCallback(async (data: RegisterRequest) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await authService.register(data);
      setAuthToken(response.token);
      navigate('/profile/new');
    } catch (err) {
      const apiError = err instanceof ApiException
        ? err
        : new ApiException({ title: 'Registration failed', status: 500 });
      setError(apiError);
      throw apiError;
    } finally {
      setIsLoading(false);
    }
  }, [setAuthToken, navigate]);

  const logout = useCallback(() => {
    authService.logout();
    clearAuth();
    queryClient.clear(); // Clear all cached data
    navigate('/login');
  }, [clearAuth, queryClient, navigate]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    // State
    isAuthenticated,
    isLoading,
    isInitializing,
    user,
    error,

    // Actions
    login,
    register,
    logout,
    clearError,
  };
}
