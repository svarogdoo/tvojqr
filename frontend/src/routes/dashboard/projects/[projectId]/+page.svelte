<script lang="ts">
  import { goto } from "$app/navigation";
  import ConfirmationModal from "$lib/components/ConfirmationModal.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import ProjectQrBuilder from "$lib/components/ProjectQrBuilder.svelte";
  import { apiFetch } from "$lib/api";
  import { toApiUrl } from "$lib/config";
  import { auth, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import { showSnackbar } from "$lib/stores/snackbar";
  import type {
    Asset,
    GeneratedSlugResponse,
    ProjectDetail,
    SlugAvailabilityResponse,
    UpdateProjectRequest,
    UpdateProjectStatusRequest,
  } from "$lib/types/projects";
  import { onDestroy, onMount } from "svelte";

  type DraftAsset = {
    id: string;
    file: File;
    previewUrl: string;
    originalFileName: string;
  };

  let projectId = "";
  let project: ProjectDetail | null = null;
  let draftAssets: DraftAsset[] = [];
  let originalSlug = "";
  let form = {
    name: "",
    slug: "",
  };
  let loading = true;
  let error = "";
  let saving = false;
  let uploading = false;
  let saveMessage = "";
  let slugMessage = "";
  let slugError = "";
  let uploadError = "";
  let checkingSlug = false;
  let generatingSlug = false;
  let initializedProjectId = "";
  let updatingStatus = false;
  let deletingProject = false;
  let showDeleteConfirmation = false;
  let isDraft = false;
  let deletingAssetId = "";
  let removedSavedAssetIds = new Set<string>();

  $: slugCheckToneClasses = slugError
    ? "border-[rgba(165,93,79,0.18)] bg-[rgba(249,238,234,0.9)] text-[color:var(--error-strong)]"
    : slugMessage && slugMessage.includes("taken")
      ? "border-[rgba(165,93,79,0.18)] bg-[rgba(249,238,234,0.9)] text-[color:var(--error-strong)]"
    : slugMessage && !slugMessage.includes("taken") && !slugMessage.includes("Enter") && !slugMessage.includes("current")
      ? "border-[rgba(77,106,83,0.18)] bg-[rgba(236,245,238,0.96)] text-[color:var(--success-strong)]"
      : "";

  async function loadProject(preserveForm = false) {
    if (isDraft) {
      loading = false;
      error = "";
      project = null;
      originalSlug = form.slug;
      return;
    }

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
      if (!preserveForm || initializedProjectId !== project.id) {
        form = {
          name: project.name,
          slug: project.slug,
        };
        initializedProjectId = project.id;
      }
      removedSavedAssetIds = new Set<string>();
      slugMessage = "";
      slugError = "";
      uploadError = "";
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
    if (!form.name.trim()) {
      showSnackbar("Project title is required.", "error");
      return;
    }

    saving = true;
    slugError = "";

    try {
      const payload: UpdateProjectRequest = {
        name: form.name.trim(),
        slug: form.slug.trim(),
      };

      let response: Response;
      let savedProject: ProjectDetail;

      if (isDraft) {
        response = await apiFetch(`/api/projects`, {
          method: "POST",
          body: JSON.stringify(payload),
        });

        if (!response.ok) {
          const body = (await response.json()) as { message?: string };
          const message = body.message ?? "Unable to save project settings.";
          if (response.status === 409 || response.status === 400) {
            slugError = message;
          }
          showSnackbar(message, "error");
          return;
        }

        savedProject = (await response.json()) as ProjectDetail;

      } else {
        response = await apiFetch(`/api/projects/${project!.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });

        if (!response.ok) {
          const body = (await response.json()) as { message?: string };
          const message = body.message ?? "Unable to save project settings.";
          if (response.status === 409 || response.status === 400) {
            slugError = message;
          }
          showSnackbar(message, "error");
          return;
        }

        savedProject = (await response.json()) as ProjectDetail;
      }

      if (!isDraft && removedSavedAssetIds.size > 0) {
        for (const assetId of removedSavedAssetIds) {
          const deleteResponse = await apiFetch(`/api/projects/${savedProject.id}/assets/${assetId}`, {
            method: "DELETE",
          });

          if (!deleteResponse.ok) {
            showSnackbar("Project settings saved, but some image deletions failed.", "error");
            await goto(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      if (draftAssets.length > 0) {
        const formData = new FormData();
        draftAssets.forEach((asset) => {
          formData.append("files", asset.file);
        });

        const uploadResponse = await apiFetch(`/api/projects/${savedProject.id}/assets`, {
          method: "POST",
          body: formData,
          headers: {},
        });

        if (!uploadResponse.ok) {
          const body = (await uploadResponse.json()) as { message?: string };
          showSnackbar(body.message ?? "Project saved, but images could not be uploaded.", "error");
          await goto(`/dashboard/projects/${savedProject.id}`);
          return;
        }
      }

      project = savedProject;
      originalSlug = savedProject.slug;
      form = {
        name: savedProject.name,
        slug: savedProject.slug,
      };
      draftAssets.forEach((asset) => URL.revokeObjectURL(asset.previewUrl));
      draftAssets = [];
      removedSavedAssetIds = new Set<string>();
      isDraft = false;
      slugMessage = "";
      showSnackbar("Project settings saved.", "success");
      await goto("/dashboard");
    } catch {
      showSnackbar("Unable to save project settings right now.", "error");
    } finally {
      saving = false;
    }
  }

  async function updateProjectStatus(status: UpdateProjectStatusRequest["status"]) {
    if (!project || isDraft) {
      return;
    }

    updatingStatus = true;

    try {
      const response = await apiFetch(`/api/projects/${project.id}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status } satisfies UpdateProjectStatusRequest),
      });

      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        showSnackbar(body.message ?? "Unable to update project status.", "error");
        return;
      }

      project = (await response.json()) as ProjectDetail;
      showSnackbar(
        status === "disabled" ? "Project disabled." : "Project enabled.",
        "success",
      );
    } catch {
      showSnackbar("Unable to update project status.", "error");
    } finally {
      updatingStatus = false;
    }
  }

  function promptDeleteProject() {
    showDeleteConfirmation = true;
  }

  function closeDeleteConfirmation() {
    if (!deletingProject) {
      showDeleteConfirmation = false;
    }
  }

  async function deleteProject() {
    if (!project || isDraft) {
      return;
    }

    deletingProject = true;

    try {
      const response = await apiFetch(`/api/projects/${project.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        showSnackbar("Unable to delete this project.", "error");
        return;
      }

      showSnackbar("Project deleted.", "success");
      showDeleteConfirmation = false;
      await goto("/dashboard");
    } catch {
      showSnackbar("Unable to delete this project.", "error");
    } finally {
      deletingProject = false;
    }
  }

  function resetMessages() {
    slugMessage = "";
    slugError = "";
  }

  async function uploadImages(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const files = input.files;
    if (!files || files.length === 0) {
      return;
    }

    uploading = true;
    uploadError = "";

    try {
      const nextAssets = Array.from(files).map((file) => ({
        id: crypto.randomUUID(),
        file,
        previewUrl: URL.createObjectURL(file),
        originalFileName: file.name,
      }));

      draftAssets = [...draftAssets, ...nextAssets];
      showSnackbar("Images added. Save to apply changes.", "success");
    } finally {
      uploading = false;
      input.value = "";
    }
  }

  function removeDraftAsset(assetId: string) {
    const asset = draftAssets.find((item) => item.id === assetId);
    if (asset) {
      URL.revokeObjectURL(asset.previewUrl);
    }

    draftAssets = draftAssets.filter((item) => item.id !== assetId);
    showSnackbar("Image removed from draft. Save to apply changes.", "success");
  }

  function deleteSavedAsset(assetId: string) {
    if (!project) {
      return;
    }

    removedSavedAssetIds = new Set([...removedSavedAssetIds, assetId]);
    showSnackbar("Image marked for deletion. Save to apply changes.", "success");
  }

  function restoreSavedAsset(assetId: string) {
    if (!removedSavedAssetIds.has(assetId)) {
      return;
    }

    const next = new Set(removedSavedAssetIds);
    next.delete(assetId);
    removedSavedAssetIds = next;
    showSnackbar("Image restored.", "info");
  }

  $: visibleSavedAssets = (project?.assets ?? []).filter((asset: Asset) => !removedSavedAssetIds.has(asset.id));

  onMount(async () => {
    projectId = window.location.pathname.split("/").at(-1) ?? "";
    isDraft = projectId === "new";
    await refreshSession();
    if (isDraft) {
      loading = false;
      error = "";
      project = null;
      form = {
        name: "",
        slug: "",
      };
      originalSlug = "";
      return;
    }

    await loadProject();
  });

  onDestroy(() => {
    draftAssets.forEach((asset) => URL.revokeObjectURL(asset.previewUrl));
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
    {:else if project || isDraft}
      <section class="rounded-[2rem] border border-black/8 bg-white/96 p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
          <div class="mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <p class="mb-2 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">Project settings</p>
              <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">{project?.name || form.name || "New project"}</h1>
              <div class="mt-4 flex flex-wrap items-center gap-3">
                {#if !isDraft}
                  <span class="rounded-full border border-stone-200 bg-stone-50 px-3 py-1.5 text-sm font-medium text-stone-700">
                    {project?.status === "disabled" ? "Disabled" : "Active"}
                  </span>
                {:else}
                  <span class="rounded-full border border-stone-200 bg-stone-50 px-3 py-1.5 text-sm font-medium text-stone-700">
                    Draft
                  </span>
                {/if}
                <p class="text-base leading-7 text-stone-600">Update the core identity for this hosted page now. Uploads, language variants, preview, and publish flow can follow on top of these settings.</p>
              </div>
            </div>
          </div>

          <div class="grid gap-5">
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Project name</p>
              <input
                bind:value={form.name}
                on:input={resetMessages}
                required
                class="mt-3 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400"
                placeholder="Project title"
              />
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Active slug</p>
              <div class="mt-3 flex flex-col gap-3 rounded-2xl border border-stone-200 bg-white p-2">
                <div class="flex min-w-0 flex-col overflow-hidden rounded-[1rem] border border-stone-200 bg-stone-50 sm:flex-row">
                  <span class="flex items-center border-b border-stone-200 px-4 py-2 text-sm text-stone-500 sm:border-b-0 sm:border-r sm:py-0">
                    hostingqr.com/
                  </span>
                  <input
                    bind:value={form.slug}
                    on:input={resetMessages}
                    class="block min-w-0 flex-1 bg-white px-4 py-3 text-stone-900 outline-none"
                  />
                </div>

                <div class="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:flex lg:flex-nowrap lg:items-center">
                  <button
                    type="button"
                    class={`btn-secondary w-full text-sm lg:w-auto ${slugCheckToneClasses}`}
                    on:click={checkSlugAvailability}
                    disabled={checkingSlug}
                  >
                    {checkingSlug ? "Checking..." : "Check"}
                  </button>
                  <button type="button" class="btn-secondary w-full text-sm lg:w-auto" on:click={generateSlug} disabled={generatingSlug}>
                    {generatingSlug ? "Generating..." : "Random"}
                  </button>
                </div>
              </div>
              {#if slugMessage}
                <p class={`mt-3 text-sm ${slugMessage.includes("taken")
                  ? "text-[color:var(--error-strong)]"
                  : slugCheckToneClasses
                    ? "text-[color:var(--success-strong)]"
                    : "text-stone-600"}`}>
                  {slugMessage}
                </p>
              {/if}
              {#if slugError}
                <p class="mt-3 text-sm text-[color:var(--error-strong)]">{slugError}</p>
              {/if}
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Images</p>
                  <p class="mt-2 text-sm leading-7 text-stone-600">Upload one or more images for the default project language.</p>
                </div>
                <label class="btn-secondary w-full cursor-pointer text-center text-sm sm:w-auto">
                  <span>{uploading ? "Uploading..." : "Add images"}</span>
                  <input type="file" accept="image/*" multiple class="hidden" on:change={uploadImages} disabled={uploading} />
                </label>
              </div>

              {#if isDraft ? draftAssets.length === 0 : visibleSavedAssets.length === 0 && draftAssets.length === 0}
                <div class="mt-5 rounded-2xl border border-dashed border-stone-300 bg-stone-50 px-5 py-6 text-sm text-stone-600">
                  No images uploaded yet.
                </div>
              {:else}
                <div class="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                  {#each visibleSavedAssets as asset}
                    <div class="relative overflow-hidden rounded-[1.25rem] border border-stone-200 bg-white shadow-sm">
                      <img src={toApiUrl(asset.url)} alt={asset.originalFileName} class="aspect-square w-full object-cover" />
                      <button
                        type="button"
                        on:click={() => deleteSavedAsset(asset.id)}
                        class="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-full bg-white/95 text-[color:var(--error-strong)] shadow-sm transition-colors hover:bg-white"
                        aria-label={`Delete ${asset.originalFileName}`}
                      >
                        <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M8 6V4h8v2" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M19 6l-1 14H6L5 6" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M10 11v6M14 11v6" />
                        </svg>
                      </button>
                      <div class="border-t border-stone-100 px-3 py-3">
                        <p class="truncate text-sm font-medium text-stone-700">{asset.originalFileName}</p>
                      </div>
                    </div>
                  {/each}

                  {#each draftAssets as asset}
                    <div class="relative overflow-hidden rounded-[1.25rem] border border-stone-200 bg-white shadow-sm">
                      <img src={asset.previewUrl} alt={asset.originalFileName} class="aspect-square w-full object-cover" />
                      <button
                        type="button"
                        on:click={() => removeDraftAsset(asset.id)}
                        class="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-full bg-white/95 text-[color:var(--error-strong)] shadow-sm transition-colors hover:bg-white"
                        aria-label={`Delete ${asset.originalFileName}`}
                      >
                        <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M8 6V4h8v2" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M19 6l-1 14H6L5 6" />
                          <path stroke-linecap="round" stroke-linejoin="round" d="M10 11v6M14 11v6" />
                        </svg>
                      </button>
                      <div class="border-t border-stone-100 px-3 py-3">
                        <p class="truncate text-sm font-medium text-stone-700">{asset.originalFileName}</p>
                        <p class="mt-1 text-xs text-stone-500">Pending upload</p>
                      </div>
                    </div>
                  {/each}
                </div>
              {/if}
            </div>

            <ProjectQrBuilder slug={form.slug} projectName={form.name || project?.name || ""} />

            <div class="flex flex-col gap-4 rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Save settings</p>
                <p class="mt-2 text-sm leading-7 text-stone-600">Save the project name and active slug before moving on to uploads and preview.</p>
              </div>
              <div class="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
                {#if !isDraft}
                  <button
                    type="button"
                    class="btn-secondary w-full text-sm sm:w-auto"
                    on:click={() => updateProjectStatus((project?.status ?? "active") === "disabled" ? "active" : "disabled")}
                    disabled={updatingStatus || deletingProject}
                  >
                    {updatingStatus
                      ? "Updating..."
                      : project?.status === "disabled"
                        ? "Enable project"
                        : "Disable project"}
                  </button>
                {/if}
                <button type="button" class="btn-primary w-full text-sm sm:w-auto" on:click={saveProject} disabled={saving || deletingProject}>
                  {saving ? "Saving..." : "Save settings"}
                </button>
              </div>
            </div>

            {#if !isDraft}
            <div class="mt-3 rounded-[1.5rem] border border-[rgba(165,93,79,0.16)] bg-[rgba(249,238,234,0.72)] px-6 py-5 shadow-sm">
              <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <p class="text-xs uppercase tracking-[0.18em] text-[color:var(--error-strong)]">Danger zone</p>
                  <p class="mt-2 text-sm leading-7 text-stone-600">Delete this project completely. This removes its settings, slug, and uploaded assets.</p>
                </div>
                <button
                  type="button"
                  class="inline-flex w-full items-center justify-center rounded-full border border-[rgba(165,93,79,0.28)] bg-white px-5 py-3 text-sm font-medium text-[color:var(--error-strong)] transition-colors hover:bg-[rgba(249,238,234,0.5)] sm:w-auto"
                  on:click={promptDeleteProject}
                  disabled={deletingProject || saving || updatingStatus}
                >
                  {deletingProject ? "Deleting..." : "Delete project"}
                </button>
              </div>
            </div>
            {/if}
          </div>
      </section>
    {/if}
  </div>
</div>

<ConfirmationModal
  show={showDeleteConfirmation}
  title="Delete project?"
  description={`This will permanently remove ${project?.name || "this project"}, including its slug and uploaded assets. This cannot be undone.`}
  confirmLabel="Delete permanently"
  cancelLabel="Keep project"
  destructive={true}
  loading={deletingProject}
  onClose={closeDeleteConfirmation}
  onConfirm={deleteProject}
/>

<svelte:head>
  <title>Project - HostingQr</title>
</svelte:head>
