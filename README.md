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

- Posted image(s) / file(s)
- Language variants
- URL ending / slug (public page address)
- QR code generation and download/display
- Subscription plan / billing status

### Pricing & Payments

To be fully autonomous, HostingQr should include built-in billing:

- Recurring payments (subscriptions)
- Tier-based plans
- Self-service plan upgrades/downgrades
- Billing status visible in the user dashboard

Pricing amounts are not decided yet. We will define pricing later, but the product and implementation should be designed to support tiers from the beginning.

### Public Hosted Page

When someone visits the user’s public URL:

- They see the uploaded image/file content in a very simple layout.
- They can switch between languages if more than one is available.

## MVP Scope (Recommended)

This is the minimum useful version to build first:

1. Google login
2. Single user dashboard page
3. Upload one or more images (menus/documents)
4. Save a unique URL slug
5. Generate QR code for that URL
6. Public page renders uploaded images
7. Basic language switching (at least 2 language versions)

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

## Open Questions (To Decide Later)

- Which storage provider will host uploaded files?
- Will public pages support only images first, or PDFs too in MVP?
- What slug rules should apply (length, allowed characters, uniqueness)?
- Can a user create multiple hosted pages/projects, or only one at first?
- Should QR codes be stored permanently or generated on-demand?
- What is the max upload size for MVP?
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

## Build

```sh
npm run build
npm run preview
```
