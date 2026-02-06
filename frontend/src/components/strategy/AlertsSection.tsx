import React from 'react';
import type { RiskAlert, AlertSeverity } from '../../types/strategyInsights';
import { ALERT_SEVERITY_COLORS } from '../../types/strategyInsights';
import { SectionSkeleton, SeverityBadge } from '../ui';

export interface AlertsSectionProps {
  /** Risk alerts */
  alerts?: RiskAlert[];
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * AlertsSection - Displays risk alerts and warnings
 */
export const AlertsSection: React.FC<AlertsSectionProps> = ({
  alerts = [],
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/4" lines={3} className={className} />;
  }

  if (alerts.length === 0) {
    return null;
  }

  // Sort by severity (High > Medium > Low)
  const severityOrder: Record<AlertSeverity, number> = { High: 0, Medium: 1, Low: 2 };
  const sortedAlerts = [...alerts].sort(
    (a, b) => severityOrder[a.severity] - severityOrder[b.severity]
  );

  return (
    <div className={`space-y-3 ${className}`}>
      <div className="flex items-center gap-2">
        <span className="text-xl" role="img" aria-label="Warning">&#x26A0;&#xFE0F;</span>
        <h3 className="font-semibold text-gray-900">Alerts</h3>
      </div>

      {sortedAlerts.map((alert, index) => {
        const colors = ALERT_SEVERITY_COLORS[alert.severity];
        return (
          <div
            key={`${alert.alertType}-${index}`}
            className={`rounded-lg border p-4 ${colors.bg} ${colors.border}`}
          >
            <div className="flex items-start gap-3">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <h4 className={`font-medium ${colors.text}`}>{alert.title}</h4>
                  <SeverityBadge severity={alert.severity} size="sm" />
                </div>
                <p className={`text-sm ${colors.text}`}>{alert.message}</p>
                {alert.suggestedAction && (
                  <p className="text-sm font-medium mt-2 text-gray-800">
                    Suggestion: {alert.suggestedAction}
                  </p>
                )}
                {alert.affectedMovements.length > 0 && (
                  <p className="text-xs text-gray-500 mt-2">
                    Affects: {alert.affectedMovements.join(', ')}
                  </p>
                )}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
};
