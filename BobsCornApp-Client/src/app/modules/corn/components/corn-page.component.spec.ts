import { of } from "rxjs";
import { TestBed } from "@angular/core/testing";
import { CornPageComponent } from "./corn-page.component";
import { ClientIdentityService } from "../../../shared/services/client-identity.service";
import { CornApiService } from "../../../shared/services/corn-api.service";

describe("CornPageComponent", () => {
  it("should render the purchase message", async () => {
    await TestBed.configureTestingModule({
      imports: [CornPageComponent],
    })
      .overrideProvider(ClientIdentityService, {
        useValue: {
          getClientId: () => "client-abc",
        },
      })
      .overrideProvider(CornApiService, {
        useValue: {
          buyCorn: jest.fn().mockReturnValue(
            of({
              corn: "🌽",
              message: "Corn purchased successfully.",
              purchasedAtUtc: "2026-04-16T00:00:00Z",
            }),
          ),
          isRateLimitError: jest.fn().mockReturnValue(false),
        },
      })
      .compileComponents();

    const fixture = TestBed.createComponent(CornPageComponent);
    fixture.componentInstance["message"] = "Corn purchased successfully.";
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      "Corn purchased successfully.",
    );
  });
});
