export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  /** Machine-readable reason on 409 responses (e.g. duplicate_pending_request). */
  code?: string;
  errors?: Record<string, string[]>;
}
