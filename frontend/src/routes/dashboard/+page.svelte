<script lang="ts">
import { goto } from "$app/navigation";
import Navigation from "$lib/components/Navigation.svelte";
import { apiFetch } from "$lib/api";
import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
import { showSnackbar } from "$lib/stores/snackbar";
import type { Entitlement, ProjectListItem } from "$lib/types/projects";
  import { onMount } from "svelte";

  let projects: ProjectListItem[] = [];
  let loadingProjects = true;
  let projectError = "";
  let creatingProject = false;
  let entitlement: Entitlement | null = null;
  let loadingEntitlement = true;

  function statusLabel(status: ProjectListItem["status"]) {
    return status === "disabled" ? "Disabled" : "Active";
  }

  function viewCountLabel(count: number) {
    return `${count.toLocaleString()} ${count === 1 ? "view" : "views"}`;
  }

  async function loadProjects() {
    loadingProjects = true;
    projectError = "";

    try {
      const response = await apiFetch("/api/projects");
      if (response.status === 401) {
        projects = [];
        return;
      }

      if (response.status === 402) {
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
    await loadEntitlement();
    if ($auth.status !== "authenticated" || !entitlement?.hasToolAccess) {
      loadingProjects = false;
      return;
    }

    await loadProjects();
  });

  async function loadEntitlement() {
    loadingEntitlement = true;
    entitlement = null;

    if ($auth.status !== "authenticated") {
      loadingEntitlement = false;
      return;
    }

    try {
      const response = await apiFetch("/api/billing/entitlement");
      if (response.status === 401) {
        return;
      }

      if (!response.ok) {
        throw new Error(`Entitlement request failed with status ${response.status}`);
      }

      entitlement = (await response.json()) as Entitlement;
    } catch {
      projectError = "Unable to load your plan status right now.";
    } finally {
      loadingEntitlement = false;
    }
  }

  async function createProject() {
    if ($auth.status === "loading") {
      return;
    }

    if ($auth.status !== "authenticated") {
      startGoogleSignIn();
      return;
    }

    if (!entitlement?.hasToolAccess) {
      await goto("/pricing");
      return;
    }

    try {
      creatingProject = true;
      await goto(`/dashboard/projects/new`);
    } catch (error) {
      const message = error instanceof Error
        ? error.message
        : "Unable to create a new project right now.";
      projectError = message;
      showSnackbar(message, "error");
    } finally {
      creatingProject = false;
    }
  }
</script>

<Navigation />

<div class="min-h-screen px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-5xl">
      <section class="rounded-[2rem] border border-black/8 bg-white/96 p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 class="text-2xl font-semibold tracking-tight text-stone-900">Your projects</h2>
            <p class="mt-2 text-sm leading-7 text-stone-600">Start with a project you already have, or create a new one when the editor flow is ready.</p>
          </div>
          <button type="button" class="btn-secondary w-full text-sm sm:w-auto" on:click={createProject} disabled={creatingProject}>
            {creatingProject ? "Creating..." : "New project"}
          </button>
        </div>

        {#if $auth.status === "anonymous"}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            Sign in first to load your project list.
          </div>
        {:else if loadingEntitlement || loadingProjects}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            Loading your projects...
          </div>
        {:else if projectError}
          <div class="mt-8 rounded-[1.5rem] border border-[color:var(--error-soft)] bg-[color:var(--error-soft)] px-6 py-8 text-sm leading-7 text-[color:var(--error-strong)]">
            {projectError}
          </div>
        {:else if !entitlement?.hasToolAccess}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            <p class="text-base font-semibold text-stone-900">Pick a plan to unlock project tools.</p>
            <p class="mt-2 max-w-xl">Your account is ready, but you need an active tier before you can create, edit, or upload hosted QR pages.</p>
            <a href="/pricing" class="mt-5 inline-flex rounded-full bg-stone-900 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800">See pricing</a>
          </div>
        {:else if projects.length === 0}
          <div class="mt-8 rounded-[1.5rem] border border-stone-200 bg-stone-50 px-6 py-8 text-sm leading-7 text-stone-600">
            No projects yet. Create your first one to start building your hosted page.
          </div>
        {:else}
          <div class="mt-8 grid gap-4">
            {#each projects as project}
              <div
                role="link"
                tabindex="0"
                on:click={() => goto(`/dashboard/projects/${project.id}`)}
                on:keydown={(event) => {
                  if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    goto(`/dashboard/projects/${project.id}`);
                  }
                }}
                class="flex cursor-pointer flex-col gap-4 rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-5 py-5 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-stone-300 sm:flex-row sm:items-center sm:justify-between sm:px-6"
              >
                <div class="min-w-0">
                  <p class="text-lg font-semibold text-stone-900">{project.name || "Untitled project"}</p>
                  <div class="mt-1 flex flex-wrap items-center gap-2 text-sm text-stone-500">
                    <span>hostingqr.com/{project.slug}</span>
                    <span class={`rounded-full border px-2.5 py-1 text-xs font-medium ${project.status === "active"
                      ? "border-[rgba(77,106,83,0.18)] bg-[rgba(236,245,238,0.96)] text-[color:var(--success-strong)]"
                      : "border-stone-200 bg-stone-100 text-stone-600"}`}>
                      {statusLabel(project.status)}
                    </span>
                    <span class="rounded-full border border-stone-200 bg-white px-2.5 py-1 text-xs font-medium text-stone-600">
                      {viewCountLabel(project.viewCount)}
                    </span>
                  </div>
                </div>
                <div class="flex items-center justify-end gap-3 sm:justify-start">
                  <a
                    href={`/${project.slug}`}
                    target="_blank"
                    rel="noreferrer"
                    class="inline-flex h-10 w-10 items-center justify-center rounded-full border border-stone-200 bg-white text-stone-500 transition-all duration-200 hover:border-stone-400 hover:bg-stone-100 hover:text-stone-900"
                    aria-label={`View public page for ${project.name || 'project'}`}
                    on:click|stopPropagation
                  >
                    <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8S1 12 1 12Z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  </a>
                </div>
              </div>
            {/each}
          </div>
        {/if}
      </section>
  </div>
</div>

<svelte:head>
  <title>Dashboard - HostingQr</title>
</svelte:head>
