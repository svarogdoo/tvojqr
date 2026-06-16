# Brief

Goal:
- Support internal tiers: none, free, standard, world cup, plus.
- Let the app distinguish signed-in users who can use tools from signed-in users who need a plan.
- Keep live payment checkout out of scope for this pass.

Success criteria:
- Backend exposes the current user's tier entitlement.
- Project tooling is blocked for users without an active tier.
- Dashboard shows a plan-picking prompt for signed-in users without an active tier.
