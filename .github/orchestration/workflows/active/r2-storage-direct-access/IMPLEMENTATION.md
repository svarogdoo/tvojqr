# Implementation

1. Update storage selection logic so R2 is used whenever its configuration is complete, unless local storage is explicitly requested.
2. Keep local upload serving intact for development.
3. Keep database asset records as object keys and build direct public R2 URLs in API responses.
4. Run backend and frontend verification.
