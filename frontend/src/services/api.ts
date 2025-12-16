import type { ApiError, ValidationErrors } from '../types';

const API_BASE = '/api';
const TOKEN_STORAGE_KEY = 'wodstrat_auth_token';

/**
 * Get stored auth token
 */
export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

/**
 * Store auth token
 */
export function setAuthToken(token: string): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

/**
 * Clear stored auth token
 */
export function clearAuthToken(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
}

/**
 * Custom exception class for API errors
 * Preserves structured error information from the backend
 */
export class ApiException extends Error {
  public readonly status: number;
  public readonly title: string;
  public readonly detail?: string;
  public readonly errors?: ValidationErrors;

  constructor(error: ApiError) {
    super(error.detail || error.title);
    this.name = 'ApiException';
    this.status = error.status;
    this.title = error.title;
    this.detail = error.detail;
    this.errors = error.errors;
  }

  /**
   * Check if this is a validation error (400 Bad Request)
   */
  isValidationError(): boolean {
    return this.status === 400 && !!this.errors;
  }

  /**
   * Check if this is a not found error (404)
   */
  isNotFound(): boolean {
    return this.status === 404;
  }

  /**
   * Get a user-friendly error message
   */
  getUserMessage(): string {
    if (this.isValidationError() && this.errors) {
      const messages = Object.values(this.errors).flat();
      return messages.length > 0 ? messages[0] : this.title;
    }
    return this.detail || this.title;
  }
}

/**
 * Parse error response from API
 */
async function parseErrorResponse(response: Response): Promise<ApiError> {
  try {
    const data = await response.json();
    return {
      type: data.type,
      title: data.title || response.statusText,
      status: response.status,
      detail: data.detail,
      errors: data.errors,
      traceId: data.traceId,
    };
  } catch {
    return {
      title: response.statusText || 'An error occurred',
      status: response.status,
    };
  }
}

/**
 * Generic fetch wrapper with error handling and JWT injection
 * @param endpoint - API endpoint to call
 * @param options - Fetch options
 * @param skipAuth - If true, do not add Authorization header (for login/register)
 */
async function request<T>(
  endpoint: string,
  options: RequestInit = {},
  skipAuth: boolean = false
): Promise<T> {
  const url = `${API_BASE}${endpoint}`;
  const token = getAuthToken();

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  // Add Authorization header if token exists and not skipping auth
  if (token && !skipAuth) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const config: RequestInit = {
    ...options,
    headers,
  };

  const response = await fetch(url, config);

  if (!response.ok) {
    // Handle 401 Unauthorized - clear token and redirect
    if (response.status === 401 && !skipAuth) {
      clearAuthToken();
      window.location.href = '/login';
      throw new ApiException({
        title: 'Session expired',
        status: 401,
        detail: 'Please log in again',
      });
    }

    const error = await parseErrorResponse(response);
    throw new ApiException(error);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

/**
 * API client with typed methods
 */
export const api = {
  /**
   * GET request
   */
  get: <T>(endpoint: string): Promise<T> => {
    return request<T>(endpoint, { method: 'GET' });
  },

  /**
   * POST request
   */
  post: <T, D = unknown>(endpoint: string, data: D): Promise<T> => {
    return request<T>(endpoint, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  /**
   * PUT request
   */
  put: <T, D = unknown>(endpoint: string, data: D): Promise<T> => {
    return request<T>(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  /**
   * DELETE request
   */
  delete: <T = void>(endpoint: string): Promise<T> => {
    return request<T>(endpoint, { method: 'DELETE' });
  },

  /**
   * POST request without auth header (for login/register)
   */
  postNoAuth: <T, D = unknown>(endpoint: string, data: D): Promise<T> => {
    return request<T>(
      endpoint,
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
      true
    );
  },
};
