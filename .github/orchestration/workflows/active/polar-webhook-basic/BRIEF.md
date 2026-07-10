# Brief

Goal:
- Receive Polar Raw webhooks on the backend.
- Verify webhook signatures before processing events.
- Activate or update user entitlements from trusted Polar events.

Success criteria:
- Invalid webhook signatures are rejected.
- Valid payment/subscription events can upsert `user_entitlements`.
- Backend tests cover the basic webhook path.
