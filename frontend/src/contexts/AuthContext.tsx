import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';
import { getAuthToken, clearAuthToken } from '../services';
import type { User, JwtPayload, AuthState } from '../types';

interface AuthContextValue extends AuthState {
  login: (token: string) => void;
  logout: () => void;
  updateAthleteId: (athleteId: number) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

/**
 * Decode JWT token and extract user information
 */
function decodeToken(token: string): User | null {
  try {
    const payload = jwtDecode<JwtPayload>(token);

    // Check if token is expired
    if (payload.exp * 1000 < Date.now()) {
      return null;
    }

    return {
      id: parseInt(payload.sub, 10),
      email: payload.email,
      athleteId: payload.athleteId ? parseInt(payload.athleteId, 10) : null,
    };
  } catch {
    return null;
  }
}

/**
 * Initialize auth state from stored token
 */
function initializeAuthState(): AuthState {
  const token = getAuthToken();

  if (!token) {
    return {
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    };
  }

  const user = decodeToken(token);

  if (!user) {
    // Token is invalid or expired, clear it
    clearAuthToken();
    return {
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    };
  }

  return {
    user,
    token,
    isAuthenticated: true,
    isLoading: false,
  };
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [authState, setAuthState] = useState<AuthState>(() => ({
    ...initializeAuthState(),
    isLoading: true,
  }));

  // Initialize auth state on mount
  useEffect(() => {
    const state = initializeAuthState();
    setAuthState({ ...state, isLoading: false });
  }, []);

  const login = useCallback((token: string) => {
    const user = decodeToken(token);

    if (user) {
      setAuthState({
        user,
        token,
        isAuthenticated: true,
        isLoading: false,
      });
    }
  }, []);

  const logout = useCallback(() => {
    clearAuthToken();
    setAuthState({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    });
  }, []);

  const updateAthleteId = useCallback((athleteId: number) => {
    setAuthState((prev) => ({
      ...prev,
      user: prev.user ? { ...prev.user, athleteId } : null,
    }));
  }, []);

  return (
    <AuthContext.Provider
      value={{
        ...authState,
        login,
        logout,
        updateAthleteId,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuthContext = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuthContext must be used within AuthProvider');
  }
  return context;
};
