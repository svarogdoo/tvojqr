<script lang="ts">
  import Navigation from "$lib/components/Navigation.svelte";
  import { apiFetch } from "$lib/api";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { ProjectDetail } from "$lib/types/projects";
  import { onMount } from "svelte";

  let projectId = "";
  let project: ProjectDetail | null = null;
  let loading = true;
  let error = "";

  async function loadProject() {
    loading = true;
    error = "";

    try {
      const response = await apiFetch(`/api/projects/${projectId}`);
      if (response.status === 401) {
        project = null;
        return;
      }

      if (response.status === 404) {
        error = "Project not found.";
        project = null;
        return;
      }

      if (!response.ok) {
        throw new Error(`Project request failed with status ${response.status}`);
      }

      project = (await response.json()) as ProjectDetail;
    } catch {
      error = "Unable to load this project right now.";
    } finally {
      loading = false;
    }
  }

  onMount(async () => {
    projectId = window.location.pathname.split("/").at(-1) ?? "";
    await refreshSession();
    await loadProject();
  });
</script>

<Navigation />

<div class="min-h-screen px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-5xl">
    {#if $auth.status === "anonymous"}
      <section class="rounded-[2rem] border border-black/8 bg-[rgba(220,228,216,0.92)] p-8 shadow-[0_20px_50px_rgba(45,53,46,0.08)] sm:p-10">
        <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Sign in to open this project</h1>
        <p class="mt-5 max-w-md text-base leading-7 text-stone-600">Your project settings are private to your account.</p>
        <button class="mt-8 btn-primary" on:click={startGoogleSignIn}>Continue with Google</button>
      </section>
    {:else if loading}
      <section class="rounded-[2rem] border border-stone-200 bg-white/96 p-8 text-stone-600 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        Loading project...
      </section>
    {:else if error}
      <section class="rounded-[2rem] border border-[color:var(--error-soft)] bg-[color:var(--error-soft)] p-8 text-[color:var(--error-strong)] shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        {error}
      </section>
    {:else if project}
      <div class="grid gap-8 lg:grid-cols-[0.8fr_1.2fr] lg:items-start">
        <section class="rounded-[2rem] border border-black/8 bg-[rgba(220,228,216,0.92)] p-8 shadow-[0_20px_50px_rgba(45,53,46,0.08)] sm:p-10">
          <p class="mb-4 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">Project settings</p>
          <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">{project.name}</h1>
          <p class="mt-5 max-w-md text-base leading-7 text-stone-600">This is the first project detail shell. Next we can add editable fields for name, slug, uploads, languages, preview, and final save.</p>
        </section>

        <section class="rounded-[2rem] border border-black/8 bg-white/96 p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
          <div class="grid gap-5">
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Project name</p>
              <p class="mt-2 text-lg font-semibold text-stone-900">{project.name}</p>
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Active slug</p>
              <p class="mt-2 text-lg font-semibold text-stone-900">hostingqr.com/{project.slug}</p>
            </div>
            <div class="rounded-[1.5rem] border border-dashed border-stone-300 bg-stone-50 px-6 py-6 text-sm leading-7 text-stone-600">
              Uploads, language variants, preview, and save controls are the next step on this page.
            </div>
          </div>
        </section>
      </div>
    {/if}
  </div>
</div>

<svelte:head>
  <title>Project - HostingQr</title>
</svelte:head>
