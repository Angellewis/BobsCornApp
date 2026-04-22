import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CornApiService } from './corn-api.service';

describe('CornApiService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), CornApiService]
    });
  });

  it('should post to the buy endpoint with the client id query string', () => {
    const service = TestBed.inject(CornApiService);
    const httpTesting = TestBed.inject(HttpTestingController);

    service.buyCorn('client-abc').subscribe();

    const request = httpTesting.expectOne(`${environment.apiUrl}/api/corn/buy?clientId=client-abc`);
    expect(request.request.method).toBe('POST');
    request.flush({ corn: 'corn', message: 'ok', purchasedAtUtc: '2026-04-16T00:00:00Z' });
    httpTesting.verify();
  });

  it('should identify rate limit errors', () => {
    const service = TestBed.inject(CornApiService);
    const error = new HttpErrorResponse({
      status: 429,
      error: { message: 'Rate limit exceeded.', retryAfterSeconds: 3 }
    });

    expect(service.isRateLimitError(error)).toBe(true);
  });

  it('should identify bad request errors', () => {
    const service = TestBed.inject(CornApiService);
    const error = new HttpErrorResponse({
      status: 400,
      error: { message: 'The clientId query parameter is required.' }
    });

    expect(service.isBadRequestError(error)).toBe(true);
  });
});
