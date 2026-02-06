import { useState, useCallback } from 'react';
import type { StrategyTabId } from '../types/strategyPage';
import { STRATEGY_TABS } from '../types/strategyPage';
import type { TabOption } from '../components/ui/Tabs';

export interface UseStrategyTabsResult {
  /** Currently active tab */
  activeTab: StrategyTabId;
  /** Function to set the active tab */
  setActiveTab: (tab: StrategyTabId) => void;
  /** Tab options for the Tabs component */
  tabs: TabOption<StrategyTabId>[];
}

/**
 * useStrategyTabs - Hook to manage strategy page tab state
 *
 * Features:
 * - Manages active tab state
 * - Provides tab configuration for Tabs component
 * - Memoized setActiveTab callback
 *
 * @param defaultTab - Default active tab (defaults to 'overview')
 */
export function useStrategyTabs(
  defaultTab: StrategyTabId = 'overview'
): UseStrategyTabsResult {
  const [activeTab, setActiveTabState] = useState<StrategyTabId>(defaultTab);

  const setActiveTab = useCallback((tab: StrategyTabId) => {
    setActiveTabState(tab);
  }, []);

  // Convert STRATEGY_TABS to TabOption format
  const tabs: TabOption<StrategyTabId>[] = STRATEGY_TABS.map((tab) => ({
    value: tab.id,
    label: tab.label,
  }));

  return {
    activeTab,
    setActiveTab,
    tabs,
  };
}
