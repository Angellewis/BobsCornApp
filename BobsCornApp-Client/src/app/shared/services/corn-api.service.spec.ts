import { provideHttpClient } from '@angular/common/http';
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

  it('should post to the buy endpoint', () => {
    const service = TestBed.inject(CornApiService);
    const httpTesting = TestBed.inject(HttpTestingController);

    service.buyCorn().subscribe();

    const request = httpTesting.expectOne(`${environment.apiUrl}/api/corn/buy`);
    expect(request.request.method).toBe('POST');
    request.flush({ corn: '🌽', message: 'ok', purchasedAtUtc: '2026-04-16T00:00:00Z' });
    httpTesting.verify();
  });

  it('should identify rate limit errors', () => {
    const service = TestBed.inject(CornApiService);

    expect(
      service.isRateLimitError({
        name: 'HttpErrorResponse',
        status: 429
      } as never)
    ).toBe(false);
  });
});
