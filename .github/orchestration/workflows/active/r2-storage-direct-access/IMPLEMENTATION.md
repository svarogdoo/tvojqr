# Implementation

1. Update storage selection logic so R2 is used whenever its configuration is complete, unless local storage is explicitly requested.
2. Keep local upload serving intact for development.
3. Run `npm run check` and `npm run build` in `frontend/`.
