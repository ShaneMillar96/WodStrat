export type {
  User,
  JwtPayload,
  AuthState,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  LoginFormData,
  RegisterFormData,
} from './auth';

export type {
  Gender,
  ExperienceLevel,
  AthleteGoal,
  Athlete,
  AthleteFormData,
  CreateAthleteRequest,
  UpdateAthleteRequest,
} from './athlete';

export {
  GENDER_LABELS,
  EXPERIENCE_LEVEL_LABELS,
  EXPERIENCE_LEVEL_DESCRIPTIONS,
  ATHLETE_GOAL_LABELS,
} from './athlete';

export type {
  ApiError,
  ValidationErrors,
  PaginatedResponse,
} from './api';

export type {
  BenchmarkCategory,
  BenchmarkMetricType,
  BenchmarkDefinition,
  AthleteBenchmark,
  BenchmarkSummary,
  CreateBenchmarkRequest,
  UpdateBenchmarkRequest,
  BenchmarkFormData,
  BenchmarkRowData,
} from './benchmark';

export {
  BENCHMARK_CATEGORY_LABELS,
  BENCHMARK_METRIC_LABELS,
  BENCHMARK_CATEGORY_COLORS,
  ALL_BENCHMARK_CATEGORIES,
  TOTAL_BENCHMARK_COUNT,
  MINIMUM_BENCHMARK_REQUIREMENT,
} from './benchmark';

// Movement types (dedicated file)
export type {
  MovementCategory,
  MovementDefinition,
  MovementLookupResult,
} from './movement';

export {
  MOVEMENT_CATEGORY_LABELS,
  MOVEMENT_CATEGORY_COLORS,
  ALL_MOVEMENT_CATEGORIES,
} from './movement';

// Workout types
export type {
  WorkoutType,
  LoadUnit,
  DistanceUnit,
  ParsingError,
  ParsedMovement,
  ParsedWorkout,
  WorkoutMovement,
  Workout,
  ParseWorkoutRequest,
  CreateWorkoutMovementRequest,
  CreateWorkoutRequest,
  UpdateWorkoutRequest,
  WorkoutInputFormData,
  WorkoutEditFormData,
} from './workout';

export {
  WORKOUT_TYPE_LABELS,
  WORKOUT_TYPE_COLORS,
  ALL_WORKOUT_TYPES,
} from './workout';

// Parsing error types
export type {
  ParsingIssueSeverity,
  ParsingIssueCode,
  ParsingIssue,
  ParseWorkoutResponse,
  LineError,
  SuggestionAction,
} from './parsingError';

export {
  SEVERITY_LABELS,
  SEVERITY_BADGE_VARIANTS,
  SEVERITY_ALERT_VARIANTS,
} from './parsingError';

// Volume load types
export type {
  LoadClassification,
  MovementVolumeLoad,
  WorkoutVolumeLoadResult,
  CalculateVolumeLoadRequest,
} from './volumeLoad';

export {
  LOAD_CLASSIFICATION_BADGE_VARIANTS,
  LOAD_CLASSIFICATION_LABELS,
  LOAD_CLASSIFICATION_COLORS,
} from './volumeLoad';
