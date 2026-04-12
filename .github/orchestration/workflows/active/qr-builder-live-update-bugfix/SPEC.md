# SPEC

## Scope

Fix the QR builder preview update behavior and improve the section layout.

## Affected Areas

- `frontend/src/lib/components/ProjectQrBuilder.svelte`

## Technical Approach

- Inspect how the QR library instance is created and updated.
- Apply the smallest reliable fix so option changes trigger a visible preview update.
- Refine the layout sizing and spacing so the preview and controls fit the container better.

## Assumptions

- The current QR library is still suitable.

## Risks

- If the library has incomplete update behavior for certain options, the safest fix may require rebuilding the instance on change.
