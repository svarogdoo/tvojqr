<script lang="ts">
  import { apiFetch } from "$lib/api";
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, startGoogleSignIn } from "$lib/stores/auth";
  import { language, type LanguageCode } from "$lib/stores/language";
  import { homepageCopy } from "$lib/homepageCopy";
  import { get } from "svelte/store";

  type BillingCycle = "monthly" | "annual";

  const usePolarCheckout = false;

  type Plan = {
    id: string;
    badge: string;
    featured: boolean;
    price: Record<BillingCycle, string>;
    description: string;
    button: string;
    details: Array<{ label: string; value: string }>;
  };

  const plans: Plan[] = [
    {
      id: "free",
      badge: "Trial",
      featured: false,
      price: {
        monthly: "Free",
        annual: "Free",
      },
      description:
        "Not sure how it works? Send us your files and we'll send you your preview.",
      button: "Start test",
      details: [
        { label: "Projects", value: "14-day test" },
        { label: "Menus", value: "—" },
        { label: "Languages", value: "—" },
        { label: "Uploaded files", value: "—" },
        { label: "Traffic", value: "—" },
        { label: "Support", value: "Direct with me" },
      ],
    },
    {
      id: "standard",
      badge: "Standard",
      featured: true,
      price: {
        monthly: "€7 / month",
        annual: "€70 / year",
      },
      description:
        "Great for small restaurants with simple needs. You can always upgrade later.",
      button: "Choose standard",
      details: [
        { label: "Projects", value: "1" },
        { label: "Menus", value: "2" },
        { label: "Languages", value: "3" },
        { label: "Uploaded files", value: "10 total" },
        { label: "Traffic", value: "25 GB" },
        { label: "Support", value: "Standard" },
      ],
    },
    {
      id: "plus",
      badge: "Plus",
      featured: false,
      price: {
        monthly: "€12 / month",
        annual: "€120 / year",
      },
      description:
        "You need more? This plan is for you. Want to go even bigger? Contact us!",
      button: "Choose plus",
      details: [
        { label: "Projects", value: "5" },
        { label: "Menus", value: "5" },
        { label: "Languages", value: "7" },
        { label: "Uploaded files", value: "25 total" },
        { label: "Traffic", value: "100 GB" },
        { label: "Support", value: "Priority" },
      ],
    },
  ];

  let currentLang: LanguageCode = "en";
  language.subscribe((value) => {
    currentLang = value;
  });

  $: copy = homepageCopy[currentLang].pricing;
  $: planCopy = {
    free: copy.plans.free,
    standard: copy.plans.standard,
    plus: copy.plans.plus,
  };
  $: detailLabels = [
    copy.details.projects,
    copy.details.menus,
    copy.details.languages,
    copy.details.uploadedFiles,
    copy.details.traffic,
    copy.details.support,
  ];
  let billingCycle: BillingCycle = "monthly";
  let checkoutPlanId: string | null = null;
  let checkoutError = "";

  function setBillingCycle(nextCycle: BillingCycle) {
    billingCycle = nextCycle;
  }

  async function startCheckout(planId: string) {
    checkoutError = "";

    if (planId === "free") {
      window.location.href = "/contact";
      return;
    }

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

<svelte:head>
  <title>{copy.title} - HostingQr</title>
  <meta
    name="description"
    content={currentLang === "es"
      ? "Elige un plan de HostingQr y revisa ejemplos simples o multilingües antes de contratar."
      : "Choose a HostingQr plan and preview simple or multilanguage examples before checkout."}
  />
</svelte:head>

<div class="flex min-h-screen flex-col bg-[rgba(243,244,246,0.98)]">
<Navigation />

<main class="flex-1 px-4 pb-16 pt-22 sm:px-6 lg:px-8">
  <section class="mx-auto max-w-6xl">
    <div
      class="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between"
    >
      <div class="max-w-2xl">
        <p
          class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500"
        >
          {copy.eyebrow}
        </p>
      </div>

    </div>

    <blockquote class="mx-auto mt-3 max-w-3xl px-6 py-3 text-center sm:px-8">
      <p class="text-2xl font-semibold tracking-tight text-stone-900 sm:text-[2rem]">
        {copy.quote.title}
        <span class="mt-2 flex flex-wrap items-center justify-center gap-2">
          <span>We will do it for</span>
          <span class="inline-flex rounded-full bg-emerald-600 px-3.5 py-1 text-xl font-bold tracking-[0.08em] text-white shadow-[0_12px_28px_rgba(5,150,105,0.22)] sm:text-2xl">
            FREE
          </span>
          <span>!</span>
        </span>
      </p>
      <p class="mt-4 text-sm font-medium text-stone-600 sm:text-base">
        Pick your plan and reach out!
      </p>
    </blockquote>

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

    <div class="mx-auto mt-12 grid max-w-5xl gap-6 lg:grid-cols-3 lg:items-stretch">
      {#each plans as plan}
        <article
          id={plan.id}
          class={`group flex h-full flex-col rounded-[2.25rem] border p-6 shadow-[0_18px_50px_rgba(45,53,46,0.08)] transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_26px_70px_rgba(45,53,46,0.12)] sm:p-7 ${plan.featured ? "border-stone-300 bg-stone-700 text-white" : "border-stone-200 bg-white text-stone-900"}`}
          data-polar-tier={plan.id}
        >
          <div class="flex items-center justify-between gap-4">
            <span
              class={`rounded-full px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] ${plan.featured ? "bg-white/10 text-white/85" : "bg-stone-100 text-stone-600"}`}
            >
              {copy.badges[plan.id === "free" ? "free" : plan.id === "standard" ? "standard" : "plus"]}
            </span>
            {#if plan.featured}
              <span
                class="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-white/70"
              >
                {copy.popular}
              </span>
            {/if}
          </div>

          <div
            class={`mt-6 h-1.5 w-14 rounded-full transition-all duration-300 group-hover:w-20 ${plan.featured ? "bg-white/30" : "bg-stone-300"}`}
          ></div>

          <div
            class={`mt-4 text-4xl font-semibold tracking-tight ${plan.featured ? "text-white" : "text-stone-900"}`}
          >
            {plan.price[billingCycle]}
          </div>
          <p
            class={`mt-3 text-sm leading-7 ${plan.featured ? "text-white/70" : "text-stone-600"}`}
          >
              {planCopy[plan.id === "free" ? "free" : plan.id === "standard" ? "standard" : "plus"].description}
          </p>

          <div
            class={`mt-8 mb-3 border-t pt-5 ${plan.featured ? "border-white/10" : "border-stone-200/80"}`}
          >
            <div class="space-y-2">
              {#each plan.details as detail, idx}
                <div
                  class={`grid grid-cols-[1fr_auto] items-center gap-4 text-sm ${plan.featured ? "text-white/85" : "text-stone-700"}`}
                >
                  <span>{detailLabels[idx]}</span>
                  <span
                    class={`font-medium ${plan.featured ? "text-white" : "text-stone-900"}`}
                    >{detail.label === "Support" ? planCopy[plan.id === "free" ? "free" : plan.id === "standard" ? "standard" : "plus"].support : detail.value}</span
                  >
                </div>
              {/each}
            </div>
          </div>

          <button
            type="button"
            on:click={() => startCheckout(plan.id)}
            disabled={checkoutPlanId === plan.id}
            class={`mt-auto inline-flex items-center justify-center rounded-full px-5 py-3 text-sm font-medium transition-all duration-300 ${plan.featured ? "bg-white text-stone-950 hover:bg-stone-100" : "border border-stone-200 bg-stone-50 text-stone-900 hover:-translate-y-0.5 hover:border-stone-300 hover:bg-white"}`}
            data-polar-plan={plan.id}
          >
            {checkoutPlanId === plan.id ? "Opening checkout..." : planCopy[plan.id === "free" ? "free" : plan.id === "standard" ? "standard" : "plus"].button}
          </button>
        </article>
      {/each}
    </div>

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
