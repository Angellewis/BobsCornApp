import { ClientIdentityService } from './client-identity.service';

describe('ClientIdentityService', () => {
  beforeEach(() => {
    localStorage.clear();
    jest.restoreAllMocks();
  });

  it('should return an existing client id from storage', () => {
    localStorage.setItem('bobs-corn-client-id', 'existing-id');
    const service = new ClientIdentityService();

    expect(service.getClientId()).toBe('existing-id');
  });

  it('should generate and persist a client id when missing', () => {
    const originalCrypto = globalThis.crypto;
    Object.defineProperty(globalThis, 'crypto', {
      value: {
        randomUUID: jest.fn(() => '11111111-1111-1111-1111-111111111111')
      },
      configurable: true
    });

    const service = new ClientIdentityService();
    const clientId = service.getClientId();

    Object.defineProperty(globalThis, 'crypto', {
      value: originalCrypto,
      configurable: true
    });

    expect(clientId).toBe('11111111-1111-1111-1111-111111111111');
  });
});
