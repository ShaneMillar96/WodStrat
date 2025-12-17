/**
 * Movement category enum values matching backend MovementCategory.cs
 */
export type MovementCategory = 'Weightlifting' | 'Gymnastics' | 'Cardio' | 'Strongman';

/**
 * Movement definition as returned from the API
 * Represents a canonical movement in the dictionary
 */
export interface MovementDefinition {
  /** Unique identifier */
  id: number;
  /** Internal identifier (e.g., "toes_to_bar", "pull_up") */
  canonicalName: string;
  /** Human-readable name (e.g., "Toes-to-Bar", "Pull-Up") */
  displayName: string;
  /** Movement category */
  category: MovementCategory;
  /** Optional description of the movement */
  description: string | null;
  /** List of known aliases (e.g., ["T2B", "toes-to-bar", "ttb"]) */
  aliases: string[];
}

/**
 * Movement search/lookup result
 * May be null if no match found
 */
export type MovementLookupResult = MovementDefinition | null;

/**
 * Display labels for movement category options
 */
export const MOVEMENT_CATEGORY_LABELS: Record<MovementCategory, string> = {
  Weightlifting: 'Weightlifting',
  Gymnastics: 'Gymnastics',
  Cardio: 'Cardio',
  Strongman: 'Strongman',
};

/**
 * Badge color mapping for movement categories
 */
export const MOVEMENT_CATEGORY_COLORS: Record<MovementCategory, string> = {
  Weightlifting: 'red',
  Gymnastics: 'green',
  Cardio: 'blue',
  Strongman: 'purple',
};

/**
 * All movement categories for filtering
 */
export const ALL_MOVEMENT_CATEGORIES: MovementCategory[] = [
  'Weightlifting',
  'Gymnastics',
  'Cardio',
  'Strongman',
];
