<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Seo from "$lib/components/Seo.svelte";

  const lastUpdated = "23 July 2026";
</script>

<Seo
  title="Privacy Policy - HostingQr"
  description="HostingQr Privacy Policy covering account data, uploaded content, analytics, payments, hosting providers, and EU/GDPR rights."
  path="/privacy"
/>

<div class="flex min-h-screen flex-col bg-[rgba(243,244,246,0.98)]">
  <Navigation />

  <main class="flex-1 px-4 pb-16 pt-28 sm:px-6 lg:px-8">
    <article class="mx-auto max-w-4xl rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_18px_50px_rgba(45,53,46,0.08)] sm:p-10">
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">Legal</p>
      <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Privacy Policy</h1>
      <p class="mt-4 text-sm text-stone-500">Last updated: {lastUpdated}</p>

      <div class="mt-8 space-y-8 text-sm leading-7 text-stone-700 sm:text-base">
        <section>
          <h2 class="text-xl font-semibold text-stone-900">1. Who Controls Your Data</h2>
          <p class="mt-3">
            HostingQr is operated by Svarog d.o.o., Jovana R. Deretica 61, 89101 Trebinje, Bosnia and Herzegovina. JIB: 4404749190009. For privacy questions, deletion requests, or data requests, contact <a class="font-medium text-stone-950 underline" href="mailto:support@hostingqr.com">support@hostingqr.com</a> or <a class="font-medium text-stone-950 underline" href="tel:+35799180703">+357 99 180703</a>.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">2. Data We Collect</h2>
          <p class="mt-3">
            We collect account data such as your name, email address, authentication identifier, and session information. We also collect project data you create, including project names, slugs, language variants, QR-related public URLs, background settings, uploaded images or files, file names, generated image versions, and metadata needed to operate the service.
          </p>
          <p class="mt-3">
            If you contact us, we collect the information you provide in the form or email, including your name, email address, message, optional language details, and optional uploaded files. If you subscribe to a paid plan, payment and billing details are handled by the checkout provider, such as Polar, and we receive subscription status information needed to unlock access.
          </p>
          <p class="mt-3">
            If you consent to analytics, Google Analytics may collect information about pages viewed, approximate location, device and browser details, referral source, campaign parameters, and interactions needed to measure site usage. We use this information in aggregate to understand traffic and improve HostingQr.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">3. Public Content</h2>
          <p class="mt-3">
            Content published to a hosted page may be visible to anyone with the public URL or QR code. This can include uploaded menu images, generated page images, file names, project names, and language variants. Do not upload personal data, confidential material, or content you do not want to make public unless you understand the sharing risk.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">4. How We Use Data</h2>
          <p class="mt-3">
            We use data to provide accounts, authentication, hosted pages, uploads, language variants, QR-page access, support, subscription access, security, abuse prevention, service maintenance, legal compliance, and product improvement. We do not sell your personal data.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">5. Legal Bases For EU/EEA Users</h2>
          <p class="mt-3">
            Where EU/EEA privacy law applies, we process data based on contract performance when providing HostingQr, legitimate interests when securing and improving the service, legal obligations when required by law or accounting rules, and consent where a specific consent mechanism is used.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">6. Service Providers</h2>
          <p class="mt-3">
            We use third-party providers to run the service. These may include Netlify for frontend hosting and domain/DNS services, Railway for backend/database hosting, Cloudflare R2 for uploaded asset storage, Google for authentication and consent-based Google Analytics, Polar for checkout/subscription handling where applicable, and our email provider for support and contact-form messages. These providers process data only as needed to provide their services to us.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">7. Payments</h2>
          <p class="mt-3">
            Payment details are processed by the payment provider shown during checkout, including Polar where applicable. We do not store full card details. We may store subscription tier, subscription status, customer identifiers, billing-period status, and related records needed to provide or restrict access.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">8. Cookies And Sessions</h2>
          <p class="mt-3">
            HostingQr uses essential cookies or similar session mechanisms to keep users signed in and to protect authenticated account areas. These are necessary for the service to work. With your consent, Google Analytics may set analytics cookies to distinguish visits and measure traffic sources. Analytics does not load before you accept it. You can reject analytics or change your choice later through the Cookie settings link in the site footer.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">9. Retention</h2>
          <p class="mt-3">
            We keep account, project, and uploaded content data while your account or project is active or as needed to provide the service. We may retain limited records for security, dispute resolution, accounting, tax, legal compliance, backups, and abuse prevention. Public pages and uploaded content may remain available until deleted, disabled, or removed under our policies.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">10. Security</h2>
          <p class="mt-3">
            We use reasonable technical and organizational measures to protect data, but no internet service is completely secure. You are responsible for keeping your authentication account secure and for not uploading sensitive content unless you accept the risks of public hosting.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">11. International Processing</h2>
          <p class="mt-3">
            Our providers may process data in countries outside Bosnia and Herzegovina or outside the EU/EEA. Where required, we rely on provider safeguards, contractual protections, and legal transfer mechanisms available through those providers.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">12. Your Rights</h2>
          <p class="mt-3">
            Depending on where you live, you may have rights to access, correct, delete, restrict, object to processing, or receive a copy of your personal data. You may withdraw analytics consent at any time through Cookie settings. You may also have the right to complain to a data protection authority. To make a request, contact <a class="font-medium text-stone-950 underline" href="mailto:support@hostingqr.com">support@hostingqr.com</a>. We may need to verify your identity before responding.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">13. Children</h2>
          <p class="mt-3">
            HostingQr is not intended for children. You must not use the service if you are not legally able to enter into a binding agreement or operate a business account in your jurisdiction.
          </p>
        </section>

        <section>
          <h2 class="text-xl font-semibold text-stone-900">14. Changes To This Policy</h2>
          <p class="mt-3">
            We may update this Privacy Policy from time to time. The updated date will show the latest version. Continued use of HostingQr after updates means the revised policy applies.
          </p>
        </section>
      </div>
    </article>
  </main>

  <Footer />
</div>
