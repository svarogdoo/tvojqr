<script lang="ts">
  import { goto } from "$app/navigation";
  import { apiFetch } from "$lib/api";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { AdminClient, AdminMenu, Invoice } from "$lib/types/admin";
  import type { Entitlement, MenuType } from "$lib/types/projects";
  import { onMount } from "svelte";

  type AdminOverview = {
    totalAccounts: number;
    totalViews: number;
    accountsByTier: Record<string, number>;
  };

  const tierLabels: Record<string, string> = { none: "No plan", admin: "Admin", free: "Free", standard: "Standard", plus: "Plus" };
  const tierOrder = ["none", "admin", "free", "standard", "plus"];

  let entitlement: Entitlement | null = null;
  let overview: AdminOverview | null = null;
  let clients: AdminClient[] = [];
  let menus: AdminMenu[] = [];
  let loading = true;
  let error = "";
  let selectedClient: AdminClient | null = null;
  let clientInvoices: Invoice[] = [];
  let billingCycle: "monthly" | "annual" = "monthly";
  let nextInvoiceDate = "";
  let billingActive = true;
  let invoiceDate = "";
  let invoiceFile: File | null = null;
  let savingBilling = false;
  let uploadingInvoice = false;
  let inviteProject: AdminMenu | null = null;
  let inviteEmail = "";
  let sendingInvite = false;
  let panelMessage = "";
  let invoiceRequestId = 0;

  $: isAdmin = entitlement?.tier === "admin" && entitlement.isActive;

  onMount(async () => {
    await refreshSession();
    if ($auth.status !== "authenticated") {
      loading = false;
      return;
    }

    try {
      const entitlementResponse = await apiFetch("/api/billing/entitlement");
      if (!entitlementResponse.ok) throw new Error();
      entitlement = (await entitlementResponse.json()) as Entitlement;
      if (entitlement.tier === "admin" && entitlement.isActive) await loadAdminData();
    } catch {
      error = "Unable to load the owner workspace right now.";
    } finally {
      loading = false;
    }
  });

  async function loadAdminData() {
    const [overviewResponse, clientsResponse, menusResponse] = await Promise.all([
      apiFetch("/api/admin/overview"),
      apiFetch("/api/admin/clients"),
      apiFetch("/api/admin/menus"),
    ]);
    if (!overviewResponse.ok || !clientsResponse.ok || !menusResponse.ok) throw new Error();
    overview = (await overviewResponse.json()) as AdminOverview;
    clients = (await clientsResponse.json()) as AdminClient[];
    menus = (await menusResponse.json()) as AdminMenu[];
  }

  async function openClient(client: AdminClient) {
    selectedClient = client;
    clientInvoices = [];
    billingCycle = client.billingCycle ?? "monthly";
    nextInvoiceDate = client.nextInvoiceDate ?? "";
    billingActive = client.billingActive ?? true;
    invoiceDate = new Date().toISOString().slice(0, 10);
    invoiceFile = null;
    panelMessage = "";
    await refreshClientInvoices(client);
  }

  async function refreshClientInvoices(client: AdminClient) {
    const requestId = ++invoiceRequestId;
    const response = await apiFetch(`/api/admin/clients/${client.userId}/invoices`);
    if (requestId !== invoiceRequestId || selectedClient?.userId !== client.userId) return;
    if (!response.ok) {
      clientInvoices = [];
      panelMessage = "Invoices could not be loaded.";
      return;
    }
    clientInvoices = (await response.json()) as Invoice[];
  }

  async function saveBilling() {
    if (!selectedClient) return;
    savingBilling = true;
    panelMessage = "";
    try {
      const response = await apiFetch(`/api/admin/clients/${selectedClient.userId}/billing`, {
        method: "PUT",
        body: JSON.stringify({ billingCycle, nextInvoiceDate: nextInvoiceDate || null, isActive: billingActive }),
      });
      if (!response.ok) throw new Error();
      panelMessage = "Billing details saved.";
      await loadAdminData();
      const refreshed = clients.find((client) => client.userId === selectedClient?.userId);
      if (refreshed) selectedClient = refreshed;
    } catch {
      panelMessage = "Billing details could not be saved.";
    } finally {
      savingBilling = false;
    }
  }

  async function uploadInvoice() {
    if (!selectedClient || !invoiceFile || !invoiceDate) return;
    uploadingInvoice = true;
    panelMessage = "";
    const body = new FormData();
    body.append("invoiceDate", invoiceDate);
    body.append("file", invoiceFile);
    try {
      const response = await apiFetch(`/api/admin/clients/${selectedClient.userId}/invoices`, { method: "POST", body });
      if (!response.ok) throw new Error();
      invoiceFile = null;
      panelMessage = "Invoice uploaded.";
      await refreshClientInvoices(selectedClient);
      await loadAdminData();
    } catch {
      panelMessage = "Upload failed. Choose a valid PDF up to 10 MB.";
    } finally {
      uploadingInvoice = false;
    }
  }

  async function deleteInvoice(invoiceId: string) {
    if (!selectedClient || !window.confirm("Delete this invoice?")) return;
    const response = await apiFetch(`/api/admin/invoices/${invoiceId}`, { method: "DELETE" });
    if (response.ok) {
      panelMessage = "Invoice deleted.";
      await refreshClientInvoices(selectedClient);
      await loadAdminData();
    } else {
      panelMessage = "Invoice could not be deleted.";
    }
  }

  async function downloadInvoice(invoice: Invoice) {
    const response = await apiFetch(`/api/invoices/${invoice.id}/download`);
    if (!response.ok) {
      panelMessage = "Invoice could not be downloaded.";
      return;
    }
    const url = URL.createObjectURL(await response.blob());
    const link = document.createElement("a");
    link.href = url;
    link.download = invoice.originalFileName;
    link.click();
    URL.revokeObjectURL(url);
  }

  function openInvite(menu: AdminMenu) {
    inviteProject = menu;
    inviteEmail = "";
    panelMessage = "";
  }

  async function sendInvite() {
    if (!inviteProject || !inviteEmail.trim()) return;
    sendingInvite = true;
    panelMessage = "";
    try {
      const response = await apiFetch(`/api/admin/projects/${inviteProject.projectId}/invitations`, {
        method: "POST",
        body: JSON.stringify({ email: inviteEmail.trim(), expiresInDays: 7 }),
      });
      if (!response.ok) {
        const problem = await response.json().catch(() => null) as { detail?: string; message?: string } | null;
        throw new Error(problem?.detail ?? problem?.message ?? "Invitation could not be sent.");
      }
      panelMessage = `Invitation sent to ${inviteEmail.trim()}.`;
      inviteProject = null;
      inviteEmail = "";
    } catch (caught) {
      panelMessage = caught instanceof Error ? caught.message : "Invitation could not be sent.";
    } finally {
      sendingInvite = false;
    }
  }

  function formatDate(value: string | null | undefined) {
    return value ? new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(`${value.slice(0, 10)}T00:00:00`)) : "Not set";
  }

  function formatMenuTypes(types: Partial<Record<MenuType, number>>) {
    const labels = [types.image ? `${types.image} Image` : "", types.digital ? `${types.digital} Digital` : ""].filter(Boolean);
    return labels.length ? labels.join(", ") : "No menus";
  }
</script>

<svelte:head><title>Owner workspace - HostingQr</title><meta name="robots" content="noindex, nofollow" /></svelte:head>
<Navigation />

<div class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <main class="mx-auto max-w-7xl">
    <div class="mb-8 max-w-3xl">
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">Owner workspace</p>
      <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Clients, menus and invoices</h1>
      <p class="mt-4 text-base leading-7 text-stone-600">Onboard clients, maintain billing dates, and open any menu from one private workspace.</p>
    </div>

    {#if $auth.status === "anonymous"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-sm"><h2 class="text-2xl font-semibold">Sign in required</h2><button type="button" class="btn-primary mt-6" on:click={() => startGoogleSignIn()}>Sign in</button></section>
    {:else if loading || $auth.status === "loading"}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 text-stone-600 shadow-sm">Loading owner workspace...</section>
    {:else if error}
      <section class="rounded-[2rem] border border-[color:var(--error-soft)] bg-[color:var(--error-soft)] p-8 text-[color:var(--error-strong)]">{error}</section>
    {:else if !isAdmin}
      <section class="rounded-[2rem] border border-stone-200 bg-white p-8 shadow-sm"><h2 class="text-2xl font-semibold">Owner access only</h2></section>
    {:else if overview}
      <section class="grid gap-5 sm:grid-cols-2">
        <div class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8"><p class="text-xs uppercase tracking-[0.2em] text-stone-500">Total accounts</p><p class="mt-4 text-5xl font-semibold">{overview.totalAccounts.toLocaleString()}</p></div>
        <div class="rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8"><p class="text-xs uppercase tracking-[0.2em] text-stone-500">Total views</p><p class="mt-4 text-5xl font-semibold">{overview.totalViews.toLocaleString()}</p></div>
      </section>

      <section class="mt-6 rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
        <p class="text-xs uppercase tracking-[0.2em] text-stone-500">Accounts by tier</p>
        <div class="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">{#each tierOrder as tier}<div class="rounded-3xl border border-stone-200 bg-stone-50 p-5"><p class="text-sm text-stone-600">{tierLabels[tier]}</p><p class="mt-2 text-3xl font-semibold">{overview.accountsByTier[tier] ?? 0}</p></div>{/each}</div>
      </section>

      <section class="mt-6 overflow-hidden rounded-[2rem] border border-stone-200 bg-white shadow-sm">
        <div class="p-6 sm:p-8"><p class="text-xs uppercase tracking-[0.2em] text-stone-500">Clients</p><h2 class="mt-2 text-2xl font-semibold">Account billing</h2></div>
        <div class="overflow-x-auto"><table class="w-full min-w-[960px] text-left text-sm"><thead class="bg-stone-50 text-xs uppercase tracking-wider text-stone-500"><tr><th class="px-6 py-4">Client</th><th class="px-4 py-4">Menus</th><th class="px-4 py-4">Joined</th><th class="px-4 py-4">Cycle</th><th class="px-4 py-4">Last invoice</th><th class="px-4 py-4">Next invoice</th><th class="px-6 py-4"></th></tr></thead><tbody class="divide-y divide-stone-100">
          {#each clients as client}<tr><td class="px-6 py-4"><p class="font-semibold text-stone-900">{client.displayName}</p><p class="text-stone-500">{client.email}</p></td><td class="px-4 py-4">{formatMenuTypes(client.menuTypes)}</td><td class="px-4 py-4">{formatDate(client.joinedAt)}</td><td class="px-4 py-4 capitalize">{client.billingCycle ?? "Not set"}</td><td class="px-4 py-4">{formatDate(client.lastInvoiceDate)}</td><td class="px-4 py-4">{formatDate(client.nextInvoiceDate)}</td><td class="px-6 py-4 text-right"><button type="button" class="btn-secondary text-sm" on:click={() => openClient(client)}>Manage</button></td></tr>{/each}
        </tbody></table></div>
      </section>

      {#if selectedClient}
        <section class="mt-6 rounded-[2rem] border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
          <div class="flex items-start justify-between gap-4"><div><p class="text-xs uppercase tracking-[0.2em] text-stone-500">Manage client</p><h2 class="mt-2 text-2xl font-semibold">{selectedClient.displayName}</h2><p class="text-sm text-stone-500">{selectedClient.email}</p></div><button type="button" class="btn-secondary text-sm" on:click={() => selectedClient = null}>Close</button></div>
          <div class="mt-6 grid gap-6 lg:grid-cols-2">
            <div class="rounded-3xl border border-stone-200 bg-stone-50 p-5"><h3 class="font-semibold">Billing details</h3><div class="mt-4 grid gap-4 sm:grid-cols-2"><label class="text-sm text-stone-600">Cycle<select bind:value={billingCycle} class="mt-2 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900"><option value="monthly">Monthly</option><option value="annual">Annual</option></select></label><label class="text-sm text-stone-600">Next invoice<input type="date" bind:value={nextInvoiceDate} class="mt-2 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900" /></label></div><label class="mt-4 flex items-center gap-3 text-sm text-stone-700"><input type="checkbox" bind:checked={billingActive} class="h-4 w-4" /> Account billing is active</label><button type="button" class="btn-primary mt-5 text-sm" disabled={savingBilling} on:click={saveBilling}>{savingBilling ? "Saving..." : "Save billing"}</button></div>
            <div class="rounded-3xl border border-stone-200 bg-stone-50 p-5"><h3 class="font-semibold">Upload invoice</h3><div class="mt-4 grid gap-4"><label class="text-sm text-stone-600">Invoice date<input type="date" bind:value={invoiceDate} class="mt-2 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900" /></label><label class="text-sm text-stone-600">PDF file<input type="file" accept="application/pdf,.pdf" class="mt-2 block w-full text-sm" on:change={(event) => invoiceFile = event.currentTarget.files?.[0] ?? null} /></label></div><button type="button" class="btn-primary mt-5 text-sm" disabled={!invoiceFile || uploadingInvoice} on:click={uploadInvoice}>{uploadingInvoice ? "Uploading..." : "Upload invoice"}</button></div>
          </div>
          {#if panelMessage}<p class="mt-4 rounded-2xl bg-stone-100 px-4 py-3 text-sm text-stone-700">{panelMessage}</p>{/if}
          <div class="mt-6"><h3 class="font-semibold">Invoices</h3>{#if clientInvoices.length === 0}<p class="mt-3 text-sm text-stone-500">No invoices uploaded yet.</p>{:else}<div class="mt-3 divide-y divide-stone-100 rounded-2xl border border-stone-200">{#each clientInvoices as invoice}<div class="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between"><div><p class="font-medium">{invoice.originalFileName}</p><p class="text-sm text-stone-500">{formatDate(invoice.invoiceDate)}</p></div><div class="flex gap-2"><button type="button" class="btn-secondary text-sm" on:click={() => downloadInvoice(invoice)}>Download</button><button type="button" class="btn-secondary text-sm text-[color:var(--error-strong)]" on:click={() => deleteInvoice(invoice.id)}>Delete</button></div></div>{/each}</div>{/if}</div>
        </section>
      {/if}

      <section class="mt-6 overflow-hidden rounded-[2rem] border border-stone-200 bg-white shadow-sm">
        <div class="p-6 sm:p-8"><p class="text-xs uppercase tracking-[0.2em] text-stone-500">All menus</p><h2 class="mt-2 text-2xl font-semibold">Open or assign any menu</h2></div>
        <div class="overflow-x-auto"><table class="w-full min-w-[880px] text-left text-sm"><thead class="bg-stone-50 text-xs uppercase tracking-wider text-stone-500"><tr><th class="px-6 py-4">Menu</th><th class="px-4 py-4">Client</th><th class="px-4 py-4">Type</th><th class="px-4 py-4">Status</th><th class="px-6 py-4"></th></tr></thead><tbody class="divide-y divide-stone-100">{#each menus as menu}<tr><td class="px-6 py-4"><p class="font-semibold">{menu.name}</p><p class="text-stone-500">/{menu.slug}</p></td><td class="px-4 py-4"><p>{menu.clientDisplayName}</p><p class="text-stone-500">{menu.clientEmail}</p></td><td class="px-4 py-4">{menu.menuType === "digital" ? "Digital Menu" : "Image Menu"}</td><td class="px-4 py-4 capitalize">{menu.status}</td><td class="px-6 py-4"><div class="flex justify-end gap-2"><button type="button" class="btn-secondary text-sm" on:click={() => goto(`/dashboard/projects/${menu.projectId}`)}>Edit</button>{#if menu.clientUserId === $auth.user.id}<button type="button" class="btn-primary text-sm" on:click={() => openInvite(menu)}>Assign</button>{/if}</div></td></tr>{/each}</tbody></table></div>
      </section>

      {#if inviteProject}
        <section class="mt-6 rounded-[2rem] border border-stone-300 bg-[rgba(220,228,216,0.72)] p-6 shadow-sm sm:p-8"><p class="text-xs uppercase tracking-[0.2em] text-stone-600">Assign menu</p><h2 class="mt-2 text-2xl font-semibold">Invite a client to {inviteProject.name}</h2><p class="mt-2 text-sm text-stone-600">The link expires in seven days and must be accepted with this Google email. You will keep owner-level edit access.</p><div class="mt-5 flex flex-col gap-3 sm:flex-row"><input type="email" bind:value={inviteEmail} placeholder="client@example.com" class="min-w-0 flex-1 rounded-2xl border border-stone-200 bg-white px-4 py-3" /><button type="button" class="btn-primary" disabled={sendingInvite || !inviteEmail.trim()} on:click={sendInvite}>{sendingInvite ? "Sending..." : "Send invitation"}</button><button type="button" class="btn-secondary" on:click={() => inviteProject = null}>Cancel</button></div></section>
      {/if}
    {/if}
  </main>
</div>
