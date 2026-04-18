import { CommonModule, DatePipe } from "@angular/common";
import { Component, OnDestroy, inject } from "@angular/core";
import { ClientIdentityService } from "../../../shared/services/client-identity.service";
import { CornApiService } from "../../../shared/services/corn-api.service";

@Component({
  selector: "app-corn-page",
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: "./corn-page.component.html",
})
export class CornPageComponent implements OnDestroy {
  private readonly clientIdentityService = inject(ClientIdentityService);
  private readonly cornApiService = inject(CornApiService);
  private cooldownTimer: number | null = null;

  protected readonly clientId = this.clientIdentityService.getClientId();
  protected purchasedCount = 0;
  protected isLoading = false;
  protected message = "Bob is ready to sell you one fresh corn.";
  protected retryAfterSeconds: number | null = null;
  protected latestPurchaseAt: string | null = null;
  protected isUnavailableError = false;

  protected get purchaseButtonLabel(): string {
    return this.isLoading ? "Buying..." : "Buy 1 corn";
  }

  protected buyCorn(): void {
    if (this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.isUnavailableError = false;
    this.message = "Asking Bob for your corn...";

    this.cornApiService.buyCorn().subscribe({
      next: (response) => {
        this.purchasedCount += 1;
        this.latestPurchaseAt = response.purchasedAtUtc;
        this.retryAfterSeconds = null;
        this.message = response.message;
        this.isUnavailableError = false;
        this.stopCooldown();
        this.isLoading = false;
      },
      error: (error: unknown) => {
        if (this.cornApiService.isRateLimitError(error)) {
          this.message = error.error.message;
          this.isUnavailableError = false;
          this.startCooldown(error.error.retryAfterSeconds);
        } else {
          this.message = "The corn stand is unavailable right now.";
          this.retryAfterSeconds = null;
          this.isUnavailableError = true;
          this.stopCooldown();
        }

        this.isLoading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.stopCooldown();
  }

  private startCooldown(seconds: number): void {
    this.stopCooldown();
    this.retryAfterSeconds = seconds;

    this.cooldownTimer = window.setInterval(() => {
      if (this.retryAfterSeconds === null || this.retryAfterSeconds <= 1) {
        this.retryAfterSeconds = null;
        this.stopCooldown();
        return;
      }

      this.retryAfterSeconds -= 1;
    }, 1000);
  }

  private stopCooldown(): void {
    if (this.cooldownTimer !== null) {
      window.clearInterval(this.cooldownTimer);
      this.cooldownTimer = null;
    }
  }
}
