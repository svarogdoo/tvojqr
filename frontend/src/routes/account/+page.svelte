<script lang="ts">
  import { apiFetch } from "$lib/api";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { AccountSummary, Invoice } from "$lib/types/admin";
  import type { MenuType } from "$lib/types/projects";
  import { onMount } from "svelte";

  let summary: AccountSummary | null = null;
  let invoices: Invoice[] = [];
  let loading = true;
  let error = "";

  onMount(async () => {
    await refreshSession();
    if ($auth.status !== "authenticated") {
      loading = false;
      return;
    }

    try {
      const [summaryResponse, invoicesResponse] = await Promise.all([
        apiFetch("/api/billing/account-summary"),
        apiFetch("/api/invoices"),
      ]);
      if (!summaryResponse.ok || !invoicesResponse.ok) throw new Error();
      summary = (await summaryResponse.json()) as AccountSummary;
      invoices = (await invoicesResponse.json()) as Invoice[];
    } catch {
      error = "Unable to load your account details right now.";
    } finally {
      loading = false;
    }
  });

  function formatDate(value: string | null | undefined) {
    return value ? new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(`${value.slice(0, 10)}T00:00:00`)) : "Not set";
  }

  function formatMenuTypes(types: Partial<Record<MenuType, number>> | undefined) {
    if (!types) return "No menus";
    const labels = [types.image ? `${types.image} Image Menu${types.image === 1 ? "" : "s"}` : "", types.digital ? `${types.digital} Digital Menu${types.digital === 1 ? "" : "s"}` : ""].filter(Boolean);
    return labels.length ? labels.join(", ") : "No menus";
  }

  async function downloadInvoice(invoice: Invoice) {
    error = "";
    try {
      const response = await apiFetch(`/api/invoices/${invoice.id}/download`);
      if (!response.ok) throw new Error();
      const url = URL.createObjectURL(await response.blob());
      const link = document.createElement("a");
      link.href = url;
      link.download = invoice.originalFileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch {
      error = "This invoice could not be downloaded.";
    }
  }
</script>

<svelte:head><title>Account & billing - HostingQr</title><meta name="robots" content="noindex, nofollow" /></svelte:head>
<Navigation />

<div class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <main class="mx-auto max-w-5xl">
    <div class="mb-8 max-w-2xl">
      <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Account & billing</h1>
      <p class="mt-4 text-base leading-7 text-stone-600">Review your menus, billing schedule, and invoice documents.</p>
    </div>

    {#if $auth.status === "anonymous"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-sm sm:p-10"><h2 class="text-2xl font-semibold">Sign in required</h2><p class="mt-3 text-sm text-stone-600">Sign in to view your account and invoices.</p><button type="button" class="btn-primary mt-6" on:click={() => startGoogleSignIn()}>Sign in</button></section>
    {:else if loading || $auth.status === "loading"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 text-stone-600 shadow-sm">Loading account settings...</section>
    {:else if $auth.status === "authenticated"}
      {#if error}<p class="mb-5 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>{/if}
      <div class="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        <section class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
          <p class="text-xs uppercase tracking-[0.2em] text-stone-500">Account details</p>
          <div class="mt-6 space-y-5"><div><p class="text-sm text-stone-500">Name</p><p class="mt-1 text-lg font-semibold">{$auth.user.displayName}</p></div><div><p class="text-sm text-stone-500">Email</p><p class="mt-1 break-all text-lg font-semibold">{$auth.user.email}</p></div><div><p class="text-sm text-stone-500">Joined</p><p class="mt-1 text-lg font-semibold">{formatDate(summary?.joinedAt)}</p></div></div>
        </section>

        <section class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
          <div class="flex items-start justify-between gap-4"><div><p class="text-xs uppercase tracking-[0.2em] text-stone-500">Your service</p><h2 class="mt-3 text-2xl font-semibold">{formatMenuTypes(summary?.menuTypes)}</h2></div><span class={`rounded-full border px-3 py-1 text-xs font-medium ${summary?.billingActive ? "border-green-200 bg-green-50 text-green-800" : "border-stone-200 bg-stone-100 text-stone-600"}`}>{summary?.billingActive === true ? "Active" : summary?.billingActive === false ? "Inactive" : "Not set"}</span></div>
          <div class="mt-6 grid gap-4 rounded-3xl border border-stone-200 bg-stone-50 p-5 sm:grid-cols-3"><div><p class="text-xs uppercase tracking-wider text-stone-500">Plan</p><p class="mt-1 font-semibold capitalize">{summary?.billingCycle ?? "Not set"}</p></div><div><p class="text-xs uppercase tracking-wider text-stone-500">Last invoice</p><p class="mt-1 font-semibold">{formatDate(summary?.lastInvoiceDate)}</p></div><div><p class="text-xs uppercase tracking-wider text-stone-500">Next invoice</p><p class="mt-1 font-semibold">{formatDate(summary?.nextInvoiceDate)}</p></div></div>
          <p class="mt-5 text-sm leading-6 text-stone-500">Billing details are maintained by HostingQr. Contact support if anything needs to be corrected.</p>
        </section>
      </div>

      <section class="mt-6 rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
        <p class="text-xs uppercase tracking-[0.2em] text-stone-500">Invoices</p><h2 class="mt-2 text-2xl font-semibold">Invoice documents</h2>
        {#if invoices.length === 0}<div class="mt-6 rounded-3xl border border-dashed border-stone-300 bg-stone-50 px-5 py-8 text-center text-sm text-stone-600">No invoices have been uploaded yet.</div>{:else}<div class="mt-6 divide-y divide-stone-100 overflow-hidden rounded-3xl border border-stone-200">{#each invoices as invoice}<div class="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:justify-between"><div><p class="font-semibold">{invoice.originalFileName}</p><p class="mt-1 text-sm text-stone-500">Invoice date: {formatDate(invoice.invoiceDate)}</p></div><button type="button" class="btn-secondary text-sm" on:click={() => downloadInvoice(invoice)}>Download PDF</button></div>{/each}</div>{/if}
      </section>
    {/if}
  </main>
</div>
