import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { ClientIdentityService } from "../shared/services/client-identity.service";

export const clientIdInterceptor: HttpInterceptorFn = (request, next) => {
  const clientIdentityService = inject(ClientIdentityService);
  const clientId = clientIdentityService.getClientId();

  return next(
    request.clone({
      setHeaders: {
        "X-Client-Id": clientId,
      },
    }),
  );
};
