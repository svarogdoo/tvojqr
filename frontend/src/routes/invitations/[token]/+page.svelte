<script lang="ts">
  import { goto } from "$app/navigation";
  import { apiFetch } from "$lib/api";
  import Navigation from "$lib/components/Navigation.svelte";
  import { auth, refreshSession, startGoogleSignInWithReturnPath } from "$lib/stores/auth";
  import { onMount } from "svelte";

  type InvitationDetails = {
    projectName: string;
    expiresAt: string;
    isAvailable: boolean;
  };

  let token = "";
  let invitation: InvitationDetails | null = null;
  let loading = true;
  let accepting = false;
  let error = "";

  onMount(async () => {
    token = window.location.pathname.split("/").at(-1) ?? "";
    await refreshSession();
    try {
      const response = await apiFetch(`/api/invitations/${encodeURIComponent(token)}`);
      if (response.status === 404) {
        error = "This invitation does not exist.";
        return;
      }
      if (!response.ok) throw new Error();
      invitation = (await response.json()) as InvitationDetails;
    } catch {
      error = "The invitation could not be loaded.";
    } finally {
      loading = false;
    }
  });

  function signIn() {
    startGoogleSignInWithReturnPath(`/invitations/${token}`);
  }

  async function acceptInvitation() {
    accepting = true;
    error = "";
    try {
      const response = await apiFetch(`/api/invitations/${encodeURIComponent(token)}/accept`, { method: "POST" });
      if (!response.ok) {
        const problem = await response.json().catch(() => null) as { message?: string } | null;
        throw new Error(problem?.message ?? "The invitation could not be accepted.");
      }
      await goto("/dashboard");
    } catch (caught) {
      error = caught instanceof Error ? caught.message : "The invitation could not be accepted.";
    } finally {
      accepting = false;
    }
  }

  function formatDate(value: string) {
    return new Intl.DateTimeFormat("en", { dateStyle: "long" }).format(new Date(value));
  }
</script>

<svelte:head><title>Menu invitation - HostingQr</title><meta name="robots" content="noindex, nofollow" /></svelte:head>
<Navigation />

<div class="min-h-screen bg-[rgba(243,244,246,0.98)] px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <main class="mx-auto max-w-2xl">
    <section class="rounded-[2rem] border border-stone-200 bg-white p-7 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
      <p class="text-sm font-medium uppercase tracking-[0.22em] text-stone-500">Client invitation</p>
      {#if loading}
        <p class="mt-6 text-stone-600">Loading invitation...</p>
      {:else if invitation}
        <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900">Your menu is ready</h1>
        <p class="mt-5 text-base leading-7 text-stone-600">You have been invited to manage <strong class="text-stone-900">{invitation.projectName}</strong> from your HostingQr account.</p>
        <div class="mt-6 rounded-3xl border border-stone-200 bg-stone-50 p-5 text-sm text-stone-600"><p>This secure invitation expires on <strong class="text-stone-900">{formatDate(invitation.expiresAt)}</strong>.</p></div>
        {#if !invitation.isAvailable}
          <p class="mt-6 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">This invitation has expired or has already been used.</p>
        {:else if $auth.status === "authenticated"}
          <p class="mt-6 text-sm text-stone-600">Signed in as <strong>{$auth.user.email}</strong>. This must match the invited email.</p>
          <button type="button" class="btn-primary mt-6 w-full sm:w-auto" disabled={accepting} on:click={acceptInvitation}>{accepting ? "Accepting..." : "Accept menu invitation"}</button>
        {:else}
          <p class="mt-6 text-sm text-stone-600">Sign in with the Google account that received this invitation.</p>
          <button type="button" class="btn-primary mt-6 w-full sm:w-auto" on:click={signIn}>Continue with Google</button>
        {/if}
      {/if}
      {#if error}<p class="mt-6 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>{/if}
    </section>
  </main>
</div>
