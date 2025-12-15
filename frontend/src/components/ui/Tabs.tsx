import React from 'react';

export interface TabOption<T extends string = string> {
  /** Unique value for the tab */
  value: T;
  /** Display label */
  label: string;
  /** Optional count badge */
  count?: number;
  /** Whether the tab is disabled */
  disabled?: boolean;
}

export interface TabsProps<T extends string = string> {
  /** Tab options */
  options: TabOption<T>[];
  /** Currently selected value */
  value: T;
  /** Callback when tab changes */
  onChange: (value: T) => void;
  /** Variant style */
  variant?: 'underline' | 'pills';
  /** Additional class names */
  className?: string;
}

/**
 * Tabs component for navigation/filtering
 */
export function Tabs<T extends string = string>({
  options,
  value,
  onChange,
  variant = 'underline',
  className = '',
}: TabsProps<T>): React.ReactElement {
  const baseTabStyles = 'inline-flex items-center gap-2 font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2';

  const variantStyles: Record<string, { container: string; tab: string; active: string; inactive: string }> = {
    underline: {
      container: 'flex border-b border-gray-200',
      tab: 'px-4 py-2 text-sm -mb-px border-b-2',
      active: 'border-primary-600 text-primary-600',
      inactive: 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
    },
    pills: {
      container: 'flex flex-wrap gap-2',
      tab: 'px-4 py-2 text-sm rounded-full',
      active: 'bg-primary-600 text-white',
      inactive: 'bg-gray-100 text-gray-700 hover:bg-gray-200',
    },
  };

  const styles = variantStyles[variant];

  return (
    <nav className={`${styles.container} ${className}`} role="tablist">
      {options.map((option) => {
        const isActive = option.value === value;
        const isDisabled = option.disabled;

        return (
          <button
            key={option.value}
            type="button"
            role="tab"
            aria-selected={isActive}
            aria-disabled={isDisabled}
            tabIndex={isActive ? 0 : -1}
            onClick={() => !isDisabled && onChange(option.value)}
            className={`
              ${baseTabStyles}
              ${styles.tab}
              ${isActive ? styles.active : styles.inactive}
              ${isDisabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer'}
            `}
          >
            {option.label}
            {option.count !== undefined && (
              <span
                className={`
                  inline-flex items-center justify-center rounded-full px-2 py-0.5 text-xs font-medium
                  ${isActive
                    ? variant === 'pills'
                      ? 'bg-primary-500 text-white'
                      : 'bg-primary-100 text-primary-600'
                    : 'bg-gray-200 text-gray-600'
                  }
                `}
              >
                {option.count}
              </span>
            )}
          </button>
        );
      })}
    </nav>
  );
}
