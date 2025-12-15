import React, { useEffect } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAthleteContext } from '../../contexts';
import { NavBar } from './NavBar';

export const MainLayout: React.FC = () => {
  const { hasAthlete } = useAthleteContext();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const isNewProfilePage = location.pathname === '/profile/new';
    if (!hasAthlete && !isNewProfilePage) {
      navigate('/profile/new', { replace: true });
    }
  }, [hasAthlete, location.pathname, navigate]);

  return (
    <div className="min-h-screen bg-gray-50">
      <NavBar />
      <main>
        <Outlet />
      </main>
    </div>
  );
};
