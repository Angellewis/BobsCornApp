import { BaseResponse } from "./api-error-response.interface";

export interface CornPurchaseResponse extends BaseResponse {
  corn: string;
  purchasedAtUtc: string;
}
