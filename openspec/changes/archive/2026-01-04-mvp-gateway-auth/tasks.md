## 1. Specification
- [x] 1.1 Review proposal/design/spec deltas with owner
- [x] 1.2 Lock token format (header + payload + kid) and expiry defaults

## 2. Gateway (Issue Token)
- [x] 2.1 Create `Arcadia.Gateway` HTTP service skeleton
- [x] 2.2 Implement `POST /auth/token` (dev key protected) returning signed token
- [x] 2.3 Implement key set loading (active `kid`, multiple keys)

## 3. Zone (Verify Only)
- [x] 3.1 Update token verification to require `kid` and validate via key set
- [x] 3.2 Remove client-side signing path (keep temporary fallback behind explicit dev flag)
- [x] 3.3 Ensure auth failure disconnect includes short error hint

## 4. Client/LoadTest
- [x] 4.1 Fetch token from Gateway before connecting to Zone
- [x] 4.2 Add negative smoke: invalid token => disconnected with error hint

## 5. Testing & Verification
- [x] 5.1 Unit tests: kid routing, expired, invalid signature
- [x] 5.2 Smoke: `scripts/smoke_enet.sh` upgraded to start Gateway + Zone + LoadTest
