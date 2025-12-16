/**
 * User information decoded from JWT token
 */
export interface User {
  id: number;
  email: string;
  athleteId: number | null;
}

/**
 * JWT token payload structure
 */
export interface JwtPayload {
  sub: string;           // User ID
  email: string;         // User email
  athleteId?: string;    // Associated athlete ID (optional)
  exp: number;           // Expiration timestamp
  iat: number;           // Issued at timestamp
}

/**
 * Authentication state
 */
export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Register request payload
 */
export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

/**
 * Auth response from API (login/register)
 */
export interface AuthResponse {
  token: string;
  expiresAt: string;
}

/**
 * Form data for login form
 */
export interface LoginFormData {
  email: string;
  password: string;
}

/**
 * Form data for register form
 */
export interface RegisterFormData {
  email: string;
  password: string;
  confirmPassword: string;
}
