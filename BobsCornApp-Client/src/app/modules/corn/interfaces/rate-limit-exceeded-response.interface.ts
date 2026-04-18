export interface RateLimitExceededResponse {
  message: string;
  retryAfterSeconds: number;
}
