import React from 'react';
import { useAuthContext, useAthleteContext } from '../../contexts';
import { useAuth } from '../../hooks';
import { NavLink } from './NavLink';
import { Button } from '../ui';

export const NavBar: React.FC = () => {
  const { isAuthenticated, user } = useAuthContext();
  const { hasAthlete } = useAthleteContext();
  const { logout } = useAuth();

  return (
    <nav className="bg-white border-b border-gray-200 shadow-sm">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <div className="flex items-center">
            <span className="text-xl font-bold text-primary-600">WodStrat</span>
          </div>

          <div className="flex items-center space-x-1">
            {isAuthenticated ? (
              <>
                <NavLink to={hasAthlete ? '/profile' : '/profile/new'}>
                  Profile
                </NavLink>
                <NavLink
                  to="/benchmarks"
                  disabled={!hasAthlete}
                >
                  Benchmarks
                </NavLink>
                <NavLink to="/strategy">Strategy</NavLink>
                <div className="ml-4 flex items-center border-l border-gray-200 pl-4">
                  {user?.email && (
                    <span className="mr-3 hidden text-sm text-gray-600 sm:block">
                      {user.email}
                    </span>
                  )}
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={logout}
                  >
                    Logout
                  </Button>
                </div>
              </>
            ) : (
              <>
                <NavLink to="/login">Login</NavLink>
                <NavLink to="/register">Register</NavLink>
              </>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};
