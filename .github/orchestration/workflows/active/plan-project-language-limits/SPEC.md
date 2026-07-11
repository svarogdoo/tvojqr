# SPEC

## Scope

Add backend plan-limit checks for project creation and language addition.

## Affected Areas

- Billing entitlement service/model.
- Project service.
- Project service tests or endpoint coverage.
- README task markers/docs.

## Technical Approach

- Add plan limit definitions for `free`, `standard`, `plus`, and `admin`.
- Extend `IEntitlementService` with current-user limit access.
- In `CreateProjectAsync`, compare current project count against the plan project limit.
- In `AddLanguageAsync`, compare current language count against the plan language limit.
- Throw clear `InvalidOperationException` messages so existing endpoint conflict handling returns useful messages.

## Assumptions

- `free` remains a limited active tier and should allow 1 project and 1 language unless adjusted later.
- Existing pricing page is the source for Standard/Plus limits.

## Risks

- Users already over a new limit will be blocked from adding more but not automatically downgraded/deleted.
