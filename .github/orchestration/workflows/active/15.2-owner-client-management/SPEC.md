# SPEC

## Scope

Add owner-only client/menu administration, secure project invitations and transfer, manual account billing profiles, private invoice documents, and client account billing details.

## Affected Areas

- PostgreSQL migrations and repositories
- Admin, project, authentication, billing, invitation, invoice, and storage APIs
- Project authorization and deletion safety
- Admin overview, project editor, invitation, and account pages
- Backend tests and environment documentation

## Technical Approach

- Continue using the active `admin` entitlement as the global owner authorization gate.
- Allow project operations when the current user owns the project or has active admin access.
- Store hashed, expiring, single-use invitation tokens and require an email match on acceptance.
- Store one manual billing profile per client and invoice metadata separately.
- Store invoice PDFs in private storage and stream them only after authorization.
- Transfer the project owner in a transaction without changing dependent menu records.

## Assumptions

- The user's existing account already has the admin entitlement.
- Billing applies to the client account, not individual menus.
- Invoice uploads are PDF-only for the first version.
- The existing Polar backend can remain dormant while its client-facing portal UI is removed.

## Risks

- Existing owner-scoped repositories need careful updates to avoid cross-account exposure.
- Email delivery requires valid SMTP configuration.
- Existing client accounts with no billing profile need a clear empty state.
