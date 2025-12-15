import React from 'react';
import { NavLink as RouterNavLink } from 'react-router-dom';

interface NavLinkProps {
  to: string;
  children: React.ReactNode;
  disabled?: boolean;
}

export const NavLink: React.FC<NavLinkProps> = ({
  to,
  children,
  disabled = false,
}) => {
  const baseStyles =
    'px-3 py-2 text-sm font-medium rounded-md transition-colors';
  const activeStyles = 'bg-primary-100 text-primary-700';
  const inactiveStyles = 'text-gray-600 hover:bg-gray-100 hover:text-gray-900';
  const disabledStyles = 'text-gray-400 cursor-not-allowed';

  if (disabled) {
    return (
      <span className={`${baseStyles} ${disabledStyles}`}>{children}</span>
    );
  }

  return (
    <RouterNavLink
      to={to}
      className={({ isActive }) =>
        `${baseStyles} ${isActive ? activeStyles : inactiveStyles}`
      }
    >
      {children}
    </RouterNavLink>
  );
};
