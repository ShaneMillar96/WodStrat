/**
 * Severity level for parsing issues
 */
export type ParsingIssueSeverity = 'error' | 'warning' | 'info';

/**
 * Error/warning codes for parsing issues
 */
export type ParsingIssueCode =
  | 'NO_MOVEMENTS_DETECTED'
  | 'INVALID_WORKOUT_FORMAT'
  | 'UNKNOWN_MOVEMENT'
  | 'AMBIGUOUS_MOVEMENT'
  | 'MISSING_REP_COUNT'
  | 'INVALID_REP_COUNT'
  | 'INVALID_LOAD_FORMAT'
  | 'INVALID_DISTANCE_FORMAT'
  | 'INVALID_TIME_FORMAT'
  | 'DUPLICATE_MOVEMENT'
  | 'EMPTY_LINE_IGNORED'
  | 'TYPO_DETECTED'
  | string; // Allow for additional codes

/**
 * A parsing issue (error or warning) returned from the API
 */
export interface ParsingIssue {
  /** Unique code identifying the type of issue */
  code: ParsingIssueCode;
  /** Human-readable description of the issue */
  message: string;
  /** Suggested fix or alternative */
  suggestion: string | null;
  /** Line number in the input (1-indexed), null if not line-specific */
  line: number | null;
  /** Severity level of the issue */
  severity: ParsingIssueSeverity;
}

/**
 * Enhanced parsing response from the API
 */
export interface ParseWorkoutResponse {
  /** Whether parsing completed successfully (no blocking errors) */
  success: boolean;
  /** Blocking errors that prevent workout creation */
  errors: ParsingIssue[];
  /** Non-blocking warnings */
  warnings: ParsingIssue[];
  /** The parsed workout data (may be partial if there are warnings) */
  parsedWorkout: import('./workout').ParsedWorkout | null;
  /** Confidence score for the parsing (0-1) */
  parseConfidence: number;
}

/**
 * Line error information for highlighting
 */
export interface LineError {
  /** Line number (1-indexed) */
  lineNumber: number;
  /** All issues on this line */
  issues: ParsingIssue[];
  /** Highest severity issue on this line */
  highestSeverity: ParsingIssueSeverity;
}

/**
 * Suggestion action for "Try to fix" functionality
 */
export interface SuggestionAction {
  /** The issue this suggestion addresses */
  issue: ParsingIssue;
  /** The suggested replacement text */
  replacement: string;
  /** Line number to apply the fix (null for global suggestions) */
  lineNumber: number | null;
}

/**
 * Display labels for severity levels
 */
export const SEVERITY_LABELS: Record<ParsingIssueSeverity, string> = {
  error: 'Error',
  warning: 'Warning',
  info: 'Info',
};

/**
 * Badge variants for severity levels
 */
export const SEVERITY_BADGE_VARIANTS: Record<ParsingIssueSeverity, 'error' | 'warning' | 'blue'> = {
  error: 'error',
  warning: 'warning',
  info: 'blue',
};

/**
 * Alert variants for severity levels
 */
export const SEVERITY_ALERT_VARIANTS: Record<ParsingIssueSeverity, 'error' | 'warning' | 'info'> = {
  error: 'error',
  warning: 'warning',
  info: 'info',
};
