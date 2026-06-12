<script lang="ts">
  import footballBg from "$lib/assets/football-bg.jpg";
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";

  type BillingCycle = "monthly" | "annual";

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
      id: "world-cup",
      badge: "World Cup",
      featured: false,
      price: {
        monthly: "$15 / month",
        annual: "$150 / year",
      },
      description:
        "Need your menu in many languages or to change prices quickly?",
      button: "Choose World Cup",
      details: [
        { label: "Projects", value: "2" },
        { label: "Menus", value: "2" },
        { label: "Languages", value: "5" },
        { label: "Uploaded files", value: "20 total" },
        { label: "Traffic", value: "50 GB" },
        { label: "Support", value: "Priority" },
      ],
    },
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
        monthly: "$9 / month",
        annual: "$90 / year",
      },
      description:
        "Great for small restaurants with simple needs. You can always upgrade later.",
      button: "Choose standard",
      details: [
        { label: "Projects", value: "1" },
        { label: "Menus", value: "2" },
        { label: "Languages", value: "2" },
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
        monthly: "$19 / month",
        annual: "$190 / year",
      },
      description:
        "You need more? This plan is for you. Want to go even bigger? Contact us!",
      button: "Choose plus",
      details: [
        { label: "Projects", value: "5" },
        { label: "Menus", value: "5" },
        { label: "Languages", value: "5" },
        { label: "Uploaded files", value: "25 total" },
        { label: "Traffic", value: "100 GB" },
        { label: "Support", value: "Priority" },
      ],
    },
  ];

  let billingCycle: BillingCycle = "monthly";

  function setBillingCycle(nextCycle: BillingCycle) {
    billingCycle = nextCycle;
  }
</script>

<svelte:head>
  <title>Pricing - HostingQr</title>
  <meta
    name="description"
    content="Choose a HostingQr plan and preview simple or multilanguage examples before checkout."
  />
</svelte:head>

<Navigation />

<main
  class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8"
>
  <section class="mx-auto max-w-6xl">
    <div
      class="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between"
    >
      <div class="max-w-2xl">
        <p
          class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500"
        >
          Pricing
        </p>
        <h1
          class="mt-4 text-5xl font-semibold tracking-tight text-stone-900 sm:text-6xl"
        >
          Simple plans, clear choice
        </h1>
        <p class="mt-5 max-w-xl text-base leading-7 text-stone-600 sm:text-lg">
          Pick the option that fits your page, then we’ll follow up or wire
          Polar later.
        </p>
      </div>

      <div class="flex items-center justify-start lg:justify-end">
        <div
          class="w-full max-w-sm rounded-full border border-stone-200 bg-white p-1 shadow-sm"
        >
          <div class="grid grid-cols-2 gap-1">
            <button
              type="button"
              on:click={() => setBillingCycle("monthly")}
              class={`rounded-full px-4 py-2 text-sm font-medium transition-all ${billingCycle === "monthly" ? "bg-stone-900 text-white shadow-sm" : "text-stone-600 hover:text-stone-900"}`}
            >
              Monthly
            </button>
            <button
              type="button"
              on:click={() => setBillingCycle("annual")}
              class={`rounded-full px-4 py-2 text-sm font-medium transition-all ${billingCycle === "annual" ? "bg-stone-900 text-white shadow-sm" : "text-stone-600 hover:text-stone-900"}`}
            >
              Annual
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-12 grid gap-6 lg:grid-cols-4 lg:items-stretch">
      {#each plans as plan}
        <article
          id={plan.id}
          style={plan.id === "world-cup"
            ? `background-color: #ffffff; background-image: linear-gradient(rgba(255,255,255,0.5), rgba(255,255,255,0.5)), url(${footballBg}); background-size: cover; background-position: center;`
            : undefined}
          class={`group flex h-full flex-col rounded-[2.25rem] border p-6 shadow-[0_18px_50px_rgba(45,53,46,0.08)] transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_26px_70px_rgba(45,53,46,0.12)] sm:p-7 ${plan.featured ? "border-stone-300 bg-stone-700 text-white" : plan.id === "world-cup" ? "border-stone-300 text-stone-900 shadow-[0_18px_50px_rgba(0,0,0,0.08)]" : "border-stone-200 bg-white text-stone-900"}`}
          data-polar-tier={plan.id}
        >
          <div class="flex items-center justify-between gap-4">
            <span
              class={`rounded-full px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] ${plan.id === "world-cup" ? "bg-stone-700 text-white" : plan.featured ? "bg-white/10 text-white/85" : "bg-stone-100 text-stone-600"}`}
            >
              {plan.badge}
            </span>
            {#if plan.featured}
              <span
                class="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-white/70"
              >
                Popular
              </span>
            {:else if plan.id === "world-cup"}
              <span class="h-8 w-8"></span>
            {/if}
          </div>

          <div
            class={`mt-6 h-1.5 w-14 rounded-full transition-all duration-300 group-hover:w-20 ${plan.featured ? "bg-white/30" : plan.id === "world-cup" ? "bg-stone-500/70" : "bg-stone-300"}`}
          ></div>

          <div
            class={`mt-4 text-4xl font-semibold tracking-tight ${plan.featured ? "text-white" : "text-stone-900"}`}
          >
            {plan.price[billingCycle]}
          </div>
          <p
            class={`mt-3 text-sm leading-7 ${plan.featured ? "text-white/70" : "text-stone-600"}`}
          >
            {plan.description}
          </p>

          <div
            class={`mt-8 mb-3 border-t pt-5 ${plan.featured ? "border-white/10" : "border-stone-200/80"}`}
          >
            <div class="space-y-2">
              {#each plan.details as detail}
                <div
                  class={`grid grid-cols-[1fr_auto] items-center gap-4 text-sm ${plan.featured ? "text-white/85" : "text-stone-700"}`}
                >
                  <span>{detail.label}</span>
                  <span
                    class={`font-medium ${plan.featured ? "text-white" : "text-stone-900"}`}
                    >{detail.value}</span
                  >
                </div>
              {/each}
            </div>
          </div>

          <button
            type="button"
            class={`mt-auto inline-flex items-center justify-center rounded-full px-5 py-3 text-sm font-medium transition-all duration-300 ${plan.featured ? "bg-white text-stone-950 hover:bg-stone-100" : plan.id === "world-cup" ? "border border-stone-300 bg-white text-stone-900 hover:-translate-y-0.5 hover:border-stone-400 hover:bg-stone-50" : "border border-stone-200 bg-stone-50 text-stone-900 hover:-translate-y-0.5 hover:border-stone-300 hover:bg-white"}`}
            data-polar-plan={plan.id}
          >
            {plan.button}
          </button>
        </article>
      {/each}
    </div>

    <div class="mx-auto mt-10 max-w-3xl text-center">
      <p class="text-sm text-stone-600">
        <span class="font-medium text-stone-900">Custom plan</span> available if
        you need something tailored.
        <a
          href="/contact"
          class="inline-flex rounded-full bg-stone-200 px-2.5 py-1 font-medium text-stone-800 transition-colors hover:bg-stone-300"
          >Contact us</a
        >.
      </p>
    </div>

    <blockquote class="mx-auto mt-10 max-w-3xl text-center">
      <p
        class="text-2xl font-semibold tracking-tight text-stone-900 sm:text-[2rem]"
      >
        Need the menu translated or redesigned?
      </p>
      <p class="mt-3 text-sm leading-7 text-stone-600 sm:text-base">
        We can do that too. Just click below!
      </p>
    </blockquote>

    <div class="mt-6 text-center">
      <a
        href="/contact"
        class="inline-flex rounded-full bg-stone-200 px-2.5 py-1 font-medium text-stone-800 transition-colors hover:bg-stone-300"
        >Contact us</a
      >
    </div>
  </section>
</main>

<Footer />
