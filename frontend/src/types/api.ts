/**
 * API error response matching ASP.NET Core ProblemDetails format
 */
export interface ApiError {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: ValidationErrors;
  traceId?: string;
}

/**
 * Validation errors object from API
 * Keys are field names (in PascalCase from backend)
 * Values are arrays of error messages
 */
export interface ValidationErrors {
  [field: string]: string[];
}

/**
 * Generic API response wrapper for paginated results
 */
export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
