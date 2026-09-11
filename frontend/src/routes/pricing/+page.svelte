<script lang="ts">
  import { apiFetch } from "$lib/api";
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Seo from "$lib/components/Seo.svelte";
  import { auth, startGoogleSignIn } from "$lib/stores/auth";
  import { language, type LanguageCode } from "$lib/stores/language";
  import { homepageCopy } from "$lib/homepageCopy";
  import { get } from "svelte/store";

  type BillingCycle = "monthly" | "annual";

  const usePolarCheckout = false;

  type Plan = {
    id: "standard" | "plus";
    price: Record<BillingCycle, { amount: string; period: "month" | "year" | "" }>;
  };

  const plans: Plan[] = [
    {
      id: "standard",
      price: {
        monthly: { amount: "€7", period: "month" },
        annual: { amount: "€70", period: "year" },
      },
    },
    {
      id: "plus",
      price: {
        monthly: { amount: "€10", period: "month" },
        annual: { amount: "€100", period: "year" },
      },
    },
  ];

  let currentLang: LanguageCode = "en";
  const pricingMetaDescriptions: Record<LanguageCode, string> = {
    en: "Choose a HostingQr plan and preview simple or multilanguage examples before checkout.",
    it: "Scegli un piano HostingQr e guarda esempi semplici o multilingua prima del checkout.",
    es: "Elige un plan de HostingQr y revisa ejemplos simples o multilingües antes de contratar.",
    hr: "Odaberite HostingQr paket i pregledajte jednostavne ili višejezične primjere prije naplate.",
  };

  language.subscribe((value) => {
    currentLang = value;
  });

  $: copy = homepageCopy[currentLang].pricing;
  $: planCopy = {
    standard: copy.plans.standard,
    plus: copy.plans.plus,
  };
  let billingCycle: BillingCycle = "monthly";
  let checkoutPlanId: string | null = null;
  let checkoutError = "";

  function setBillingCycle(nextCycle: BillingCycle) {
    billingCycle = nextCycle;
  }

  async function startCheckout(planId: string) {
    checkoutError = "";

    if (!usePolarCheckout) {
      window.location.href = `/contact?plan=${encodeURIComponent(planId)}&billingCycle=${encodeURIComponent(billingCycle)}`;
      return;
    }

    if (get(auth).status !== "authenticated") {
      startGoogleSignIn();
      return;
    }

    checkoutPlanId = planId;

    try {
      const response = await apiFetch("/api/billing/checkout", {
        method: "POST",
        body: JSON.stringify({
          tier: planId,
          billingCycle,
        }),
      });

      if (!response.ok) {
        throw new Error(`Checkout failed with status ${response.status}`);
      }

      const checkout = (await response.json()) as { checkoutUrl: string };
      window.location.href = checkout.checkoutUrl;
    } catch {
      checkoutError = "Checkout is not available right now. Please try again or contact support.";
      checkoutPlanId = null;
    }
  }
</script>

<Seo
  title={`${copy.title} - HostingQr`}
  description={pricingMetaDescriptions[currentLang]}
  path="/pricing"
/>

<div class="flex min-h-screen flex-col bg-[rgba(243,244,246,0.98)]">
<Navigation />

<main class="flex-1 px-4 pb-16 pt-22 sm:px-6 lg:px-8">
  <section class="mx-auto max-w-6xl">
    <div
      class="mx-auto max-w-3xl text-center"
    >
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
        {copy.eyebrow}
      </p>
      <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
        {copy.title}
      </h1>
      <p class="mx-auto mt-5 max-w-2xl text-base leading-7 text-stone-600 sm:text-lg">
        {copy.subtitle}
      </p>
    </div>

    <div class="mt-8 flex justify-center">
      <div class="w-full max-w-sm rounded-full border border-stone-200 bg-white p-1 shadow-sm">
        <div class="grid grid-cols-2 gap-1">
            <button
              type="button"
              on:click={() => setBillingCycle("monthly")}
              class={`rounded-full px-4 py-2 text-sm font-medium transition-all ${billingCycle === "monthly" ? "bg-stone-900 text-white shadow-sm" : "text-stone-600 hover:text-stone-900"}`}
            >
            {copy.monthly}
          </button>
          <button
            type="button"
            on:click={() => setBillingCycle("annual")}
            class={`rounded-full px-4 py-2 text-sm font-medium transition-all ${billingCycle === "annual" ? "bg-stone-900 text-white shadow-sm" : "text-stone-600 hover:text-stone-900"}`}
          >
            {copy.annual}
          </button>
        </div>
      </div>
    </div>
    <p class="mt-3 text-center text-sm font-medium text-stone-600">{copy.annualNote}</p>

    <div class="mx-auto mt-10 grid max-w-4xl gap-6 md:grid-cols-2 md:items-stretch">
      {#each plans as plan}
        <article
          id={plan.id}
          class="group flex h-full flex-col rounded-[2.25rem] border border-stone-200 bg-white p-6 text-stone-900 shadow-[0_18px_50px_rgba(45,53,46,0.08)] transition-all duration-300 hover:-translate-y-1 hover:border-stone-300 hover:shadow-[0_26px_70px_rgba(45,53,46,0.12)] sm:p-7"
          data-polar-tier={plan.id}
        >
          <div class="text-center">
            <div class="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-stone-100 text-stone-700">
              {#if plan.id === "standard"}
                <svg class="h-7 w-7" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true">
                  <rect x="5" y="4" width="12" height="15" rx="2" />
                  <path stroke-linecap="round" d="M8 8h6M8 11h6M8 14h4M17 7h2v13a2 2 0 0 1-2 2H8" />
                </svg>
              {:else}
                <svg class="h-7 w-7" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true">
                  <rect x="4" y="3" width="16" height="18" rx="3" />
                  <path stroke-linecap="round" d="M8 8h8M8 12h5M8 16h7" />
                  <circle cx="16.5" cy="12" r="1" fill="currentColor" stroke="none" />
                </svg>
              {/if}
            </div>
            <h2 class="mt-4 text-xl font-semibold tracking-tight text-stone-900">
              {copy.badges[plan.id]}
            </h2>
          </div>

          <div
            class="mt-6 h-1.5 w-14 rounded-full bg-stone-300 transition-all duration-300 group-hover:w-20"
          ></div>

          <div class="mt-4 flex items-baseline justify-center gap-2">
            <span
              class="text-4xl font-semibold tracking-tight text-stone-900"
            >
              {plan.price[billingCycle].amount}
            </span>
            {#if plan.price[billingCycle].period}
              <span class="text-sm font-medium text-stone-500">
                / {plan.price[billingCycle].period === "month" ? copy.periods.month : copy.periods.year}
              </span>
            {/if}
          </div>
          <p
            class="mt-3 text-center text-sm leading-7 text-stone-600"
          >
              {planCopy[plan.id].description}
          </p>
          <p class="mx-auto mt-4 w-fit rounded-full border border-stone-200 bg-stone-100 px-3.5 py-1.5 text-center text-xs font-bold uppercase tracking-[0.08em] text-stone-800">
            {planCopy[plan.id].included}
          </p>

          <div
            class="mt-8 mb-3 border-t border-stone-200/80 pt-5"
          >
            <div class="space-y-3">
              {#each planCopy[plan.id].features as feature}
                <div class="flex items-start gap-3 text-sm leading-6 text-stone-700">
                  <span class="mt-0.5 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-stone-100 text-stone-700" aria-hidden="true">✓</span>
                  <span>{feature}</span>
                </div>
              {/each}
            </div>
          </div>

          <div class="mt-auto space-y-3 pt-4">
            {#if plan.id === "standard"}
              <a
                href="https://hostingqr.com/paname"
                target="_blank"
                rel="noreferrer"
                class="inline-flex w-full items-center justify-center rounded-full border border-stone-200 bg-white px-5 py-3 text-sm font-medium text-stone-700 transition-all hover:-translate-y-0.5 hover:border-stone-300 hover:text-stone-950"
              >
                {copy.examples.image}
              </a>
            {:else}
              <span class="inline-flex w-full cursor-default items-center justify-center rounded-full border border-stone-200 bg-stone-50 px-5 py-3 text-sm font-medium text-stone-400" aria-disabled="true">
                {copy.examples.digitalSoon}
              </span>
            {/if}
            <button
              type="button"
              on:click={() => startCheckout(plan.id)}
              disabled={checkoutPlanId === plan.id}
              class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-5 py-3 text-sm font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
              data-polar-plan={plan.id}
            >
              {checkoutPlanId === plan.id ? "Opening checkout..." : planCopy[plan.id].button}
            </button>
          </div>
        </article>
      {/each}
    </div>

    <div class="mx-auto mt-7 max-w-3xl rounded-[1.5rem] border border-[rgba(140,157,142,0.22)] bg-[rgba(226,233,224,0.72)] px-5 py-5 shadow-[0_10px_28px_rgba(74,88,76,0.06)] sm:flex sm:items-center sm:justify-between sm:gap-7 sm:px-6">
      <div>
        <p class="text-[0.68rem] font-semibold uppercase tracking-[0.18em] text-stone-600">{copy.trial.eyebrow}</p>
        <h2 class="mt-1.5 text-xl font-semibold tracking-tight text-stone-900">{copy.trial.title}</h2>
        <p class="mt-1.5 max-w-xl text-sm leading-6 text-stone-600">{copy.trial.description}</p>
      </div>
      <a href="/contact?plan=free&billingCycle=trial" class="mt-4 inline-flex w-full shrink-0 items-center justify-center rounded-full border border-stone-300/80 bg-white/80 px-5 py-2.5 text-sm font-medium text-stone-800 transition-colors hover:bg-white sm:mt-0 sm:w-auto">
        {copy.trial.button}
      </a>
    </div>

    <p class="mx-auto mt-6 max-w-3xl text-center text-sm leading-6 text-stone-600">
      <span class="font-semibold text-stone-900">{copy.translationNote.label}:</span>
      {" "}{copy.translationNote.text}
    </p>

    {#if checkoutError}
      <p class="mx-auto mt-5 max-w-2xl rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-center text-sm text-red-700">
        {checkoutError}
      </p>
    {/if}

    <div class="mx-auto mt-10 max-w-3xl text-center">
      <p class="text-sm text-stone-600">
        <span class="font-medium text-stone-900">{copy.customPlan.label}</span>
        {" "}{copy.customPlan.text}
        <a
          href="/contact"
          class="inline-flex rounded-full bg-stone-200 px-2.5 py-1 font-medium text-stone-800 transition-colors hover:bg-stone-300"
          >{copy.customPlan.cta}</a
        >
      </p>
    </div>

  </section>
</main>

<Footer />
</div>
