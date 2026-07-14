# SPEC

## Scope

- Pricing page checkout button behavior.
- Contact page notice and hidden fields.
- Contact form email content.

## Technical Approach

- Add a boolean switch on the pricing page to control whether paid plans use Polar checkout.
- When disabled, send paid plans to `/contact?plan=<tier>&billingCycle=<cycle>`.
- Keep existing Polar checkout request code in place for later re-enable.
- On contact page, read query parameters and show a temporary payment notice for `standard` or `plus`.
- Include hidden `plan` and `billingCycle` fields in the form.
- Include these fields in the email body.

## Risks

- Contact page query parameters are client-side state only until form submit; hidden fields carry the values to the server action.
