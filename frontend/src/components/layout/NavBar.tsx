import React from 'react';
import { useAthleteContext } from '../../contexts';
import { NavLink } from './NavLink';

export const NavBar: React.FC = () => {
  const { athleteId, hasAthlete } = useAthleteContext();

  return (
    <nav className="bg-white border-b border-gray-200 shadow-sm">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <div className="flex items-center">
            <span className="text-xl font-bold text-primary-600">WodStrat</span>
          </div>

          <div className="flex items-center space-x-1">
            <NavLink to={hasAthlete ? `/profile/${athleteId}` : '/profile/new'}>
              Profile
            </NavLink>
            <NavLink
              to={hasAthlete ? `/athletes/${athleteId}/benchmarks` : '#'}
              disabled={!hasAthlete}
            >
              Benchmarks
            </NavLink>
            <NavLink to="/strategy">Strategy</NavLink>
          </div>
        </div>
      </div>
    </nav>
  );
};
