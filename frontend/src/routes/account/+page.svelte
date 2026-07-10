<script lang="ts">
  import { goto } from "$app/navigation";
  import { apiFetch } from "$lib/api";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { Entitlement } from "$lib/types/projects";
  import { onMount } from "svelte";

  let entitlement: Entitlement | null = null;
  let loading = true;
  let openingPortal = false;
  let error = "";

  $: planLabel = entitlement ? formatPlan(entitlement.tier) : "No active plan";
  $: planStatus = entitlement?.hasToolAccess ? "Active" : "Inactive";

  onMount(async () => {
    await refreshSession();

    if ($auth.status !== "authenticated") {
      loading = false;
      return;
    }

    await loadEntitlement();
  });

  async function loadEntitlement() {
    loading = true;
    error = "";

    try {
      const response = await apiFetch("/api/billing/entitlement");
      if (response.status === 401) {
        entitlement = null;
        return;
      }

      if (!response.ok) {
        throw new Error(`Entitlement request failed with status ${response.status}`);
      }

      entitlement = (await response.json()) as Entitlement;
    } catch {
      error = "Unable to load your billing status right now.";
    } finally {
      loading = false;
    }
  }

  async function openBillingPortal() {
    if ($auth.status !== "authenticated") {
      startGoogleSignIn();
      return;
    }

    openingPortal = true;
    error = "";

    try {
      const response = await apiFetch("/api/billing/portal", { method: "POST" });
      if (!response.ok) {
        throw new Error(`Billing portal request failed with status ${response.status}`);
      }

      const portal = (await response.json()) as { portalUrl: string };
      window.location.href = portal.portalUrl;
    } catch {
      error = "Unable to open billing right now. Please try again or contact support.";
      openingPortal = false;
    }
  }

  function formatPlan(tier: Entitlement["tier"]) {
    switch (tier) {
      case "admin":
        return "Admin";
      case "free":
        return "Free";
      case "standard":
        return "Standard";
      case "plus":
        return "Plus";
      default:
        return "No active plan";
    }
  }

  function formatDate(value: string | null | undefined) {
    if (!value) {
      return "Not set";
    }

    return new Intl.DateTimeFormat("en", {
      dateStyle: "medium",
    }).format(new Date(value));
  }
</script>

<svelte:head>
  <title>Account & billing - HostingQr</title>
</svelte:head>

<Navigation />

<div class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <main class="mx-auto max-w-5xl">
    <div class="mb-8 max-w-2xl">
      <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Account & billing</h1>
      <p class="mt-4 text-base leading-7 text-stone-600">
        Manage your HostingQr account details and open your secure Polar billing portal.
      </p>
    </div>

    {#if $auth.status === "anonymous"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <h2 class="text-2xl font-semibold tracking-tight text-stone-900">Sign in required</h2>
        <p class="mt-3 max-w-xl text-sm leading-7 text-stone-600">
          Sign in first to view your account and billing settings.
        </p>
        <button type="button" class="mt-6 rounded-full bg-stone-900 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800" on:click={startGoogleSignIn}>
          Sign in
        </button>
      </section>
    {:else if loading || $auth.status === "loading"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 text-sm leading-7 text-stone-600 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        Loading account settings...
      </section>
    {:else if $auth.status === "authenticated"}
      <div class="grid gap-6 lg:grid-cols-[1fr_1.15fr]">
        <section class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-8">
          <p class="text-xs font-medium uppercase tracking-[0.2em] text-stone-500">Account details</p>
          <div class="mt-6 space-y-5">
            <div>
              <p class="text-sm text-stone-500">Name</p>
              <p class="mt-1 text-lg font-semibold text-stone-900">{$auth.user.displayName}</p>
            </div>
            <div>
              <p class="text-sm text-stone-500">Email</p>
              <p class="mt-1 break-all text-lg font-semibold text-stone-900">{$auth.user.email}</p>
            </div>
          </div>
        </section>

        <section class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-8">
          <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <p class="text-xs font-medium uppercase tracking-[0.2em] text-stone-500">Billing</p>
              <h2 class="mt-3 text-2xl font-semibold tracking-tight text-stone-900">{planLabel}</h2>
              <p class="mt-2 text-sm leading-7 text-stone-600">
                Your billing is managed securely by Polar. You can update payment details, view billing information, or cancel your subscription from the billing portal.
              </p>
            </div>
            <span class={`inline-flex rounded-full border px-3 py-1 text-xs font-medium ${entitlement?.hasToolAccess ? "border-[rgba(77,106,83,0.18)] bg-[rgba(236,245,238,0.96)] text-[color:var(--success-strong)]" : "border-stone-200 bg-stone-100 text-stone-600"}`}>
              {planStatus}
            </span>
          </div>

          <div class="mt-6 grid gap-3 rounded-[1.5rem] border border-stone-200 bg-stone-50/70 p-5 text-sm text-stone-600 sm:grid-cols-2">
            <div>
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Access until</p>
              <p class="mt-1 font-medium text-stone-900">{formatDate(entitlement?.endsAt)}</p>
            </div>
            <div>
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Granted manually</p>
              <p class="mt-1 font-medium text-stone-900">{entitlement?.grantedManually ? "Yes" : "No"}</p>
            </div>
          </div>

          {#if error}
            <p class="mt-5 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>
          {/if}

          <div class="mt-6 flex flex-col gap-3 sm:flex-row">
            <button type="button" class="inline-flex justify-center rounded-full bg-stone-900 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800 disabled:cursor-not-allowed disabled:opacity-60" on:click={openBillingPortal} disabled={openingPortal}>
              {openingPortal ? "Opening billing..." : "Manage billing"}
            </button>
            <button type="button" class="inline-flex justify-center rounded-full border border-stone-200 bg-stone-50 px-5 py-3 text-sm font-medium text-stone-900 transition-colors hover:border-stone-300 hover:bg-white" on:click={() => goto("/contact")}>
              Contact support
            </button>
          </div>
        </section>
      </div>
    {/if}
  </main>
</div>
