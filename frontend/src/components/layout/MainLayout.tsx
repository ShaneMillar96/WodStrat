import React, { useEffect, useRef } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthContext, useAthleteContext } from '../../contexts';
import { NavBar } from './NavBar';

export const MainLayout: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuthContext();
  const { hasAthlete } = useAthleteContext();
  const location = useLocation();
  const navigate = useNavigate();
  const hasRedirected = useRef(false);

  useEffect(() => {
    // Don't redirect while loading auth state
    if (isLoading) return;

    // Only redirect once when auth state is first determined
    if (hasRedirected.current) return;

    const isNewProfilePage = location.pathname === '/profile/new';

    // Redirect to profile/new if authenticated but no athlete profile
    if (isAuthenticated && !hasAthlete && !isNewProfilePage) {
      hasRedirected.current = true;
      navigate('/profile/new', { replace: true });
      return;
    }

    // Redirect to profile if on /profile/new but already has athlete
    if (isAuthenticated && hasAthlete && isNewProfilePage) {
      hasRedirected.current = true;
      navigate('/profile', { replace: true });
      return;
    }

    // Mark as handled even if no redirect was needed
    hasRedirected.current = true;
  }, [isAuthenticated, hasAthlete, isLoading, location.pathname, navigate]);

  // Reset redirect flag when auth state changes (e.g., logout then login)
  useEffect(() => {
    hasRedirected.current = false;
  }, [isAuthenticated, hasAthlete]);

  // Show loading while auth initializes
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-600 border-t-transparent" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <NavBar />
      <main>
        <Outlet />
      </main>
    </div>
  );
};
