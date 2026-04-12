<script lang="ts">
  import Navigation from "$lib/components/Navigation.svelte";
  import { apiFetch } from "$lib/api";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import type {
    GeneratedSlugResponse,
    ProjectDetail,
    SlugAvailabilityResponse,
    UpdateProjectRequest,
  } from "$lib/types/projects";
  import { onMount } from "svelte";

  let projectId = "";
  let project: ProjectDetail | null = null;
  let originalSlug = "";
  let form = {
    name: "",
    slug: "",
  };
  let loading = true;
  let error = "";
  let saving = false;
  let saveMessage = "";
  let slugMessage = "";
  let slugError = "";
  let checkingSlug = false;
  let generatingSlug = false;

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
      originalSlug = project.slug;
      form = {
        name: project.name,
        slug: project.slug,
      };
      slugMessage = "";
      slugError = "";
    } catch {
      error = "Unable to load this project right now.";
    } finally {
      loading = false;
    }
  }

  async function checkSlugAvailability() {
    slugError = "";
    saveMessage = "";

    if (!form.slug.trim()) {
      slugMessage = "Enter a slug first.";
      return;
    }

    if (form.slug.trim() === originalSlug) {
      slugMessage = "This is the current slug for the project.";
      return;
    }

    checkingSlug = true;
    try {
      const response = await apiFetch(`/api/slugs/${encodeURIComponent(form.slug.trim())}/availability`);

      if (!response.ok) {
        const payload = (await response.json()) as { message?: string };
        slugError = payload.message ?? "Unable to check slug availability.";
        slugMessage = "";
        return;
      }

      const payload = (await response.json()) as SlugAvailabilityResponse;
      slugMessage = payload.isAvailable
        ? `Slug ${payload.slug} is available.`
        : `Slug ${payload.slug} is already taken.`;
    } catch {
      slugError = "Unable to check slug availability right now.";
      slugMessage = "";
    } finally {
      checkingSlug = false;
    }
  }

  async function generateSlug() {
    generatingSlug = true;
    slugError = "";
    saveMessage = "";

    try {
      const response = await apiFetch("/api/slugs/generate", { method: "POST" });
      if (!response.ok) {
        throw new Error("Unable to generate slug.");
      }

      const payload = (await response.json()) as GeneratedSlugResponse;
      form.slug = payload.slug;
      slugMessage = `Generated slug ${payload.slug}.`;
    } catch {
      slugError = "Unable to generate a slug right now.";
      slugMessage = "";
    } finally {
      generatingSlug = false;
    }
  }

  async function saveProject() {
    if (!project) {
      return;
    }

    saving = true;
    saveMessage = "";
    slugError = "";

    try {
      const payload: UpdateProjectRequest = {
        name: form.name.trim(),
        slug: form.slug.trim(),
      };

      const response = await apiFetch(`/api/projects/${project.id}`, {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        const message = body.message ?? "Unable to save project settings.";
        if (response.status === 409 || response.status === 400) {
          slugError = message;
        } else {
          saveMessage = message;
        }
        return;
      }

      const body = (await response.json()) as ProjectDetail;
      project = body;
      originalSlug = body.slug;
      form = {
        name: body.name,
        slug: body.slug,
      };
      slugMessage = "";
      saveMessage = "Project settings saved.";
    } catch {
      saveMessage = "Unable to save project settings right now.";
    } finally {
      saving = false;
    }
  }

  function resetMessages() {
    saveMessage = "";
    slugMessage = "";
    slugError = "";
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
        <button type="button" class="mt-8 btn-primary" on:click={startGoogleSignIn}>Continue with Google</button>
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
          <p class="mt-5 max-w-md text-base leading-7 text-stone-600">Update the core identity for this hosted page now. Uploads, language variants, preview, and publish flow can follow on top of these settings.</p>
        </section>

        <section class="rounded-[2rem] border border-black/8 bg-white/96 p-8 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
          <div class="grid gap-5">
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Project name</p>
              <input
                bind:value={form.name}
                on:input={resetMessages}
                class="mt-3 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400"
              />
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Active slug</p>
              <div class="mt-3 overflow-hidden rounded-2xl border border-stone-200 bg-white sm:flex sm:items-center">
                <span class="block border-b border-stone-200 px-4 py-3 text-sm text-stone-500 sm:border-b-0 sm:border-r">
                  hostingqr.com/
                </span>
                <input
                  bind:value={form.slug}
                  on:input={resetMessages}
                  class="block w-full bg-transparent px-4 py-3 text-stone-900 outline-none"
                />
              </div>
              <div class="mt-4 flex flex-wrap gap-3">
                <button type="button" class="btn-secondary text-sm" on:click={checkSlugAvailability} disabled={checkingSlug}>
                  {checkingSlug ? "Checking..." : "Check availability"}
                </button>
                <button type="button" class="btn-secondary text-sm" on:click={generateSlug} disabled={generatingSlug}>
                  {generatingSlug ? "Generating..." : "Random slug"}
                </button>
              </div>
              {#if slugMessage}
                <p class="mt-3 text-sm text-stone-600">{slugMessage}</p>
              {/if}
              {#if slugError}
                <p class="mt-3 text-sm text-[color:var(--error-strong)]">{slugError}</p>
              {/if}
            </div>
            <div class="rounded-[1.5rem] border border-dashed border-stone-300 bg-stone-50 px-6 py-6 text-sm leading-7 text-stone-600">
              Uploads, language variants, preview, and save controls are the next step on this page.
            </div>
            <div class="flex flex-wrap items-center justify-between gap-4 rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <div>
                <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Save settings</p>
                <p class="mt-2 text-sm leading-7 text-stone-600">Save the project name and active slug before moving on to uploads and preview.</p>
                {#if saveMessage}
                  <p class="mt-2 text-sm text-stone-700">{saveMessage}</p>
                {/if}
              </div>
              <button type="button" class="btn-primary text-sm" on:click={saveProject} disabled={saving}>
                {saving ? "Saving..." : "Save settings"}
              </button>
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
