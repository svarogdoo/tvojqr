<script lang="ts">
  import { apiFetch } from "$lib/api";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { Entitlement } from "$lib/types/projects";
  import { onMount } from "svelte";

  type AdminOverview = {
    totalAccounts: number;
    totalViews: number;
    accountsByTier: Record<string, number>;
  };

  const tierLabels: Record<string, string> = {
    none: "No plan",
    admin: "Admin",
    free: "Free",
    standard: "Standard",
    plus: "Plus",
  };

  const tierOrder = ["none", "admin", "free", "standard", "plus"];

  let entitlement: Entitlement | null = null;
  let overview: AdminOverview | null = null;
  let loading = true;
  let error = "";

  $: isAdmin = entitlement?.tier === "admin" && entitlement.isActive;

  onMount(async () => {
    await refreshSession();

    if ($auth.status !== "authenticated") {
      loading = false;
      return;
    }

    await loadEntitlement();
    if (isAdmin) {
      await loadOverview();
    }

    loading = false;
  });

  async function loadEntitlement() {
    try {
      const response = await apiFetch("/api/billing/entitlement");
      if (!response.ok) {
        throw new Error(`Entitlement request failed with status ${response.status}`);
      }

      entitlement = (await response.json()) as Entitlement;
    } catch {
      error = "Unable to verify admin access right now.";
    }
  }

  async function loadOverview() {
    try {
      const response = await apiFetch("/api/admin/overview");
      if (!response.ok) {
        throw new Error(`Admin overview request failed with status ${response.status}`);
      }

      overview = (await response.json()) as AdminOverview;
    } catch {
      error = "Unable to load admin overview right now.";
    }
  }

  function formatNumber(value: number | undefined) {
    return (value ?? 0).toLocaleString();
  }
</script>

<svelte:head>
  <title>Admin overview - HostingQr</title>
</svelte:head>

<Navigation />

<div class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <main class="mx-auto max-w-6xl">
    <div class="mb-8 max-w-2xl">
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">Admin</p>
      <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Overview</h1>
      <p class="mt-4 text-base leading-7 text-stone-600">
        A simple snapshot of accounts, views, and plan distribution.
      </p>
    </div>

    {#if $auth.status === "anonymous"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <h2 class="text-2xl font-semibold tracking-tight text-stone-900">Sign in required</h2>
        <p class="mt-3 max-w-xl text-sm leading-7 text-stone-600">Sign in with an admin account to view this page.</p>
        <button type="button" class="mt-6 rounded-full bg-stone-900 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800" on:click={startGoogleSignIn}>
          Sign in
        </button>
      </section>
    {:else if loading || $auth.status === "loading"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 text-sm leading-7 text-stone-600 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        Loading admin overview...
      </section>
    {:else if error}
      <section class="rounded-[2rem] border border-[color:var(--error-soft)] bg-[color:var(--error-soft)] p-8 text-sm leading-7 text-[color:var(--error-strong)] shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        {error}
      </section>
    {:else if !isAdmin}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <h2 class="text-2xl font-semibold tracking-tight text-stone-900">Admin access only</h2>
        <p class="mt-3 max-w-xl text-sm leading-7 text-stone-600">This overview is only available to admin accounts.</p>
      </section>
    {:else if overview}
      <section class="grid gap-5 md:grid-cols-2">
        <div class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-8">
          <p class="text-xs font-medium uppercase tracking-[0.2em] text-stone-500">Total accounts</p>
          <p class="mt-4 text-5xl font-semibold tracking-tight text-stone-900">{formatNumber(overview.totalAccounts)}</p>
        </div>
        <div class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-8">
          <p class="text-xs font-medium uppercase tracking-[0.2em] text-stone-500">Total views</p>
          <p class="mt-4 text-5xl font-semibold tracking-tight text-stone-900">{formatNumber(overview.totalViews)}</p>
        </div>
      </section>

      <section class="mt-6 rounded-[2rem] border border-stone-200 bg-white p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-8">
        <div class="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p class="text-xs font-medium uppercase tracking-[0.2em] text-stone-500">Accounts by tier</p>
            <h2 class="mt-3 text-2xl font-semibold tracking-tight text-stone-900">Current plan distribution</h2>
          </div>
        </div>

        <div class="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          {#each tierOrder as tier}
            <div class="rounded-[1.5rem] border border-stone-200 bg-stone-50/70 p-5">
              <p class="text-sm font-medium text-stone-600">{tierLabels[tier]}</p>
              <p class="mt-3 text-3xl font-semibold tracking-tight text-stone-900">{formatNumber(overview.accountsByTier[tier])}</p>
            </div>
          {/each}
        </div>
      </section>
    {/if}
  </main>
</div>
