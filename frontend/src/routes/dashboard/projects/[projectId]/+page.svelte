<script lang="ts">
  import { beforeNavigate, goto } from "$app/navigation";
  import ConfirmationModal from "$lib/components/ConfirmationModal.svelte";
  import DigitalMenuEditor from "$lib/components/DigitalMenuEditor.svelte";
  import MenuLanguageToolbar from "$lib/components/MenuLanguageToolbar.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import ProjectQrBuilder from "$lib/components/ProjectQrBuilder.svelte";
  import { apiFetch } from "$lib/api";
  import { toApiUrl } from "$lib/config";
  import { auth, authNavigationStartedEvent, refreshSession, startGoogleSignIn } from "$lib/stores/auth";
  import { showSnackbar } from "$lib/stores/snackbar";
  import type {
    Asset,
    CreateProjectRequest,
    Entitlement,
    GeneratedSlugResponse,
    MenuType,
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
    { code: "pt", name: "Portuguese", flag: "🇵🇹" },
    { code: "it", name: "Italian", flag: "🇮🇹" },
    { code: "ar", name: "Arabic", flag: "🇸🇦" },
    { code: "fr", name: "French", flag: "🇫🇷" },
    { code: "bs", name: "Bosnian", flag: "🇧🇦" },
    { code: "ja", name: "Japanese", flag: "🇯🇵" },
    { code: "hr", name: "Croatian", flag: "🇭🇷" },
    { code: "sr", name: "Serbian", flag: "🇷🇸" },
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
  let mixedAssetOrderIds: string[] = [];
  let baselineSavedAssetOrderIds: string[] = [];
  let baselineLanguages: ProjectLanguageVariant[] = [];
  let draggedAssetOrderId = "";
  let addingLanguage = false;
  let selectedMenuType: MenuType | null = null;
  let currentTier: Entitlement["tier"] | null = null;
  let hasDigitalMenuChanges = false;
  let activeEditorTab: "general" | "menu" = "general";
  let activeImageLanguageCode = "en";
  let removingLanguageCode = "";
  let changingDefaultLanguageCode = "";
  let uploadingCover = false;
  let removingCover = false;

  $: hasFormChanges = form.name !== savedForm.name
    || form.slug !== savedForm.slug
    || form.backgroundColor !== savedForm.backgroundColor;
  $: currentSavedAssetOrderIds = mixedAssetItems
    .filter((item) => item.kind === "saved")
    .map((item) => item.asset.id);
  $: hasAssetOrderChanges = currentSavedAssetOrderIds.length === baselineSavedAssetOrderIds.length
    && currentSavedAssetOrderIds.some((assetId, index) => assetId !== baselineSavedAssetOrderIds[index]);
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
  $: hasImageMenuChanges = hasAssetOrderChanges || draftAssets.length > 0 || removedSavedAssetIds.size > 0;
  $: hasProjectChanges = hasFormChanges || hasAssetOrderChanges || hasLanguageChanges || draftAssets.length > 0 || removedSavedAssetIds.size > 0 || removedLanguageCodes.size > 0;
  $: hasUnsavedChanges = hasProjectChanges || hasDigitalMenuChanges;

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
      selectedMenuType = project.menuType;
      activeImageLanguageCode = project.languages.find((language) => language.isDefault)?.languageCode ?? project.languages[0]?.languageCode ?? "en";
      originalSlug = project.slug;
      savedAssetOrderIds = project.assets.map((asset) => asset.id);
      mixedAssetOrderIds = project.assets.map((asset) => savedOrderId(asset.id));
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
    if (hasDigitalMenuChanges) {
      showSnackbar("Save your Digital Menu changes first.", "error");
      return;
    }
    if (!form.name.trim()) {
      showSnackbar("Project title is required.", "error");
      return;
    }

    const wasDraft = isDraft;
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
        if (!selectedMenuType) {
          showSnackbar("Choose a menu type first.", "error");
          return;
        }

        const createPayload: CreateProjectRequest = {
          ...payload,
          defaultLanguageCode: form.defaultLanguageCode,
          defaultLanguageDisplayName: form.defaultLanguageDisplayName,
          menuType: selectedMenuType,
        };
        response = await apiFetch(`/api/projects`, {
          method: "POST",
          body: JSON.stringify(createPayload),
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

          savedProject = (await deleteLanguageResponse.json()) as ProjectDetail;
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

          savedProject = (await response.json()) as ProjectDetail;
        }
      }

      const uploadedDraftAssetIds = new Map<string, string>();
      if (draftAssets.length > 0) {
        for (const language of languageSections) {
          const languageDraftAssets = mixedAssetItems
            .filter((item) => item.kind === "draft" && item.languageCode === language.languageCode)
            .map((item) => item.asset as DraftAsset);
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

          const assetsAfterUpload = (await uploadResponse.json()) as Asset[];
          const newestAssets = assetsAfterUpload
            .filter((asset) => asset.languageCode === language.languageCode)
            .slice(-languageDraftAssets.length);
          languageDraftAssets.forEach((draftAsset, index) => {
            const uploadedAsset = newestAssets[index];
            if (uploadedAsset) {
              uploadedDraftAssetIds.set(draftOrderId(draftAsset.id), uploadedAsset.id);
            }
          });
        }
      }

      const finalAssetOrderIds = mixedAssetOrderIds
        .map((orderId) => orderId.startsWith("saved:") ? orderId.slice("saved:".length) : uploadedDraftAssetIds.get(orderId))
        .filter((assetId): assetId is string => Boolean(assetId))
        .filter((assetId) => !removedSavedAssetIds.has(assetId));

      if (!isDraft && (hasAssetOrderChanges || uploadedDraftAssetIds.size > 0 || removedSavedAssetIds.size > 0)) {
        for (const language of languageSections) {
          const orderedLanguageAssetIds = finalAssetOrderIds.filter((assetId) => {
            const savedAsset = project?.assets.find((asset) => asset.id === assetId);
            if (savedAsset) {
              return savedAsset.languageCode === language.languageCode;
            }

            const draftEntry = [...uploadedDraftAssetIds.entries()].find(([, uploadedAssetId]) => uploadedAssetId === assetId);
            const draftAsset = draftEntry ? draftAssets.find((asset) => draftOrderId(asset.id) === draftEntry[0]) : null;
            return draftAsset?.languageCode === language.languageCode;
          });

          if (orderedLanguageAssetIds.length === 0) {
            continue;
          }

          const reorderResponse = await apiFetch(`/api/projects/${savedProject.id}/assets/order`, {
            method: "PUT",
            body: JSON.stringify({ assetIds: orderedLanguageAssetIds }),
          });

          if (!reorderResponse.ok) {
            const body = (await reorderResponse.json()) as { message?: string };
            showSnackbar(body.message ?? "Project settings saved, but image order could not be updated.", "error");
            await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
            return;
          }
        }
      }

      project = savedProject;
      selectedMenuType = savedProject.menuType;
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
      savedAssetOrderIds = finalAssetOrderIds;
      mixedAssetOrderIds = finalAssetOrderIds.map((assetId) => savedOrderId(assetId));
      removedSavedAssetIds = new Set<string>();
      removedLanguageCodes = new Set<string>();
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = savedProject.languages.map((language) => ({ ...language }));
      isDraft = false;
      slugMessage = "";
      if (wasDraft) {
        await gotoWithoutUnsavedWarning(`/dashboard/projects/${savedProject.id}`);
      }
      showSnackbar("Project settings saved.", "success");
    } catch {
      showSnackbar("Unable to save project settings right now.", "error");
    } finally {
      saving = false;
    }
  }

  async function uploadCoverImage(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !project || project.menuType !== "digital") {
      input.value = "";
      return;
    }

    uploadingCover = true;
    try {
      const formData = new FormData();
      formData.append("file", file);
      const response = await apiFetch(`/api/projects/${project.id}/cover-image`, {
        method: "POST",
        body: formData,
        headers: {},
      });
      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        showSnackbar(body.message ?? "Unable to upload the cover image.", "error");
        return;
      }

      project = { ...project, coverImage: (await response.json()) as Asset };
      showSnackbar("Cover image updated.", "success");
    } catch {
      showSnackbar("Unable to upload the cover image.", "error");
    } finally {
      uploadingCover = false;
      input.value = "";
    }
  }

  async function removeCoverImage() {
    if (!project?.coverImage || removingCover) {
      return;
    }

    removingCover = true;
    try {
      const response = await apiFetch(`/api/projects/${project.id}/cover-image`, { method: "DELETE" });
      if (!response.ok) {
        showSnackbar("Unable to remove the cover image.", "error");
        return;
      }

      project = { ...project, coverImage: null };
      showSnackbar("Cover image removed.", "success");
    } catch {
      showSnackbar("Unable to remove the cover image.", "error");
    } finally {
      removingCover = false;
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

  async function addLanguage(languageCode: string) {
    if (!project) {
      return;
    }

    if (addingLanguage || removingLanguageCode || changingDefaultLanguageCode) {
      return;
    }

    if (selectedMenuType === "image" && hasImageMenuChanges) {
      showSnackbar("Save your image changes before adding another language.", "error");
      return;
    }

    const option = availableLanguageOptions.find((candidate) => candidate.code === languageCode);
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
      mixedAssetOrderIds = project.assets.map((asset) => savedOrderId(asset.id));
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = project.languages.map((language) => ({ ...language }));
      activeImageLanguageCode = option.code;
      showSnackbar("Language added.", "success");
    } catch {
      showSnackbar("Unable to add language.", "error");
    } finally {
      addingLanguage = false;
    }
  }

  async function removeLanguage(language: ProjectLanguageVariant) {
    if (!project || language.isDefault) {
      return;
    }

    if (addingLanguage || removingLanguageCode || changingDefaultLanguageCode) {
      return;
    }

    const hasPendingImageChanges = draftAssets.length > 0 || removedSavedAssetIds.size > 0 || hasAssetOrderChanges;
    if ((selectedMenuType === "digital" && hasDigitalMenuChanges) || (selectedMenuType === "image" && hasPendingImageChanges)) {
      showSnackbar("Save your menu changes before removing a language.", "error");
      return;
    }

    const confirmed = window.confirm(selectedMenuType === "digital"
      ? `Remove ${language.displayName}? All menu translations in this language will be permanently deleted.`
      : `Remove ${language.displayName} and its images?`);
    if (!confirmed) {
      return;
    }

    removingLanguageCode = language.languageCode;
    try {
      const response = await apiFetch(`/api/projects/${project.id}/languages/${encodeURIComponent(language.languageCode)}`, {
        method: "DELETE",
      });
      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        showSnackbar(body.message ?? "Unable to remove language.", "error");
        return;
      }

      project = (await response.json()) as ProjectDetail;
      savedAssetOrderIds = project.assets.map((asset) => asset.id);
      mixedAssetOrderIds = project.assets.map((asset) => savedOrderId(asset.id));
      baselineSavedAssetOrderIds = [...savedAssetOrderIds];
      baselineLanguages = project.languages.map((item) => ({ ...item }));
      if (activeImageLanguageCode === language.languageCode) {
        activeImageLanguageCode = project.languages.find((item) => item.isDefault)?.languageCode ?? project.languages[0]?.languageCode ?? "en";
      }
      showSnackbar("Language removed.", "success");
    } catch {
      showSnackbar("Unable to remove language.", "error");
    } finally {
      removingLanguageCode = "";
    }
  }

  async function makeDefaultLanguage(language: ProjectLanguageVariant) {
    if (!project || language.isDefault) {
      return;
    }

    if (addingLanguage || removingLanguageCode || changingDefaultLanguageCode) {
      return;
    }

    const hasPendingImageChanges = draftAssets.length > 0 || removedSavedAssetIds.size > 0 || hasAssetOrderChanges;
    if ((selectedMenuType === "digital" && hasDigitalMenuChanges) || (selectedMenuType === "image" && hasPendingImageChanges)) {
      showSnackbar("Save your menu changes before changing the default language.", "error");
      return;
    }

    const currentDefault = project.languages.find((item) => item.isDefault);
    if (!currentDefault) {
      return;
    }

    changingDefaultLanguageCode = language.languageCode;
    try {
      const response = await apiFetch(`/api/projects/${project.id}/languages/${encodeURIComponent(currentDefault.languageCode)}`, {
        method: "PUT",
        body: JSON.stringify({ languageCode: language.languageCode, displayName: language.displayName }),
      });
      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        showSnackbar(body.message ?? "Unable to change the default language.", "error");
        return;
      }

      project = (await response.json()) as ProjectDetail;
      baselineLanguages = project.languages.map((item) => ({ ...item }));
      activeImageLanguageCode = language.languageCode;
      showSnackbar(`${language.displayName} is now the default language.`, "success");
    } catch {
      showSnackbar("Unable to change the default language.", "error");
    } finally {
      changingDefaultLanguageCode = "";
    }
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
      mixedAssetOrderIds = [...mixedAssetOrderIds, ...nextAssets.map((asset) => draftOrderId(asset.id))];
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
    mixedAssetOrderIds = mixedAssetOrderIds.filter((orderId) => orderId !== draftOrderId(assetId));
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

  function savedOrderId(assetId: string) {
    return `saved:${assetId}`;
  }

  function draftOrderId(assetId: string) {
    return `draft:${assetId}`;
  }

  function moveMixedAsset(targetOrderId: string) {
    if (!draggedAssetOrderId || draggedAssetOrderId === targetOrderId) {
      return;
    }

    const draggedAsset = mixedAssetItems.find((item) => item.orderId === draggedAssetOrderId);
    const targetAsset = mixedAssetItems.find((item) => item.orderId === targetOrderId);
    if (!draggedAsset || !targetAsset || draggedAsset.languageCode !== targetAsset.languageCode) {
      return;
    }

    const nextOrder = [...mixedAssetOrderIds];
    const fromIndex = nextOrder.indexOf(draggedAssetOrderId);
    const toIndex = nextOrder.indexOf(targetOrderId);
    if (fromIndex === -1 || toIndex === -1) {
      return;
    }

    const [orderId] = nextOrder.splice(fromIndex, 1);
    nextOrder.splice(toIndex, 0, orderId);
    mixedAssetOrderIds = nextOrder;
  }

  function dragMixedAsset(event: DragEvent, orderId: string) {
    draggedAssetOrderId = orderId;
    event.dataTransfer?.setData("text/plain", orderId);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = "move";
    }
  }

  function dropMixedAsset(event: DragEvent, targetOrderId: string) {
    event.preventDefault();
    moveMixedAsset(targetOrderId);
    draggedAssetOrderId = "";
  }

  function moveImageAsset(languageCode: string, orderId: string, direction: -1 | 1) {
    const languageOrderIds = mixedAssetItems
      .filter((item) => item.languageCode === languageCode)
      .map((item) => item.orderId);
    const currentIndex = languageOrderIds.indexOf(orderId);
    const targetOrderId = languageOrderIds[currentIndex + direction];
    if (currentIndex === -1 || !targetOrderId) {
      return;
    }

    const nextOrder = [...mixedAssetOrderIds];
    const currentGlobalIndex = nextOrder.indexOf(orderId);
    const targetGlobalIndex = nextOrder.indexOf(targetOrderId);
    [nextOrder[currentGlobalIndex], nextOrder[targetGlobalIndex]] = [nextOrder[targetGlobalIndex], nextOrder[currentGlobalIndex]];
    mixedAssetOrderIds = nextOrder;
  }

  $: mixedAssetItems = mixedAssetOrderIds
    .map((orderId) => {
      if (orderId.startsWith("saved:")) {
        const asset = (project?.assets ?? []).find((item) => item.id === orderId.slice("saved:".length));
        return asset ? { kind: "saved" as const, orderId, languageCode: asset.languageCode, asset } : null;
      }

      const asset = draftAssets.find((item) => item.id === orderId.slice("draft:".length));
      return asset ? { kind: "draft" as const, orderId, languageCode: asset.languageCode, asset } : null;
    })
    .filter((item): item is ({ kind: "saved"; orderId: string; languageCode: string; asset: Asset } | { kind: "draft"; orderId: string; languageCode: string; asset: DraftAsset }) => Boolean(item))
    .filter((item) => item.kind === "draft" || !removedSavedAssetIds.has(item.asset.id))
    .filter((item) => !removedLanguageCodes.has(item.languageCode));
  $: languageSections = (project?.languages ?? [{ id: "draft-default", languageCode: form.defaultLanguageCode, displayName: form.defaultLanguageDisplayName, isDefault: true, sortOrder: 0 }])
    .filter((language) => !removedLanguageCodes.has(language.languageCode))
    .sort((a, b) => a.sortOrder - b.sortOrder);
  $: if (languageSections.length > 0) {
    form.defaultLanguageCode = languageSections[0].languageCode;
    form.defaultLanguageDisplayName = languageSections[0].displayName;
  }
  $: if (languageSections.length > 0 && !languageSections.some((language) => language.languageCode === activeImageLanguageCode)) {
    activeImageLanguageCode = languageSections.find((language) => language.isDefault)?.languageCode ?? languageSections[0].languageCode;
  }

  onMount(async () => {
    projectId = window.location.pathname.split("/").at(-1) ?? "";
    isDraft = projectId === "new";
    await refreshSession();
    if (isDraft) {
      try {
        const entitlementResponse = await apiFetch("/api/billing/entitlement");
        if (entitlementResponse.ok) {
          currentTier = ((await entitlementResponse.json()) as Entitlement).tier;
        }
      } catch {
        currentTier = null;
      }
      loading = false;
      error = "";
      project = null;
      selectedMenuType = null;
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
    const allowAuthNavigation = () => {
      allowNavigation = true;
    };

    const warnBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!hasUnsavedChanges || allowNavigation) {
        return;
      }

      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener(authNavigationStartedEvent, allowAuthNavigation);
    window.addEventListener("beforeunload", warnBeforeUnload);
    return () => {
      window.removeEventListener(authNavigationStartedEvent, allowAuthNavigation);
      window.removeEventListener("beforeunload", warnBeforeUnload);
    };
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
    {:else if isDraft && !selectedMenuType}
      <section class="rounded-[2rem] border border-black/8 bg-white/96 p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
        <div class="mx-auto max-w-2xl text-center">
          <p class="text-sm font-medium uppercase tracking-[0.2em] text-stone-500">New project</p>
          <h1 class="mt-3 text-3xl font-semibold tracking-tight text-stone-900 sm:text-5xl">What kind of menu do you want?</h1>
          <p class="mx-auto mt-4 max-w-xl text-base leading-7 text-stone-600">Choose how you want to manage and present this menu. This choice is fixed after the project is created.</p>
        </div>

        <div class="mx-auto mt-8 grid max-w-3xl gap-5 md:grid-cols-2">
          <button type="button" on:click={() => selectedMenuType = "image"} class="group rounded-[1.75rem] border border-stone-200 bg-stone-50 p-6 text-left shadow-sm transition-all hover:-translate-y-1 hover:border-stone-300 hover:bg-white hover:shadow-lg">
            <span class="inline-flex rounded-full bg-stone-200 px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] text-stone-700">Image Menu</span>
            <h2 class="mt-5 text-2xl font-semibold tracking-tight text-stone-900">Use your existing design</h2>
            <p class="mt-3 text-sm leading-7 text-stone-600">Upload finished menu images for each language. Replace the images whenever your menu changes.</p>
            <span class="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-stone-900">Choose Image Menu <span aria-hidden="true">→</span></span>
          </button>

          <button type="button" on:click={() => selectedMenuType = "digital"} disabled={currentTier === "standard"} class="group rounded-[1.75rem] border border-stone-300 bg-[rgba(220,228,216,0.62)] p-6 text-left shadow-sm transition-all hover:-translate-y-1 hover:border-stone-400 hover:bg-[rgba(220,228,216,0.82)] hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-60 disabled:hover:translate-y-0">
            <span class="inline-flex rounded-full bg-stone-800 px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] text-white">Digital Menu</span>
            <h2 class="mt-5 text-2xl font-semibold tracking-tight text-stone-900">Edit everything anytime</h2>
            <p class="mt-3 text-sm leading-7 text-stone-600">Manage sections, dishes, prices, translations, stock, and serving times from any device.</p>
            <span class="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-stone-800">Choose Digital Menu <span aria-hidden="true">→</span></span>
          </button>
        </div>
        {#if currentTier === "standard"}
          <p class="mx-auto mt-5 max-w-xl text-center text-sm text-stone-600">Digital Menu requires the Digital Menu plan. <a href="/pricing" class="font-semibold text-stone-800 underline">View pricing</a></p>
        {/if}
      </section>
    {:else if project || isDraft}
      <section class="rounded-[2rem] border border-black/8 bg-white/96 p-6 shadow-[0_20px_50px_rgba(45,53,46,0.09)] sm:p-10">
          <div class="mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <div class="mb-2 flex flex-wrap items-center gap-2">
                <p class="text-sm font-medium uppercase tracking-[0.2em] text-stone-500">Project settings</p>
                <span class="rounded-full bg-stone-100 px-2.5 py-1 text-xs font-semibold text-stone-600">{selectedMenuType === "digital" ? "Digital Menu" : "Image Menu"}</span>
              </div>
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

          {#if selectedMenuType}
            <div class="mb-6 rounded-2xl border border-stone-200 bg-stone-100 p-1" role="tablist" aria-label="Project editor sections">
              <div class="grid grid-cols-2 gap-1">
                <button
                  type="button"
                  role="tab"
                  aria-selected={activeEditorTab === "general"}
                  on:click={() => activeEditorTab = "general"}
                  class={`rounded-xl px-3 py-2.5 text-sm font-semibold transition-all ${activeEditorTab === "general" ? "bg-white text-stone-900 shadow-sm" : "text-stone-500 hover:text-stone-800"}`}
                >
                  General Settings
                </button>
                <button
                  type="button"
                  role="tab"
                  aria-selected={activeEditorTab === "menu"}
                  on:click={() => activeEditorTab = "menu"}
                  disabled={isDraft}
                  class={`rounded-xl px-3 py-2.5 text-sm font-semibold transition-all ${activeEditorTab === "menu" ? "bg-white text-stone-900 shadow-sm" : "text-stone-500 hover:text-stone-800"} disabled:cursor-not-allowed disabled:opacity-45`}
                >
                  Menu Editor
                </button>
              </div>
            </div>
          {/if}

          <div class="grid gap-5">
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm" class:hidden={activeEditorTab === "menu"}>
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Project name</p>
              <input
                bind:value={form.name}
                on:input={resetMessages}
                required
                class="mt-3 block w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400"
                placeholder="Project title"
              />
            </div>
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm" class:hidden={activeEditorTab === "menu"}>
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
            <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm" class:hidden={activeEditorTab === "menu"}>
              <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Public page background</p>
              <p class="mt-2 text-sm leading-7 text-stone-600">Choose the background color visitors see behind your menu.</p>
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
            {#if selectedMenuType === "digital"}
              <div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm" class:hidden={activeEditorTab === "menu"}>
                <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Restaurant cover</p>
                <p class="mt-2 text-sm leading-7 text-stone-600">Add a wide restaurant or food photo to the top of your public menu.</p>
                {#if project?.coverImage}
                  <div class="mt-4 overflow-hidden rounded-[1.25rem] border border-stone-200 bg-white">
                    <img src={toApiUrl(project.coverImage.url)} alt="Current restaurant cover" class="aspect-[16/7] w-full object-cover" />
                    <div class="flex flex-col gap-2 p-3 sm:flex-row sm:justify-end">
                      <label class="btn-secondary cursor-pointer text-center text-sm">
                        <span>{uploadingCover ? "Replacing..." : "Replace image"}</span>
                        <input type="file" accept="image/jpeg,image/png,image/webp,image/gif" class="hidden" on:change={uploadCoverImage} disabled={uploadingCover || removingCover} />
                      </label>
                      <button type="button" class="btn-secondary text-sm text-[color:var(--error-strong)]" on:click={removeCoverImage} disabled={uploadingCover || removingCover}>
                        {removingCover ? "Removing..." : "Remove image"}
                      </button>
                    </div>
                  </div>
                {:else if project}
                  <label class="mt-4 flex min-h-32 cursor-pointer flex-col items-center justify-center rounded-[1.25rem] border border-dashed border-stone-300 bg-white px-5 py-7 text-center transition-colors hover:border-stone-400 hover:bg-stone-50">
                    <span class="text-sm font-semibold text-stone-800">{uploadingCover ? "Uploading..." : "Choose cover image"}</span>
                    <span class="mt-1 text-xs text-stone-500">JPG, PNG, WebP, or GIF</span>
                    <input type="file" accept="image/jpeg,image/png,image/webp,image/gif" class="hidden" on:change={uploadCoverImage} disabled={uploadingCover} />
                  </label>
                {:else}
                  <p class="mt-4 rounded-2xl border border-dashed border-stone-300 bg-white px-5 py-6 text-center text-sm text-stone-600">Save this project before adding a cover image.</p>
                {/if}
              </div>
            {/if}
            <div class="rounded-[1.5rem] border border-stone-200 bg-stone-50/70 px-4 py-5 shadow-sm sm:px-6" class:hidden={selectedMenuType !== "image" || activeEditorTab !== "menu"}>
              <MenuLanguageToolbar
                languages={languageSections}
                bind:activeLanguageCode={activeImageLanguageCode}
                options={availableLanguageOptions}
                adding={addingLanguage}
                {removingLanguageCode}
                {changingDefaultLanguageCode}
                onAdd={addLanguage}
                onRemove={removeLanguage}
                onMakeDefault={makeDefaultLanguage}
              />

              {#each languageSections.filter((language) => language.languageCode === activeImageLanguageCode) as language}
                {@const sectionAssets = mixedAssetItems.filter((item) => item.languageCode === language.languageCode)}
                <section class="mt-4 rounded-[1.25rem] border border-stone-200 bg-white px-4 py-4 shadow-sm">
                  <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <div>
                      <h2 class="text-lg font-semibold text-stone-900">{language.displayName} menu images</h2>
                      <p class="mt-1 text-sm text-stone-500">Drag images into the order visitors should see.</p>
                    </div>
                    <label class="btn-secondary cursor-pointer text-center text-sm">
                      <span>{uploading ? "Adding..." : "+ Add images"}</span>
                      <input type="file" accept="image/*" multiple class="hidden" on:change={(event) => uploadImages(event, language.languageCode)} disabled={uploading} />
                    </label>
                  </div>

                  {#if sectionAssets.length === 0}
                    <div class="mt-4 rounded-2xl border border-dashed border-stone-300 bg-stone-50 px-5 py-8 text-center text-sm text-stone-600">
                      No images added for {language.displayName} yet.
                    </div>
                  {:else}
                    <div class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                      {#each sectionAssets as item, itemIndex}
                        <div class={`relative overflow-hidden rounded-[1.25rem] border bg-white shadow-sm transition-all ${draggedAssetOrderId === item.orderId ? "border-stone-400 opacity-60" : "border-stone-200"}`} draggable="true" on:dragstart={(event) => dragMixedAsset(event, item.orderId)} on:dragend={() => draggedAssetOrderId = ""} on:dragover|preventDefault on:drop={(event) => dropMixedAsset(event, item.orderId)} role="group" aria-label={`Drag to reorder ${item.asset.originalFileName}`}>
                          <img src={item.kind === "saved" ? toApiUrl(item.asset.url) : item.asset.previewUrl} alt={item.asset.originalFileName} class="aspect-square w-full object-cover" />
                          <div class="absolute left-3 top-3 inline-flex items-center gap-1 rounded-full bg-white/95 px-3 py-2 text-xs font-medium text-stone-500 shadow-sm" title="Drag to reorder">Drag</div>
                          <button type="button" on:click={() => item.kind === "saved" ? deleteSavedAsset(item.asset.id) : removeDraftAsset(item.asset.id)} class="absolute right-3 top-3 inline-flex h-9 w-9 items-center justify-center rounded-full bg-white/95 text-[color:var(--error-strong)] shadow-sm transition-colors hover:bg-white" aria-label={`Delete ${item.asset.originalFileName}`}>
                            <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18" /><path stroke-linecap="round" stroke-linejoin="round" d="M8 6V4h8v2" /><path stroke-linecap="round" stroke-linejoin="round" d="M19 6l-1 14H6L5 6" /><path stroke-linecap="round" stroke-linejoin="round" d="M10 11v6M14 11v6" /></svg>
                          </button>
                          <div class="absolute bottom-14 right-3 flex gap-1 rounded-full bg-white/95 p-1 shadow-sm lg:hidden">
                            <button type="button" on:click={() => moveImageAsset(language.languageCode, item.orderId, -1)} disabled={itemIndex === 0} class="flex h-10 w-10 items-center justify-center rounded-full text-stone-700 disabled:opacity-30" aria-label={`Move ${item.asset.originalFileName} earlier`}>←</button>
                            <button type="button" on:click={() => moveImageAsset(language.languageCode, item.orderId, 1)} disabled={itemIndex === sectionAssets.length - 1} class="flex h-10 w-10 items-center justify-center rounded-full text-stone-700 disabled:opacity-30" aria-label={`Move ${item.asset.originalFileName} later`}>→</button>
                          </div>
                          <div class="border-t border-stone-100 px-3 py-3"><p class="truncate text-sm font-medium text-stone-700">{item.asset.originalFileName}</p>{#if item.kind === "draft"}<p class="mt-1 text-xs text-stone-500">Pending upload</p>{/if}</div>
                        </div>
                      {/each}
                    </div>
                  {/if}
                </section>
              {/each}

              <div class="sticky bottom-4 z-10 mt-4 rounded-2xl border border-stone-200 bg-white/95 p-3 shadow-[0_16px_40px_rgba(45,53,46,0.16)] backdrop-blur sm:flex sm:items-center sm:justify-between">
                <p class="px-2 text-sm text-stone-600">{hasImageMenuChanges ? "You have unsaved image changes." : "Image menu is saved."}</p>
                <button type="button" class={`mt-3 w-full text-sm sm:mt-0 sm:w-auto ${hasImageMenuChanges ? "btn-primary" : "btn-secondary"}`} on:click={saveProject} disabled={!hasImageMenuChanges || saving}>{saving ? "Saving..." : hasImageMenuChanges ? "Save menu" : "Saved"}</button>
              </div>
            </div>

            <div class:hidden={activeEditorTab === "menu"}>
              <ProjectQrBuilder slug={form.slug} projectName={form.name || project?.name || ""} />
            </div>

            <div class="flex flex-col gap-4 rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-5 shadow-sm sm:flex-row sm:items-center sm:justify-between" class:hidden={activeEditorTab === "menu"}>
              <div>
                <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Save settings</p>
                <p class="mt-2 text-sm leading-7 text-stone-600">
                   {hasDigitalMenuChanges ? "Save your Digital Menu changes in the menu editor first." : hasProjectChanges ? "You have unsaved project changes. Save before leaving this page." : "Your project settings are saved."}
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
                  class={`w-full text-sm sm:w-auto ${hasProjectChanges && !hasDigitalMenuChanges ? "btn-primary shadow-[0_12px_28px_rgba(77,106,83,0.22)] ring-2 ring-[rgba(77,106,83,0.18)]" : "btn-secondary"}`}
                  on:click={saveProject}
                  disabled={saving || deletingProject || !hasProjectChanges || hasDigitalMenuChanges}
                >
                  {saving ? "Saving..." : hasProjectChanges ? "Save project" : "Saved"}
                </button>
              </div>
            </div>

            {#if !isDraft}
            <div class="mt-3 rounded-[1.5rem] border border-[rgba(165,93,79,0.16)] bg-[rgba(249,238,234,0.72)] px-6 py-5 shadow-sm" class:hidden={activeEditorTab === "menu"}>
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

          {#if !isDraft && project?.menuType === "digital"}
            <div class:hidden={activeEditorTab !== "menu"} role="tabpanel" aria-label="Menu Editor">
              <DigitalMenuEditor
                projectId={project.id}
                languages={languageSections}
                initialTimeZone={project.timeZone}
                languageOptions={availableLanguageOptions}
                {addingLanguage}
                {removingLanguageCode}
                {changingDefaultLanguageCode}
                onAddLanguage={addLanguage}
                onRemoveLanguage={removeLanguage}
                onMakeDefaultLanguage={makeDefaultLanguage}
                bind:dirty={hasDigitalMenuChanges}
              />
            </div>
          {/if}
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
  <meta name="robots" content="noindex, nofollow" />
</svelte:head>
