import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, throwError } from "rxjs";
import { catchError } from "rxjs/operators";
import { environment } from "../../../environments/environment";
import { CornPurchaseResponse } from "../../modules/corn/interfaces/corn-purchase-response.interface";
import { RateLimitExceededResponse } from "../../modules/corn/interfaces/rate-limit-exceeded-response.interface";

@Injectable({ providedIn: "root" })
export class CornApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/corn`;

  constructor(private readonly _httpClient: HttpClient) {}

  buyCorn(): Observable<CornPurchaseResponse> {
    return this._httpClient
      .post<CornPurchaseResponse>(`${this.baseUrl}/buy`, {})
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => error)));
  }

  isRateLimitError(
    error: unknown,
  ): error is HttpErrorResponse & { error: RateLimitExceededResponse } {
    return error instanceof HttpErrorResponse && error.status === 429;
  }
}
