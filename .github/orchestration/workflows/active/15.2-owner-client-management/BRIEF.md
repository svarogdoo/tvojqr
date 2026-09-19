# BRIEF

## User-Facing Goal

Provide a clear owner workspace for onboarding and supporting clients, and a read-only billing summary for each client.

## Success Criteria

- The owner can see all clients and menus and edit any menu.
- The owner can invite a client to take ownership of a completed menu.
- A client can access only their own menus.
- The owner can maintain billing cycle and invoice dates and upload invoice PDFs.
- The client can see their joined date, menu types, billing dates, and invoice links.

## UX Notes

- Extend the existing admin overview rather than creating a second admin shell.
- Keep assignment available from the project editor.
- Make billing controls owner-only and client billing content read-only.

## Risks Or Tradeoffs

- Invitation acceptance must bind to the invited Google email.
- Private invoices require authenticated delivery rather than public asset URLs.
- Global admin editing must be applied consistently across all project child resources.
