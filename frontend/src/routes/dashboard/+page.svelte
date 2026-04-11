<script lang="ts">
  import Navigation from "$lib/components/Navigation.svelte";
  import { apiFetch } from "$lib/api";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type { ProjectListItem } from "$lib/types/projects";
  import { onMount } from "svelte";

  let projects: ProjectListItem[] = [];
  let loadingProjects = true;
  let projectError = "";

  async function loadProjects() {
    loadingProjects = true;
    projectError = "";

    try {
      const response = await apiFetch("/api/projects");
      if (response.status === 401) {
        projects = [];
        return;
      }

      if (!response.ok) {
        throw new Error(`Project request failed with status ${response.status}`);
      }

      projects = (await response.json()) as ProjectListItem[];
    } catch {
      projectError = "Unable to load projects right now.";
    } finally {
      loadingProjects = false;
    }
  }

  onMount(async () => {
    await refreshSession();
    await loadProjects();
  });
</script>

<Navigation />

<div class="min-h-screen px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-6xl">
    <div class="grid gap-8 lg:grid-cols-[0.75fr_1.25fr] lg:items-start">
      <section class="rounded-[2rem] border border-black/8 bg-[rgba(220,228,216,0.92)] p-8 shadow-[0_20px_50px_rgba(45,53,46,0.08)] sm:p-10">
        {#if $auth.status === "authenticated"}
          <p class="mb-4 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">
            Dashboard
          </p>
          <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
            Welcome back, {$auth.user.displayName}
          </h1>
          <p class="mt-5 max-w-md text-base leading-7 text-stone-600">
            Choose one of your existing projects or create a new one to keep building your public QR page.
          </p>
        {:else if $auth.status === "loading"}
          <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">Checking your session</h1>
          <p class="mt-5 max-w-md text-base leading-7 text-stone-600">Please wait while we look for your current login.</p>
        {:else}
          <p class="mb-4 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">
            Dashboard
          </p>
          <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
            Sign in to manage your projects
          </h1>
          <p class="mt-5 max-w-md text-base leading-7 text-stone-600">
            Your account dashboard will show every project you own so you can open one, adjust settings, and continue editing later.
          </p>
          <button class="mt-8 btn-primary" on:click={startGoogleSignIn}>Continue with Google</button>
        {/if}
      </section>

      <section class="rounded-[2rem] border border-black/8 bg-white/96 p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <div class="flex items-center justify-between gap-4">
          <div>
            <h2 class="text-2xl font-semibold tracking-tight text-stone-900">Your projects</h2>
            <p class="mt-2 text-sm leading-7 text-stone-600">Start with a project you already have, or create a new one when the editor flow is ready.</p>
          </div>
          <a href="/create-new" class="btn-secondary text-sm">New project</a>
        </div>

        {#if $auth.status === "anonymous"}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            Sign in first to load your project list.
          </div>
        {:else if loadingProjects}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            Loading your projects...
          </div>
        {:else if projectError}
          <div class="mt-8 rounded-[1.5rem] border border-[color:var(--error-soft)] bg-[color:var(--error-soft)] px-6 py-8 text-sm leading-7 text-[color:var(--error-strong)]">
            {projectError}
          </div>
        {:else if projects.length === 0}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            No projects yet. Create your first one to start building your hosted page.
          </div>
        {:else}
          <div class="mt-8 grid gap-4">
            {#each projects as project}
              <a
                href={`/dashboard/projects/${project.id}`}
                class="flex items-center justify-between rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-stone-300"
              >
                <div>
                  <p class="text-lg font-semibold text-stone-900">{project.name}</p>
                  <p class="mt-1 text-sm text-stone-500">hostingqr.com/{project.slug}</p>
                </div>
                <span class="text-sm font-medium text-stone-700">Open</span>
              </a>
            {/each}
          </div>
        {/if}
      </section>
    </div>
  </div>
</div>

<svelte:head>
  <title>Dashboard - HostingQr</title>
</svelte:head>
