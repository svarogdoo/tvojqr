# Brief

User-facing goal: uploaded images should keep working across deploys/restarts and should be served from durable object storage.

Success criteria:
- When R2 is configured, new uploads return direct R2-backed public URLs.
- Local filesystem storage remains available for development.
- Verification passes with frontend/backend checks.
