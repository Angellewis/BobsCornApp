import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ClientIdentityService {
  private readonly storageKey = 'bobs-corn-client-id';

  getClientId(): string {
    const existingClientId = localStorage.getItem(this.storageKey);
    if (existingClientId) {
      return existingClientId;
    }

    const generatedClientId = crypto.randomUUID();
    localStorage.setItem(this.storageKey, generatedClientId);
    return generatedClientId;
  }
}
