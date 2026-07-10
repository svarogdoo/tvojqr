# HostingQr

HostingQr is a simple website for people who want to host images/files and share them through a QR code without buying or managing their own website.

The original use case is restaurants (menus), but the platform should work for any person or business that needs a quick QR-based landing page for documents or images.

## Product Vision

Users should be able to:

1. Sign in (Google login for now).
2. Upload images/files (for example menu images).
3. Choose a public name / URL ending.
4. Add multiple language versions of the same content.
5. Generate a QR code that points to their public page.
6. Print and place the QR code anywhere.

Visitors who scan the QR code should land on a simple public page where they can:

1. View the uploaded content.
2. Switch between available languages.

## Core Idea

The app should become mostly autonomous:

- A user comes in, creates an account, and manages their content without manual admin work.
- The user controls their uploads and presentation from a simple dashboard/menu.
- The system generates and maintains the public URL + QR code automatically.
- The system also handles recurring payments/subscriptions automatically.

## Current Direction (What We Are Building Toward)

### User Account & Access

- Authentication: Google sign-in (initial auth method).
- Each user has a private dashboard for managing their hosted content.

### User Dashboard (Simple Menu)

The dashboard should let users manage:

- Multiple projects from one account
- Posted image(s) / file(s)
- Language variants
- URL ending / slug (public page address)
- QR code generation and download/display
- Preview before publish/save

The intended control-panel flow is:

1. User signs in and lands on a dashboard listing all their projects.
2. User can open an existing project or create a new one.
3. Inside a project they can manage:
   - project name
   - one active slug
   - image uploads
   - language variants
   - preview before final save/publish
4. User saves changes and later retrieves the live project again from the dashboard.

### Pricing & Payments

To be fully autonomous, HostingQr should include built-in billing:

- Recurring payments (subscriptions)
- Tier-based plans
- Self-service plan upgrades/downgrades
- Billing status visible in the user dashboard

Pricing amounts are not decided yet. We will define pricing later, but the product and implementation should be designed to support tiers from the beginning.

For now, billing and payment work is intentionally deferred until the core control-panel, upload, slug, preview, and public-page flows are working well.

### Public Hosted Page

When someone visits the user’s public URL:

- They see the uploaded image/file content in a very simple layout.
- They can switch between languages if more than one is available.

## MVP Scope (Recommended)

This is the minimum useful version to build first:

1. Google login
2. Dashboard listing multiple user projects
3. Project settings page for one selected project
4. Upload one or more images (menus/documents)
5. Save one unique URL slug per project
6. Random slug generation option
7. Preview changes before final save
8. Public page renders uploaded images
9. Basic language switching (at least 2 language versions)

Project settings MVP should include:

- project name
- slug field with availability checking
- random slug generator button
- image upload section
- language-specific upload support
- preview before final save

Because users are authenticated in the dashboard flow, project creation should not require asking for email again.

## Post-MVP / Future Enhancements

- Support more file types (PDFs, docs)
- Better content organization (multiple sections/pages)
- QR code customization (size, format, branding)
- Analytics (scan count, last visit, top language)
- Expiration / temporary links
- Domain mapping (user connects their own domain later)
- Admin moderation / abuse controls
- Billing / paid tiers (storage, traffic, advanced features)
- Invoicing / receipts / tax handling (depending on market)

## Suggested Data/Feature Model (Guidance)

This is not a strict schema yet, but a useful implementation reference:

- User
- Project/Profile (the hosted page owner identity)
- Slug (public URL ending)
- Language Variant (e.g. `en`, `hr`, `de`)
- Asset/File (image or document upload)
- QR Code record (generated URL + asset metadata if stored)
- Subscription
- Plan/Tier
- Payment/Billing event (for audit/history)

## UX Principles (Guidance)

- Keep onboarding short and obvious.
- Minimize technical language for non-technical users.
- Make the “Get my QR code” action the main goal.
- Keep the public page fast and simple.
- Make language switching visible and easy on mobile.

## Technical Implementation Priorities (Guidance for Future Changes)

When implementing new features, prioritize in this order:

1. End-to-end user flow (login -> upload -> slug -> QR -> public page)
2. Reliable file hosting and URL generation
3. Simple content management UX
4. Billing foundation (subscriptions + tier enforcement)
5. Language support
6. Automation and quality-of-life improvements

## TODO (Based on Current Code + Product Direction)

This TODO reflects the current codebase state and the target HostingQr product described above.

Status markers for tasks:

- `[DONE]` completed
- `[PARTIAL]` started but needs another pass
- No marker = not started yet

### 1. Front Page (Marketing / Landing)

[DONE] 1.1 Rework landing page copy from manual service/agency messaging to self-service SaaS messaging.
[DONE] 1.1.a Update hero text to explain upload -> slug -> QR flow.
[DONE] 1.1.b Update CTA labels to match account/signup flow instead of manual submission.
[DONE] 1.1.c Align examples/services sections with actual MVP features.
1.2 Replace placeholder contact form behavior with real backend handling or remove it until supported.
1.2.a `src/lib/components/Contact.svelte` currently simulates success locally only.
[DONE] 1.3 Add pricing section on front page (even if prices are "coming soon" initially).
[DONE] 1.3.a Show planned tiers and key differences.
[DONE] 1.3.b Add billing/subscription CTA.
1.4 Improve navigation for product app flow.
1.4.a Add clear links for `Login`, `Dashboard`, `Pricing`.
[DONE] 1.4.b Keep current multilingual language switch but ensure labels remain consistent.
[DONE] 1.4.c Limit the language switch to English and Spanish.
1.5 Front-page polish/fixes.
1.5.a Review mobile spacing/overflow on hero image crop.
1.5.b Check accessibility (button labels, focus states, heading order).
[DONE] 1.5.c Simplify the homepage by removing examples and contact, then add a short story section.
[DONE] 1.5.d Make the page shell fill to the footer so no extra background shows below it.

### 2. UI Redesign / Visual System

[DONE] 2.1 Redesign the visual direction so the product feels smoother and more polished.
[DONE] 2.1.a Define a softer/smoother color palette (brand colors, neutrals, success/error states).
[DONE] 2.1.b Replace harsh/high-contrast combinations where they feel rough.
[DONE] 2.1.c Document palette usage rules (primary action, secondary action, surfaces, borders).
[PARTIAL] 2.2 Improve typography and spacing consistency across the app.
[PARTIAL] 2.2.a Define heading/body/button text styles.
[PARTIAL] 2.2.b Standardize spacing scale for sections/cards/forms.
[PARTIAL] 2.2.c Normalize border radius and shadow styles.
2.3 Create consistent component styling patterns.
[PARTIAL] 2.3.a Buttons (primary/secondary/ghost/destructive).
[DONE] 2.3.b Form fields (inputs, labels, hints, errors).
2.3.c Modals (`SuccessModal`, `ErrorModal`) visual consistency and motion polish.
[PARTIAL] 2.4 Mobile-first visual polish pass.
[DONE] 2.4.a Check landing page sections for spacing and overflow.
[DONE] 2.4.b Check create/upload flow form readability on smaller screens.
[DONE] 2.4.c Check public slug page readability on mobile.
[DONE] 2.4.d Check dashboard and project settings responsiveness on mobile.
[DONE] 2.4.e Fix project editor overflow on narrow screens.

### 3. Manual Intake Flow (`/create-new`) -> Product Onboarding

3.1 Replace manual "send us files" form with account-based onboarding flow.
3.1.a Add sign-in/sign-up entry point (Google auth first).
3.1.b Remove personal info fields that are only used for email intake (`firstName`, `lastName`, `email`) once auth exists.
3.2 Keep current upload UI pieces, but refactor for authenticated asset uploads.
3.2.a Reuse/keep multi-file preview and remove-file behavior from `src/routes/create-new/+page.svelte`.
3.2.b Add upload validation (file type, file size, max file count).
3.2.c Add user-facing validation messages before submit.
[PARTIAL] 3.3 Add language-aware content management to onboarding/dashboard.
[DONE] 3.3.a Let user assign files to language variants.
[DONE] 3.3.b Let user set default language.
3.4 Add slug setup UX.
3.4.a Validate slug format in UI.
3.4.b Check slug availability.
3.4.c Explain final URL before publish.

### 4. Current Server Action / Email Submission (Short-Term Fixes)

[DONE] 4.1 Fix server action bug in `src/routes/create-new/+page.server.ts`.
[DONE] 4.1.a `from:` currently uses `${EMAIL_USER}` but only `env.EMAIL_USER` is defined.
4.2 Decide whether to keep email submission temporarily.
4.2.a If kept: harden it (validation, rate limiting, spam protection).
4.2.b If removed: replace with authenticated upload/storage flow.
4.3 Add robust error handling for file/email submission path.
[DONE] 4.3.a Handle missing env vars (`EMAIL_USER`, `EMAIL_PASS`) gracefully.
4.3.b Return user-friendly error states for upload/send failures.

### 5. Public Hosted Page (`/[slug]`)

5.1 Replace hardcoded demo data with dynamic data source.
5.1.a `src/lib/restaurants.ts` is currently a static example object.
[DONE] 5.2 Implement real language switching on public pages.
[DONE] 5.2.a Public page currently reads language store but does not render per-language assets/content yet.
[DONE] 5.2.b Show only languages available for that page.
5.3 Support more than one asset/file per hosted page.
5.3.a Image gallery / multi-page menu display.
5.3.b PDF display/download behavior (if included in MVP).
[PARTIAL] 5.4 Improve public page UX.
[DONE] 5.4.a Better empty/not-found states (brand-consistent and localized).
[DONE] 5.4.b Mobile-first reading experience for menus/documents.
[DONE] 5.4.c Optional file title/description display.
[DONE] 5.4.d Render saved default-language images on the public slug page.
[DONE] 5.4.e Show a distinct disabled-project state with a path back to HostingQr.

### 6. Authentication & User Dashboard (Core Missing Product Layer)

6.1 Add authentication (Google login first).
[DONE] 6.1.a Session handling
[PARTIAL] 6.1.b Route protection for dashboard/pages
[DONE] 6.1.c Sign out flow
[PARTIAL] 6.2 Build initial dashboard.
[DONE] 6.2.a List user hosted pages/projects
[PARTIAL] 6.2.b Create new hosted page/project
[DONE] 6.2.b.1 Keep new project in frontend draft state until explicit save.
[DONE] 6.2.c Open a specific project from the project list
[DONE] 6.2.d Edit project name
[DONE] 6.2.e Edit one active slug per project
[DONE] 6.2.e.1 Group language selection into a navigation dropdown.
[DONE] 6.2.e.2 Add an auth-aware user menu with projects/sign-out or sign-in actions.
[DONE] 6.2.e.3 Auto-check slug availability after typing pauses.
[PARTIAL] 6.2.f Manage uploads
[DONE] 6.2.f.1 Add quick public-view access from dashboard project rows.
[DONE] 6.2.f.2 Show project status in the dashboard list.
[DONE] 6.2.f.3 Stage asset add/remove changes in the editor until explicit save.
[DONE] 6.2.f.4 Warn before leaving the project editor with unsaved changes.
[DONE] 6.2.f.5 Reorder saved images from the project editor.
[DONE] 6.2.g Manage languages
[DONE] 6.2.g.1 Add/remove language sections in the project editor.
[DONE] 6.2.g.2 Upload images into a selected language section.
[DONE] 6.2.g.3 Make each language box editable with a collision-safe selector and remove the separate default-language card.
[DONE] 6.2.g.4 Persist non-default language section edits by loading the project language baseline on open.
[DONE] 6.2.h Preview project changes before final save
[DONE] 6.2.h.1 Add an eye preview button beside the project title in the editor.
[PARTIAL] 6.2.i View/download QR code
6.2.j If the user has no projects yet, show a clear empty state instead of a generic load/error message.
[DONE] 6.2.k Choose a public page background color from project settings.
6.3 Define first account model decisions.
6.3.a [DECIDED] One user can manage multiple projects
6.3.b Owner profile display name

### 7. File Storage & Hosting Infrastructure

[PARTIAL] 7.1 Replace email attachments as "storage" with real file storage provider.
[DONE] 7.1.a Choose storage backend (Cloudflare R2).
[DONE] 7.1.b Implement upload pipeline from authenticated users.
[PARTIAL] 7.1.c Save metadata (file type, size, language, order).
[DONE] 7.1.d Convert uploaded images to a compressed safe web format (`.webp`) when appropriate.
[DONE] 7.1.e Prefer R2 automatically when configured so production uploads resolve from durable object storage.
[DONE] 7.1.f Store R2 object keys in Postgres while returning direct R2 public URLs to the frontend.
7.2 Add file lifecycle management.
[PARTIAL] 7.2.a Delete/replace uploads
7.2.b Versioning strategy (optional later)
7.2.c [PARTIAL] Storage cleanup when content is removed
[PARTIAL] 7.2.d Support multiple images per project and per language variant.
[DONE] 7.2.d.1 Persist saved image display order per project.
7.2.e Use a custom R2 asset domain for production image URLs, e.g. `assets.hostingqr.com` via `Storage__R2__PublicBaseUrl`.
7.3 Enforce limits (especially once billing exists).
7.3.a Max file size
7.3.b Max total storage
7.3.c Allowed MIME types

### 8. QR Code Generation

8.1 Add QR code generation for each published slug.
[DONE] 8.1.a Generate QR from final public URL
[DONE] 8.1.b Display in project dashboard/editor
[DONE] 8.1.c Download/export (PNG/SVG)
8.1.d Add QR styling presets and color controls
8.2 Decide QR storage strategy.
8.2.a Generate on demand vs persist generated assets
8.3 Add QR regeneration rules if slug changes.

### 9. Billing, Pricing & Payments (Deferred Until Core Product Flow Is Stable)

[PARTIAL] 9.1 Define plan structure before pricing amounts.
[DONE] 9.1.a Tier names
9.1.b Feature limits (projects, languages, storage, traffic, file types)
[PARTIAL] 9.1.c Free tier and/or free trial policy
9.2 [PARTIAL] Integrate recurring payments provider.
9.2.a [PARTIAL] Subscription checkout
9.2.b Customer billing portal
9.2.c [PARTIAL] Webhook handling for subscription events
[PARTIAL] 9.3 Add billing status to dashboard.
[DONE] 9.3.a Current plan
9.3.b Renewal date
9.3.c Payment status / failed payment warnings
[PARTIAL] 9.4 Implement plan enforcement.
[DONE] 9.4.a Block/limit actions when user exceeds tier
9.4.b Grace period behavior after failed payment
9.4.c Downgrade handling when usage exceeds new tier
[DONE] 9.5 Add a pricing contact form that emails inbound requests.
9.5.a Name and email required
9.5.b Optional file upload for hosted content
9.5.c Optional multiple-language request details
9.5.d Success confirmation after submission

### 10. Data Model & Backend Foundation

[PARTIAL] 10.0 Scaffold the `.NET` backend foundation.
[DONE] 10.0.a Create backend solution and layered project structure.
[DONE] 10.0.b Add Swagger/OpenAPI and a simple backend smoke-test endpoint.
[DONE] 10.0.c Add Dapper/Npgsql foundation without EF.
[PARTIAL] 10.1 Introduce a database for users/projects/assets/subscriptions.
[DONE] 10.1.a Users
[DONE] 10.1.b Projects/hosted pages (one user -> many projects)
[DONE] 10.1.c Slugs (one active slug per project)
[DONE] 10.1.c.1 Persist per-project public page background color.
[DONE] 10.1.d Language variants
[PARTIAL] 10.1.e Assets/files
10.1.f Subscriptions/plans
10.1.g Billing events
[PARTIAL] 10.2 Build server-side CRUD endpoints/actions for dashboard operations.
[DONE] 10.2.a List projects for the current user dashboard
[DONE] 10.2.b Create a new project
[DONE] 10.2.c Fetch one project for editing/settings
[PARTIAL] 10.2.d Save project settings and uploads
[DONE] 10.2.d.1 Save project name and slug settings
[DONE] 10.2.d.2 Upload default-language image assets
[DONE] 10.2.d.3 Preserve project form state during upload/save attempts
[DONE] 10.2.d.4 Update project status and delete a project
[DONE] 10.2.d.5 Add/remove project language variants and upload assets per language.
10.2.e Preview project before final save/publish
[PARTIAL] 10.3 Add slug uniqueness checks and conflict handling.
[DONE] 10.3.a Check custom slug availability
[DONE] 10.3.b Generate a random unique slug
[DONE] 10.3.c Enforce one active slug per project
[DONE] 10.3.d Debounce project-editor slug availability checks.
10.4 Add audit/logging for important state changes (publish/unpublish, billing changes).

### 11. Localization / Translation System

11.1 Refactor translations to support both marketing UI and user-hosted content cleanly.
11.1.a Current `src/lib/translations.ts` is app-UI focused and very large.
11.2 Ensure consistency and proofreading across all languages.
11.2.a Fix wording/typos in current translations.
[DONE] 11.2.b Standardize "HostingQr" casing (`HostingQr` vs `HostingQR`).
11.3 Persist user language preference (app UI), not only in-memory store.

### 12. Quality, Security & Operations

12.1 Add form/file validation on both client and server for all upload flows.
[DONE] 12.1.a Slug format validation
[DONE] 12.1.b Random slug generation validation/path coverage
[PARTIAL] 12.1.c Image-only upload validation for MVP
[DONE] 12.1.e Reusable snackbar feedback for save/upload outcomes
[DONE] 12.1.f Reusable confirmation modal for destructive actions
[DONE] 12.1.g Unsaved-change warning for project editor navigation/refresh.
12.1.d Per-language upload validation
12.2 Add abuse protection.
12.2.a Rate limiting
12.2.b Spam protection
12.2.c File scanning policy (if needed)
12.3 Add tests for core flows.
12.3.a Slug validation
12.3.b Upload validation
12.3.c Public page rendering by slug
12.3.d [PARTIAL] Billing webhook handling
12.4 Add environment setup documentation.
[PARTIAL] 12.4.a Auth env vars
[DONE] 12.4.a.1 Backend Google auth supports user-secrets / env-based local configuration.
12.4.b [DONE] Storage env vars
12.4.c [DONE] Billing env vars
12.4.d Email env vars (if temporary flow remains)
12.5 Add monitoring/error tracking for production.

### 13. Local Developer Orchestration

[DONE] 13.1 Replace the local orchestration runtime with a markdown-only repo workflow.
[DONE] 13.1.a Use `.github/orchestration/main.instructions.md` as the orchestration entry point.
[DONE] 13.1.b Define role behavior in `.github/orchestration/roles/*.instructions.md`.
[DONE] 13.1.c Define workflow document shapes in `.github/orchestration/templates/*.template.md`.
[DONE] 13.1.d Keep active and completed workflow folders under `.github/orchestration/workflows/`.
13.1.e Decide whether active workflow files should be committed or kept ephemeral per task.
13.1.f Add example completed workflow records once the process has been used end to end.

[DONE] 13.2 Restructure the workspace for clearer frontend/backend/tooling separation.
[DONE] 13.2.a Move the Svelte app into `frontend/`.
[DONE] 13.2.b Keep local developer orchestration under `.github/orchestration/`.
[DONE] 13.2.c Reserve `backend/` for the future `.NET` product backend.

## Open Questions (To Decide Later)

- Which storage provider will host uploaded files?
- Will public pages support only images first, or PDFs too in MVP?
- What slug rules should apply (length, allowed characters, uniqueness)?
- Should QR codes be stored permanently or generated on-demand?
- What is the max upload size for MVP?
- How aggressive should image compression be before quality becomes unacceptable?
- Should preview changes be a temporary draft view or a fully generated unpublished public page?
- Which payment provider should be used (for example Stripe)?
- What limits define each tier (storage, number of pages, traffic, languages)?
- Will there be a free tier and/or free trial?
- How should failed payments affect hosted pages (grace period, soft lock, unpublish)?
- What happens on downgrade if usage exceeds the lower tier limits?

## Development Note

Use this README as the working product brief when planning changes. If the product direction changes, update this file first so implementation stays aligned.

## Local Development

```sh
npm install
npm run dev
```

This starts both:

- local PostgreSQL in Docker
- frontend Svelte app
- backend ASP.NET Core API

Local Docker PostgreSQL is exposed on port `5433` to avoid conflicts with any existing local Postgres already using `5432`.

You can also run them separately:

```sh
npm run dev:db
npm run dev:frontend
npm run dev:backend
npm run dev:down
```

Frontend local env:

```sh
cp frontend/.env.example frontend/.env
```

Production frontend env should set:

```text
PUBLIC_API_BASE_URL=https://api.hostingqr.com
```

## Build

```sh
npm run build
npm run preview
```
