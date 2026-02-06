import React from 'react';
import type { StrategyTabId } from '../../types/strategyPage';
import { useStrategyTabs } from '../../hooks/useStrategyTabs';
import { Tabs } from '../ui';

export interface StrategyTabContainerProps {
  /** Children render prop receiving active tab */
  children: (activeTab: StrategyTabId) => React.ReactNode;
  /** Default active tab */
  defaultTab?: StrategyTabId;
  /** Additional CSS classes */
  className?: string;
}

/**
 * StrategyTabContainer - Tab navigation container for strategy page
 *
 * Features:
 * - Manages active tab state using useStrategyTabs hook
 * - Renders tab navigation using Tabs component
 * - Uses render prop pattern for flexible content rendering
 */
export const StrategyTabContainer: React.FC<StrategyTabContainerProps> = ({
  children,
  defaultTab = 'overview',
  className = '',
}) => {
  const { activeTab, setActiveTab, tabs } = useStrategyTabs(defaultTab);

  return (
    <div className={className}>
      {/* Tab Navigation */}
      <Tabs
        options={tabs}
        value={activeTab}
        onChange={setActiveTab}
        variant="underline"
        className="mb-4"
      />

      {/* Tab Content */}
      <div role="tabpanel" aria-labelledby={`tab-${activeTab}`}>
        {children(activeTab)}
      </div>
    </div>
  );
};
