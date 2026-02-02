/**
 * Workout type badge styles with Tailwind classes
 */
export const WORKOUT_TYPE_STYLES: Record<string, string> = {
  Amrap: 'bg-blue-100 text-blue-800 border-blue-300',
  ForTime: 'bg-green-100 text-green-800 border-green-300',
  Emom: 'bg-purple-100 text-purple-800 border-purple-300',
  Rounds: 'bg-orange-100 text-orange-800 border-orange-300',
  Intervals: 'bg-pink-100 text-pink-800 border-pink-300',
};

/**
 * Time domain badge styles with Tailwind classes
 */
export const TIME_DOMAIN_STYLES: Record<string, string> = {
  short: 'bg-yellow-100 text-yellow-800',
  medium: 'bg-blue-100 text-blue-800',
  long: 'bg-gray-100 text-gray-800',
};

/**
 * Time domain labels
 */
export const TIME_DOMAIN_LABELS: Record<string, string> = {
  short: 'Short',
  medium: 'Medium',
  long: 'Long',
};

/**
 * Confidence level styles
 */
export const CONFIDENCE_STYLES = {
  high: { color: 'text-green-600', bg: 'bg-green-500', barColor: 'bg-green-500' },
  medium: { color: 'text-yellow-600', bg: 'bg-yellow-500', barColor: 'bg-yellow-500' },
  low: { color: 'text-red-600', bg: 'bg-red-500', barColor: 'bg-red-500' },
};

/**
 * Movement category icons (emoji)
 */
export const CATEGORY_ICONS: Record<string, string> = {
  Weightlifting: '\u{1F3CB}',  // Weight lifter emoji
  Gymnastics: '\u{1F938}',     // Person doing cartwheel emoji
  Cardio: '\u{1F3C3}',         // Runner emoji
  Strongman: '\u{1F4AA}',      // Flexed bicep emoji
};

/**
 * Movement category icon colors (for styled icons)
 */
export const CATEGORY_ICON_COLORS: Record<string, string> = {
  Weightlifting: 'text-red-500',
  Gymnastics: 'text-green-500',
  Cardio: 'text-blue-500',
  Strongman: 'text-purple-500',
};

/**
 * Movement category background colors (for icon backgrounds)
 */
export const CATEGORY_BG_COLORS: Record<string, string> = {
  Weightlifting: 'bg-red-100',
  Gymnastics: 'bg-green-100',
  Cardio: 'bg-blue-100',
  Strongman: 'bg-purple-100',
};

/**
 * Rep scheme type colors
 */
export const REP_SCHEME_COLORS: Record<string, string> = {
  descending: 'text-orange-600',
  ascending: 'text-green-600',
  fixed: 'text-blue-600',
  custom: 'text-gray-600',
};

/**
 * Size-based styling for components
 */
export const SIZE_STYLES = {
  sm: {
    text: 'text-xs',
    padding: 'px-1.5 py-0.5',
    icon: 'text-sm',
    gap: 'gap-1',
  },
  md: {
    text: 'text-sm',
    padding: 'px-2 py-1',
    icon: 'text-base',
    gap: 'gap-1.5',
  },
  lg: {
    text: 'text-base',
    padding: 'px-2.5 py-1.5',
    icon: 'text-lg',
    gap: 'gap-2',
  },
};
