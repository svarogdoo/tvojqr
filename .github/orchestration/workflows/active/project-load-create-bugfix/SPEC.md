# SPEC

## Scope

Diagnose and fix the current dashboard project loading and project creation failures.

## Affected Areas

- backend project and asset query/materialization path
- frontend dashboard create/open flow
- any response model mismatches introduced by the recent upload/settings work

## Technical Approach

- Reproduce the failing routes/endpoints.
- Inspect backend query materialization and project detail response building.
- Fix the minimal backend/frontend mismatch causing untitled or unloadable projects.
- Verify with existing test/build/check commands.

## Assumptions

- The failures are caused by recent backend/frontend changes rather than auth configuration.

## Risks

- Existing local DB contents may require a compatibility-safe fix rather than a destructive reset.
