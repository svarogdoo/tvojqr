# Implementation

1. Added `Polar__WebhookSecret` to backend Polar options.
2. Added Standard Webhooks signature verification in the billing webhook endpoint.
3. Added `POST /api/billing/polar/webhook`.
4. Added entitlement upsert support to the entitlement repository.
5. Added endpoint tests for invalid signatures and valid paid-order activation.
6. Updated backend and root README docs.

Verification:
- `dotnet build backend/HostingQr.Backend.sln` passed.
- `dotnet test backend/HostingQr.Backend.sln` passed.
