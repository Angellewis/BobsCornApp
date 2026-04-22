import { BaseResponse } from "./api-error-response.interface";

export interface RateLimitExceededResponse extends BaseResponse {
  retryAfterSeconds: number;
}
