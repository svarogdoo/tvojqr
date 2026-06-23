# Implementation

1. Added backend `PolarOptions` config and DI binding.
2. Added `POST /api/billing/checkout` with authenticated-user requirement.
3. Mapped tier/cycle to backend-configured Polar product IDs.
4. Created checkout through Polar API and returned checkout URL.
5. Wired pricing buttons to start checkout or require sign-in.
6. Documented backend Polar env vars.
7. Verification pending.
