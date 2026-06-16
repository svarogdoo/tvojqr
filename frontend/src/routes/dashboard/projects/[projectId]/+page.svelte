<script lang="ts">
  import { beforeNavigate, goto } from "$app/navigation";
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
    ProjectLanguageVariant,
    SlugAvailabilityResponse,
    UpdateProjectRequest,
    UpdateProjectStatusRequest,
  } from "$lib/types/projects";
  import { onDestroy, onMount } from "svelte";

  const defaultBackgroundColor = "#f8f7f3";
  const slugCheckDelayMs = 700;
  const availableLanguageOptions = [
    { code: "en", name: "English", flag: "🇬🇧" },
    { code: "es", name: "Spanish", flag: "🇪🇸" },
  ];

  type DraftAsset = {
    id: string;
    file: File;
    previewUrl: string;
    originalFileName: string;
    languageCode: string;
  };

  let projectId = "";
  let project: ProjectDetail | null = null;
  let draftAssets: DraftAsset[] = [];
  let originalSlug = "";
  let form = {
    name: "",
    slug: "",
    backgroundColor: defaultBackgroundColor,
    defaultLanguageCode: "en",
    defaultLanguageDisplayName: "English",
  };
  let savedForm = { ...form };
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
  let removedLanguageCodes = new Set<string>();
  let allowNavigation = false;
  let slugCheckTimeout: ReturnType<typeof setTimeout> | null = null;
  let slugCheckRequestId = 0;
  let savedAssetOrderIds: string[] = [];
  let baselineSavedAssetOrderIds: string[] = [];
  let baselineLanguages: ProjectLanguageVariant[] = [];
  let draggedSavedAssetId = "";
  let addingLanguage = false;

  $: hasFormChanges = form.name !== savedForm.name
    || form.slug !== savedForm.slug
    || form.backgroundColor !== savedForm.backgroundColor;
  $: hasAssetOrderChanges = savedAssetOrderIds.length === baselineSavedAssetOrderIds.length
    && savedAssetOrderIds.some((assetId, index) => assetId !== baselineSavedAssetOrderIds[index]);
  $: hasLanguageChanges = (() => {
    const current = languageSections.map((language) => ({
      id: language.id,
      languageCode: language.languageCode,
      displayName: language.displayName,
      isDefault: language.isDefault,
      sortOrder: language.sortOrder,
    }));

    const baseline = baselineLanguages
      .filter((language) => !removedLanguageCodes.has(language.languageCode))
      .map((language) => ({
        id: language.id,
        languageCode: language.languageCode,
        displayName: language.displayName,
        isDefault: language.isDefault,
        sortOrder: language.sortOrder,
      }));

    return JSON.stringify(current) !== JSON.stringify(baseline);
  })();
  $: hasUnsavedChanges = hasFormChanges || hasAssetOrderChanges || hasLanguageChanges || draftAssets.length > 0 || removedSavedAssetIds.size > 0 || removedLanguageCodes.size > 0;

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
      savedAssetOrderIds = project.assets.map((asset) => asset.id);
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = project.languages.map((language) => ({ ...language }));
      if (!preserveForm || initializedProjectId !== project.id) {
      form = {
        name: project.name,
        slug: project.slug,
        backgroundColor: project.backgroundColor || defaultBackgroundColor,
        defaultLanguageCode: project.languages.find((language) => language.isDefault)?.languageCode ?? project.languages[0]?.languageCode ?? "en",
        defaultLanguageDisplayName: project.languages.find((language) => language.isDefault)?.displayName ?? project.languages[0]?.displayName ?? "English",
      };
        savedForm = { ...form };
        initializedProjectId = project.id;
      }
      removedSavedAssetIds = new Set<string>();
      removedLanguageCodes = new Set<string>();
      slugMessage = "";
      slugError = "";
      uploadError = "";
    } catch {
      error = "Unable to load this project right now.";
    } finally {
      loading = false;
    }
  }

  async function checkSlugAvailability(showEmptyMessage = true) {
    if (checkingSlug) {
      return;
    }

    slugError = "";
    saveMessage = "";
    const requestedSlug = form.slug.trim();

    if (!requestedSlug) {
      slugMessage = showEmptyMessage ? "Enter a slug first." : "";
      return;
    }

    if (requestedSlug === originalSlug) {
      slugMessage = "This is the current slug for the project.";
      return;
    }

    checkingSlug = true;
    const requestId = ++slugCheckRequestId;
    try {
      const response = await apiFetch(`/api/slugs/${encodeURIComponent(requestedSlug)}/availability`);
      if (requestId !== slugCheckRequestId || requestedSlug !== form.slug.trim()) {
        return;
      }

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
      if (requestId !== slugCheckRequestId) {
        return;
      }

      slugError = "Unable to check slug availability right now.";
      slugMessage = "";
    } finally {
      if (requestId === slugCheckRequestId) {
        checkingSlug = false;
      }
    }
  }

  function clearSlugCheckTimeout() {
    if (slugCheckTimeout) {
      clearTimeout(slugCheckTimeout);
      slugCheckTimeout = null;
    }
  }

  function scheduleSlugAvailabilityCheck() {
    clearSlugCheckTimeout();

    if (saving || !form.slug.trim() || form.slug.trim() === originalSlug) {
      return;
    }

    slugCheckTimeout = setTimeout(() => {
      slugCheckTimeout = null;
      void checkSlugAvailability(false);
    }, slugCheckDelayMs);
  }

  function handleSlugInput() {
    resetMessages();
    scheduleSlugAvailabilityCheck();
  }

  async function generateSlug() {
    generatingSlug = true;
    slugError = "";
    saveMessage = "";
    clearSlugCheckTimeout();

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

  async function gotoWithoutUnsavedWarning(path: string) {
    allowNavigation = true;
    try {
      await goto(path);
    } finally {
      allowNavigation = false;
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
        backgroundColor: form.backgroundColor,
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
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      if (!isDraft && removedLanguageCodes.size > 0) {
        for (const languageCode of removedLanguageCodes) {
          const deleteLanguageResponse = await apiFetch(`/api/projects/${savedProject.id}/languages/${languageCode}`, {
            method: "DELETE",
          });

          if (!deleteLanguageResponse.ok) {
            const body = (await deleteLanguageResponse.json()) as { message?: string };
            showSnackbar(body.message ?? "Project settings saved, but a language could not be removed.", "error");
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      if (!isDraft) {
        const baselineById = new Map(baselineLanguages.map((language) => [language.id, language]));
        const languageChanges = languageSections
          .map((language) => ({ current: language, baseline: baselineById.get(language.id) }))
          .filter((entry): entry is { current: ProjectLanguageVariant; baseline: ProjectLanguageVariant } => Boolean(entry.baseline))
          .filter((entry) => entry.current.languageCode !== entry.baseline.languageCode || entry.current.displayName !== entry.baseline.displayName)
          .sort((a, b) => Number(b.current.isDefault) - Number(a.current.isDefault));

        for (const change of languageChanges) {
          const response = await apiFetch(`/api/projects/${savedProject.id}/languages/${encodeURIComponent(change.baseline.languageCode)}`, {
            method: "PUT",
            body: JSON.stringify({
              languageCode: change.current.languageCode,
              displayName: change.current.displayName,
            }),
          });

          if (!response.ok) {
            const body = (await response.json()) as { message?: string };
            showSnackbar(body.message ?? "Project settings saved, but a language could not be updated.", "error");
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      if (!isDraft && hasAssetOrderChanges) {
        for (const language of languageSections) {
          const remainingAssetIds = savedAssetOrderIds.filter((assetId) => {
            const asset = project?.assets.find((item) => item.id === assetId);
            return asset?.languageCode === language.languageCode && !removedSavedAssetIds.has(assetId);
          });

          if (remainingAssetIds.length === 0) {
            continue;
          }

          const reorderResponse = await apiFetch(`/api/projects/${savedProject.id}/assets/order`, {
            method: "PUT",
            body: JSON.stringify({ assetIds: remainingAssetIds }),
          });

          if (!reorderResponse.ok) {
            const body = (await reorderResponse.json()) as { message?: string };
            showSnackbar(body.message ?? "Project settings saved, but image order could not be updated.", "error");
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      if (draftAssets.length > 0) {
        for (const language of languageSections) {
          const languageDraftAssets = draftAssets.filter((asset) => asset.languageCode === language.languageCode);
          if (languageDraftAssets.length === 0) {
            continue;
          }

          const formData = new FormData();
          formData.append("languageCode", language.languageCode);
          languageDraftAssets.forEach((asset) => {
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
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      project = savedProject;
      originalSlug = savedProject.slug;
      form = {
        name: savedProject.name,
        slug: savedProject.slug,
        backgroundColor: savedProject.backgroundColor || defaultBackgroundColor,
        defaultLanguageCode: savedProject.languages.find((language) => language.isDefault)?.languageCode ?? savedProject.languages[0]?.languageCode ?? "en",
        defaultLanguageDisplayName: savedProject.languages.find((language) => language.isDefault)?.displayName ?? savedProject.languages[0]?.displayName ?? "English",
      };
      savedForm = { ...form };
      draftAssets.forEach((asset) => URL.revokeObjectURL(asset.previewUrl));
      draftAssets = [];
      removedSavedAssetIds = new Set<string>();
      removedLanguageCodes = new Set<string>();
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = savedProject.languages.map((language) => ({ ...language }));
      isDraft = false;
      slugMessage = "";
      await gotoWithoutUnsavedWarning("/dashboard");
      showSnackbar("Project settings saved.", "success");
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

  function languageMeta(languageCode: string) {
    return availableLanguageOptions.find((language) => language.code === languageCode) ?? {
      code: languageCode,
      name: languageCode.toUpperCase(),
      flag: "🌐",
    };
  }

  function languageOptionsFor(language: ProjectLanguageVariant) {
    const selectedCodes = new Set((project?.languages ?? [])
      .filter((item) => !removedLanguageCodes.has(item.languageCode) && item.id !== language.id)
      .map((item) => item.languageCode));

    return availableLanguageOptions.filter((option) => !selectedCodes.has(option.code) || option.code === language.languageCode);
  }

  function getFirstAvailableLanguageOption() {
    return availableLanguageOptions.find((option) => !languageSections.some((language) => language.languageCode === option.code));
  }

  function updateLanguageSelection(language: ProjectLanguageVariant, nextLanguageCode: string) {
    if (!project) {
      return;
    }

    const previousLanguageCode = language.languageCode;
    if (previousLanguageCode === nextLanguageCode) {
      return;
    }

    const meta = languageMeta(nextLanguageCode);
    project = {
      ...project,
      languages: project.languages.map((item) => item.id === language.id
        ? { ...item, languageCode: nextLanguageCode, displayName: meta.name }
        : item),
      assets: project.assets.map((asset) => asset.languageCode === previousLanguageCode
        ? { ...asset, languageCode: nextLanguageCode }
        : asset),
    };
    resetMessages();
  }

  async function addLanguage() {
    if (!project) {
      return;
    }

    const option = getFirstAvailableLanguageOption();
    if (!option) {
      showSnackbar("No more languages are available to add.", "error");
      return;
    }

    addingLanguage = true;
    try {
      const response = await apiFetch(`/api/projects/${project.id}/languages`, {
        method: "POST",
        body: JSON.stringify({ languageCode: option.code, displayName: option.name }),
      });

      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        showSnackbar(body.message ?? "Unable to add language.", "error");
        return;
      }

      project = (await response.json()) as ProjectDetail;
      savedAssetOrderIds = project.assets.map((asset) => asset.id);
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = project.languages.map((language) => ({ ...language }));
      showSnackbar("Language added.", "success");
    } catch {
      showSnackbar("Unable to add language.", "error");
    } finally {
      addingLanguage = false;
    }
  }

  function removeLanguage(language: ProjectLanguageVariant) {
    if (!project || language.isDefault) {
      return;
    }

    const confirmed = window.confirm(`Remove ${language.displayName} and its images?`);
    if (!confirmed) {
      return;
    }

    removedLanguageCodes = new Set([...removedLanguageCodes, language.languageCode]);
    showSnackbar("Language marked for removal. Save to apply changes.", "success");
  }

  async function uploadImages(event: Event, languageCode: string) {
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
        languageCode,
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

  function moveSavedAsset(targetAssetId: string) {
    if (!draggedSavedAssetId || draggedSavedAssetId === targetAssetId) {
      return;
    }

    const nextOrder = [...savedAssetOrderIds];
    const fromIndex = nextOrder.indexOf(draggedSavedAssetId);
    const toIndex = nextOrder.indexOf(targetAssetId);
    if (fromIndex === -1 || toIndex === -1) {
      return;
    }

    const [assetId] = nextOrder.splice(fromIndex, 1);
    nextOrder.splice(toIndex, 0, assetId);
    savedAssetOrderIds = nextOrder;
  }

  function dragSavedAsset(event: DragEvent, assetId: string) {
    draggedSavedAssetId = assetId;
    event.dataTransfer?.setData("text/plain", assetId);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = "move";
    }
  }

  function dropSavedAsset(event: DragEvent, targetAssetId: string) {
    event.preventDefault();
    moveSavedAsset(targetAssetId);
    draggedSavedAssetId = "";
  }

  $: orderedSavedAssets = savedAssetOrderIds
    .map((assetId) => (project?.assets ?? []).find((asset: Asset) => asset.id === assetId))
    .filter((asset): asset is Asset => Boolean(asset));
  $: visibleSavedAssets = orderedSavedAssets.filter((asset: Asset) => !removedSavedAssetIds.has(asset.id) && !removedLanguageCodes.has(asset.languageCode));
  $: languageSections = (project?.languages ?? [{ id: "draft-default", languageCode: form.defaultLanguageCode, displayName: form.defaultLanguageDisplayName, isDefault: true, sortOrder: 0 }])
    .filter((language) => !removedLanguageCodes.has(language.languageCode))
    .sort((a, b) => a.sortOrder - b.sortOrder);
  $: if (languageSections.length > 0) {
    form.defaultLanguageCode = languageSections[0].languageCode;
    form.defaultLanguageDisplayName = languageSections[0].displayName;
  }

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
        backgroundColor: defaultBackgroundColor,
        defaultLanguageCode: "en",
        defaultLanguageDisplayName: "English",
      };
      savedForm = { ...form };
      originalSlug = "";
      return;
    }

    await loadProject();
  });

  onDestroy(() => {
    clearSlugCheckTimeout();
    draftAssets.forEach((asset) => URL.revokeObjectURL(asset.previewUrl));
  });

  beforeNavigate((navigation) => {
    if (allowNavigation || !hasUnsavedChanges) {
      return;
    }

    const shouldLeave = window.confirm("You have unsaved changes. Leave this page without saving?");
    if (!shouldLeave) {
      navigation.cancel();
    }
  });

  onMount(() => {
    const warnBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!hasUnsavedChanges || allowNavigation) {
        return;
      }

      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", warnBeforeUnload);
    return () => window.removeEventListener("beforeunload", warnBeforeUnload);
  });
</script>

<Navigation />

<div class="min-h-screen px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-5xl">
    <a href="/dashboard" class="mb-3 inline-flex items-center gap-2 text-xs font-medium uppercase tracking-[0.18em] text-stone-500 transition-colors hover:text-stone-900">
      <span class="text-sm leading-none" aria-hidden="true">←</span>
      <span>Dashboard</span>
    </a>
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
              <div class="mt-1 flex flex-wrap items-center gap-3">
                <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">{project?.name || form.name || "New project"}</h1>
                {#if project?.slug}
                  <a
                    href={`/${project.slug}`}
                    target="_blank"
                    rel="noreferrer"
                    class="inline-flex h-10 w-10 items-center justify-center rounded-full border border-stone-200 bg-white text-stone-500 transition-all duration-200 hover:border-stone-400 hover:bg-stone-100 hover:text-stone-900"
                    aria-label={`View public page for ${project.name || form.name || 'project'}`}
                  >
                    <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8S1 12 1 12Z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  </a>
                {/if}
              </div>
              <div class="mt-4 flex flex-wrap items-center gap-3">
                <p class="text-base leading-7 text-stone-600">Update the core identity for this hosted page now. Uploads, language variants, preview, and publish flow can follow on top of these settings.</p>
              </div>
            </div>
            {#if !isDraft}
              <span class={`w-fit rounded-full border px-3 py-1.5 text-sm font-medium ${project?.status === "disabled"
                ? "border-stone-200 bg-stone-100 text-stone-500"
                : "border-[rgba(77,106,83,0.14)] bg-[rgba(236,245,238,0.7)] text-[color:var(--success-strong)]"}`}>
                {project?.status === "disabled" ? "Disabled" : "Active"}
              </span>
            {:else}
              <span class="w-fit rounded-full border border-stone-200 bg-stone-100 px-3 py-1.5 text-sm font-medium text-stone-500">
                Draft
              </span>
            {/if}
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
              <div class="mt-3 flex flex-col gap-3 rounded-2xl border border-stone-200 bg-white p-2 lg:flex-row lg:items-stretch">
                <div class="flex min-w-0 flex-1 flex-col overflow-hidden rounded-[1rem] border border-stone-200 bg-stone-50 sm:flex-row">
                  <span class="flex items-center border-b border-stone-200 px-4 py-2 text-sm text-stone-500 sm:border-b-0 sm:border-r sm:py-0">
                    hostingqr.com/
                  </span>
                  <input
                    bind:value={form.slug}
                    on:input={handleSlugInput}
                    class="block min-w-0 flex-1 bg-white px-4 py-3 text-stone-900 outline-none"
                  />
                </div>

                <div class="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:flex lg:flex-nowrap lg:items-stretch">
                  <button
                    type="button"
                    class="btn-secondary w-full text-sm lg:w-28"
                    on:click={() => checkSlugAvailability()}
                  >
                    Check
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
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Public page background</p>
              <p class="mt-2 text-sm leading-7 text-stone-600">Choose the background color visitors see behind your uploaded images.</p>
              <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center">
                <label class="flex h-14 w-full cursor-pointer items-center gap-3 rounded-2xl border border-stone-200 bg-white px-4 sm:w-auto">
                  <span class="text-sm font-medium text-stone-700">Color</span>
                  <input
                    type="color"
                    bind:value={form.backgroundColor}
                    class="h-9 w-14 cursor-pointer rounded-lg border border-stone-200 bg-transparent p-1"
                    aria-label="Public page background color"
                  />
                </label>
                <input
                  bind:value={form.backgroundColor}
                  pattern="#[0-9a-fA-F]{6}"
                  maxlength="7"
                  class="block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 font-mono text-sm text-stone-900 outline-none transition-all focus:border-stone-400 sm:max-w-40"
                  aria-label="Background color hex value"
                />
                <div class="h-14 rounded-2xl border border-stone-200 px-5 py-3 text-sm text-stone-600 sm:flex-1" style={`background-color: ${form.backgroundColor};`}>
                  Preview
                </div>
              </div>
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm">
              <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Content languages</p>
                  <p class="mt-2 text-sm leading-7 text-stone-600">Upload images per language, then drag them into the order visitors should see.</p>
                </div>
                <button type="button" class="btn-secondary text-sm" on:click={addLanguage} disabled={addingLanguage}>
                  {addingLanguage ? "Adding..." : "+ Add"}
                </button>
              </div>

              <div class="mt-5 grid gap-5">
                {#each languageSections as language}
                  {@const choices = languageOptionsFor(language)}
                  {@const sectionSavedAssets = visibleSavedAssets.filter((asset) => asset.languageCode === language.languageCode)}
                  {@const sectionDraftAssets = draftAssets.filter((asset) => asset.languageCode === language.languageCode)}
                  <section class="rounded-[1.25rem] border border-stone-200 bg-white px-4 py-4 shadow-sm">
                    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <div class="flex flex-wrap items-center gap-2">
                          <select
                            value={language.languageCode}
                            on:change={(event) => updateLanguageSelection(language, (event.currentTarget as HTMLSelectElement).value)}
                            class="min-w-56 rounded-xl border border-stone-200 bg-white px-3 py-2 text-sm font-medium text-stone-900 outline-none transition-all focus:border-stone-400"
                            aria-label={`Language for ${language.displayName}`}
                          >
                            {#each choices as option}
                              <option value={option.code}>{option.flag} {option.name}</option>
                            {/each}
                            {#if !choices.some((option) => option.code === language.languageCode)}
                              {@const currentMeta = languageMeta(language.languageCode)}
                              <option value={currentMeta.code}>{currentMeta.flag} {currentMeta.name}</option>
                            {/if}
                          </select>
                          <span class="rounded-full border border-stone-200 bg-stone-50 px-2.5 py-1 text-xs font-medium uppercase text-stone-500">{language.languageCode}</span>
                          {#if language.isDefault}
                            <span class="rounded-full border border-[rgba(77,106,83,0.14)] bg-[rgba(236,245,238,0.7)] px-2.5 py-1 text-xs font-medium text-[color:var(--success-strong)]">Default</span>
                          {/if}
                        </div>
                        <p class="mt-1 text-sm text-stone-500">Images in this section appear when visitors select this language.</p>
                      </div>
                      <div class="flex flex-col gap-2 sm:flex-row">
                        {#if !language.isDefault && !isDraft}
                          <button type="button" class="btn-secondary text-sm" on:click={() => removeLanguage(language)}>Remove</button>
                        {/if}
                        <label class="btn-secondary cursor-pointer text-center text-sm">
                          <span>{uploading ? "Uploading..." : "Add images"}</span>
                          <input type="file" accept="image/*" multiple class="hidden" on:change={(event) => uploadImages(event, language.languageCode)} disabled={uploading} />
                        </label>
                      </div>
                    </div>

                    {#if sectionSavedAssets.length === 0 && sectionDraftAssets.length === 0}
                      <div class="mt-4 rounded-2xl border border-dashed border-stone-300 bg-stone-50 px-5 py-6 text-sm text-stone-600">
                        No images uploaded for {language.displayName} yet.
                      </div>
                    {:else}
                      <div class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        {#each sectionSavedAssets as asset}
                          <div class={`relative overflow-hidden rounded-[1.25rem] border bg-white shadow-sm transition-all ${draggedSavedAssetId === asset.id ? "border-stone-400 opacity-60" : "border-stone-200"}`} draggable="true" on:dragstart={(event) => dragSavedAsset(event, asset.id)} on:dragend={() => draggedSavedAssetId = ""} on:dragover|preventDefault on:drop={(event) => dropSavedAsset(event, asset.id)} role="group" aria-label={`Drag to reorder ${asset.originalFileName}`}>
                            <img src={toApiUrl(asset.url)} alt={asset.originalFileName} class="aspect-square w-full object-cover" />
                            <div class="absolute left-3 top-3 inline-flex items-center gap-1 rounded-full bg-white/95 px-3 py-2 text-xs font-medium text-stone-500 shadow-sm" title="Drag to reorder">Drag</div>
                            <button type="button" on:click={() => deleteSavedAsset(asset.id)} class="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-full bg-white/95 text-[color:var(--error-strong)] shadow-sm transition-colors hover:bg-white" aria-label={`Delete ${asset.originalFileName}`}>
                              <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18" /><path stroke-linecap="round" stroke-linejoin="round" d="M8 6V4h8v2" /><path stroke-linecap="round" stroke-linejoin="round" d="M19 6l-1 14H6L5 6" /><path stroke-linecap="round" stroke-linejoin="round" d="M10 11v6M14 11v6" /></svg>
                            </button>
                            <div class="border-t border-stone-100 px-3 py-3"><p class="truncate text-sm font-medium text-stone-700">{asset.originalFileName}</p></div>
                          </div>
                        {/each}
                        {#each sectionDraftAssets as asset}
                          <div class="relative overflow-hidden rounded-[1.25rem] border border-stone-200 bg-white shadow-sm">
                            <img src={asset.previewUrl} alt={asset.originalFileName} class="aspect-square w-full object-cover" />
                            <button type="button" on:click={() => removeDraftAsset(asset.id)} class="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-full bg-white/95 text-[color:var(--error-strong)] shadow-sm transition-colors hover:bg-white" aria-label={`Delete ${asset.originalFileName}`}>
                              <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18" /><path stroke-linecap="round" stroke-linejoin="round" d="M8 6V4h8v2" /><path stroke-linecap="round" stroke-linejoin="round" d="M19 6l-1 14H6L5 6" /><path stroke-linecap="round" stroke-linejoin="round" d="M10 11v6M14 11v6" /></svg>
                            </button>
                            <div class="border-t border-stone-100 px-3 py-3"><p class="truncate text-sm font-medium text-stone-700">{asset.originalFileName}</p><p class="mt-1 text-xs text-stone-500">Pending upload</p></div>
                          </div>
                        {/each}
                      </div>
                    {/if}
                  </section>
                {/each}
              </div>
            </div>

            <ProjectQrBuilder slug={form.slug} projectName={form.name || project?.name || ""} />

            <div class="flex flex-col gap-4 rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Save settings</p>
                <p class="mt-2 text-sm leading-7 text-stone-600">
                  {hasUnsavedChanges ? "You have unsaved changes. Save before leaving this page." : "Your project settings are saved."}
                </p>
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
                <button
                  type="button"
                  class={`w-full text-sm sm:w-auto ${hasUnsavedChanges ? "btn-primary shadow-[0_12px_28px_rgba(77,106,83,0.22)] ring-2 ring-[rgba(77,106,83,0.18)]" : "btn-secondary"}`}
                  on:click={saveProject}
                  disabled={saving || deletingProject || !hasUnsavedChanges}
                >
                  {saving ? "Saving..." : hasUnsavedChanges ? "Save changes" : "Saved"}
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
