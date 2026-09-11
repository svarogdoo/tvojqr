<script lang="ts">
  import { apiFetch } from "$lib/api";
  import { toApiUrl } from "$lib/config";
  import type { DigitalMenu, DigitalMenuCategory, DigitalMenuItem, MenuType } from "$lib/types/projects";
  import { onMount } from "svelte";

  type PublicAsset = {
    id: string;
    originalFileName: string;
    url: string;
    languageCode: string;
  };

  type PublicLanguage = {
    id: string;
    languageCode: string;
    displayName: string;
    isDefault: boolean;
    sortOrder: number;
  };

  type PublicProject = {
    projectId: string;
    name: string;
    slug: string;
    ownerDisplayName: string;
    status: "active" | "disabled";
    menuType: MenuType;
    timeZone: string;
    backgroundColor: string;
    languages: PublicLanguage[];
    assets: PublicAsset[];
    coverImage: PublicAsset | null;
  };

  const languageMeta: Record<string, { flag: string; label: string }> = {
    en: { flag: "🇬🇧", label: "English" },
    de: { flag: "🇩🇪", label: "German" },
    it: { flag: "🇮🇹", label: "Italian" },
    fr: { flag: "🇫🇷", label: "French" },
    es: { flag: "🇪🇸", label: "Spanish" },
    sr: { flag: "🇷🇸", label: "Serbian" },
    ru: { flag: "🇷🇺", label: "Russian" },
    el: { flag: "🇬🇷", label: "Greek" },
    hr: { flag: "🇭🇷", label: "Croatian" },
  };

  type PageState = "loading" | "active" | "disabled" | "missing" | "error";

  let slug = "";
  let state: PageState = "loading";
  let project: PublicProject | null = null;
  let selectedLanguageCode = "";
  let languageMenuOpen = false;
  let digitalMenu: DigitalMenu | null = null;
  $: visibleDigitalCategories = (digitalMenu?.categories ?? [])
    .map((category) => ({ ...category, items: category.items.filter((item) => !item.isOutOfStock) }))
    .filter((category) => category.items.length > 0);

  $: availableLanguages = (project?.languages ?? [])
    .filter((language) => project?.menuType === "digital"
      ? hasDigitalContentForLanguage(language.languageCode, digitalMenu?.categories ?? [])
      : (project?.assets ?? []).some((asset) => asset.languageCode === language.languageCode))
    .sort((a, b) => a.sortOrder - b.sortOrder);
  $: if (availableLanguages.length > 0 && !availableLanguages.some((language) => language.languageCode === selectedLanguageCode)) {
    selectedLanguageCode = availableLanguages.find((language) => language.isDefault)?.languageCode ?? availableLanguages[0].languageCode;
  }
  $: selectedLanguage =
    availableLanguages.find(
      (language) => language.languageCode === selectedLanguageCode,
    ) ?? availableLanguages[0];
  $: visibleAssets = selectedLanguage
    ? (project?.assets ?? []).filter(
        (asset) => asset.languageCode === selectedLanguage.languageCode,
      )
    : (project?.assets ?? []);

  function getLanguageMeta(languageCode: string) {
    return (
      languageMeta[languageCode] ?? {
        flag: "🌐",
        label: languageCode.toUpperCase(),
      }
    );
  }

  function hasDigitalContentForLanguage(languageCode: string, categories: DigitalMenuCategory[]) {
    return categories.some((category) =>
      category.translations.some((translation) => translation.languageCode === languageCode && translation.name.trim()) ||
      category.items.some((item) => item.translations.some((translation) =>
        translation.languageCode === languageCode && translation.name.trim())));
  }

  function translatedCategoryName(category: DigitalMenuCategory, languageCode: string) {
    return translatedValue(category.translations, "name", languageCode);
  }

  function translatedItem(item: DigitalMenuItem, languageCode: string) {
    const selected = item.translations.find((translation) => translation.languageCode === languageCode);
    const fallbackCode = project?.languages.find((language) => language.isDefault)?.languageCode;
    const fallback = item.translations.find((translation) => translation.languageCode === fallbackCode) ?? item.translations[0];
    return {
      name: selected?.name.trim() || fallback?.name || "",
      description: selected?.description.trim() || fallback?.description || "",
    };
  }

  function translatedValue(translations: Array<{ languageCode: string; name: string }>, field: "name", languageCode: string) {
    const fallbackCode = project?.languages.find((language) => language.isDefault)?.languageCode;
    const selected = translations.find((translation) => translation.languageCode === languageCode && translation[field].trim());
    const fallback = translations.find((translation) => translation.languageCode === fallbackCode && translation[field].trim());
    return selected?.[field] ?? fallback?.[field] ?? translations.find((translation) => translation[field].trim())?.[field] ?? "Untitled";
  }

  function formatPrice(priceText: string, languageCode: string) {
    const trimmed = priceText.trim();
    const normalized = trimmed.replace(",", ".");
    if (!/^\d+(?:\.\d{1,2})?$/.test(normalized) || !digitalMenu?.currencyCode) {
      return trimmed;
    }

    return new Intl.NumberFormat(languageCode || "en", {
      style: "currency",
      currency: digitalMenu.currencyCode,
    }).format(Number(normalized));
  }

  onMount(async () => {
    slug = window.location.pathname.replace(/^\//, "");

    try {
      const response = await apiFetch(
        `/api/public/${encodeURIComponent(slug)}`,
      );

      if (response.status === 404) {
        state = "missing";
        return;
      }

      if (response.status === 410) {
        project = (await response.json()) as PublicProject;
        selectedLanguageCode =
          project.languages.find((language) => language.isDefault)
            ?.languageCode ??
          project.languages[0]?.languageCode ??
          "";
        state = "disabled";
        return;
      }

      if (!response.ok) {
        state = "error";
        return;
      }

      project = (await response.json()) as PublicProject;
      selectedLanguageCode =
        project.languages.find((language) => language.isDefault)
          ?.languageCode ??
        project.languages[0]?.languageCode ??
        "";
      if (project.menuType === "digital") {
        const menuResponse = await apiFetch(`/api/public/${encodeURIComponent(slug)}/digital-menu`);
        if (!menuResponse.ok) {
          state = "error";
          return;
        }
        digitalMenu = (await menuResponse.json()) as DigitalMenu;
      }
      state = "active";
    } catch {
      state = "error";
    }
  });
</script>

<svelte:head>
  <meta name="robots" content="noindex, follow" />
  {#if state === "active" && project}
    <title>{project.name || "Hosted page"} - HostingQr</title>
    <meta
      name="description"
      content={`View the hosted page for ${project.name || project.slug}.`}
    />
  {:else if state === "disabled" && project}
    <title>{project.name || "Hosted page"} unavailable - HostingQr</title>
  {:else}
    <title>HostingQr</title>
  {/if}
</svelte:head>

<div
  class="min-h-screen px-4 py-4 sm:px-6 lg:px-8"
  style={`--page-bg: ${project?.backgroundColor ?? "#f8f7f3"}; background-color: var(--page-bg);`}
>
  {#if state === "loading"}
    <div class="flex min-h-[70vh] items-center justify-center">
      <div class="flex items-center gap-2" aria-label="Loading page">
        <span
          class="h-2 w-2 animate-[pulse_1.4s_ease-in-out_infinite] rounded-full bg-stone-500/50"
        ></span>
        <span
          class="h-2 w-2 animate-[pulse_1.4s_ease-in-out_0.18s_infinite] rounded-full bg-stone-500/50"
        ></span>
        <span
          class="h-2 w-2 animate-[pulse_1.4s_ease-in-out_0.36s_infinite] rounded-full bg-stone-500/50"
        ></span>
      </div>
    </div>
  {:else if state === "active" && project}
    <main class="mx-auto max-w-5xl">
      {#if availableLanguages.length > 1 && selectedLanguage && (project.menuType !== "digital" || !project.coverImage)}
        {@const currentMeta = getLanguageMeta(selectedLanguage.languageCode)}
        <div class="mb-4 flex justify-end">
          <div
            class="relative -m-3 p-3"
            on:mouseleave={() => (languageMenuOpen = false)}
            role="group"
            aria-label="Page language selector"
          >
            <button
              type="button"
              class="inline-flex items-center gap-2 rounded-2xl border border-black/8 px-4 py-2 text-sm font-semibold uppercase tracking-[0.12em] text-stone-800 shadow-[0_12px_30px_rgba(45,53,46,0.10)] backdrop-blur-md transition-all hover:-translate-y-0.5 hover:shadow-[0_16px_36px_rgba(45,53,46,0.14)]"
              style="background: color-mix(in srgb, var(--page-bg) 22%, white 78%);"
              on:click={() => (languageMenuOpen = !languageMenuOpen)}
              aria-label="Choose page language"
            >
              <span class="text-base leading-none">{currentMeta.flag}</span>
              <span>{selectedLanguage.languageCode}</span>
            </button>
            {#if languageMenuOpen}
              <div
                class="absolute right-3 z-20 mt-2 min-w-44 overflow-hidden rounded-3xl border border-black/8 p-1 shadow-[0_20px_55px_rgba(45,53,46,0.14)] backdrop-blur-xl"
                style="background: color-mix(in srgb, var(--page-bg) 18%, white 82%);"
              >
                {#each availableLanguages as language}
                  {@const meta = getLanguageMeta(language.languageCode)}
                  <button
                    type="button"
                    class="flex w-full items-center gap-3 rounded-2xl px-3 py-2.5 text-left text-sm text-stone-700 transition-colors hover:bg-white/55"
                    on:click={() => {
                      selectedLanguageCode = language.languageCode;
                      languageMenuOpen = false;
                    }}
                  >
                    <span class="text-base leading-none">{meta.flag}</span>
                    <span class="font-semibold uppercase"
                      >{language.languageCode}</span
                    >
                    <span class="text-stone-500">{language.displayName}</span>
                  </button>
                {/each}
              </div>
            {/if}
          </div>
        </div>
      {/if}
      {#if project.menuType === "digital"}
        {#if project.coverImage}
          <header class="relative left-1/2 -mt-4 flex h-64 w-screen -translate-x-1/2 items-center justify-center overflow-hidden rounded-b-[2rem] px-6 text-center shadow-[0_24px_60px_rgba(45,53,46,0.14)] sm:h-96 sm:rounded-b-[2.5rem]">
            <img src={toApiUrl(project.coverImage.url)} alt={`${project.name} restaurant cover`} class="absolute inset-0 h-full w-full object-cover" />
            <div class="absolute inset-0 bg-black/38"></div>
            {#if availableLanguages.length > 1 && selectedLanguage}
              {@const currentMeta = getLanguageMeta(selectedLanguage.languageCode)}
              <div class="absolute right-4 top-4 z-10 sm:right-6 sm:top-6">
                <div class="relative -m-3 p-3" on:mouseleave={() => (languageMenuOpen = false)} role="group" aria-label="Page language selector">
                  <button
                    type="button"
                    class="inline-flex items-center gap-2 rounded-2xl border border-white/25 bg-black/28 px-4 py-2 text-sm font-semibold uppercase tracking-[0.12em] text-white shadow-lg backdrop-blur-md transition-all hover:bg-black/38"
                    on:click={() => (languageMenuOpen = !languageMenuOpen)}
                    aria-label="Choose page language"
                  >
                    <span class="text-base leading-none">{currentMeta.flag}</span>
                    <span>{selectedLanguage.languageCode}</span>
                  </button>
                  {#if languageMenuOpen}
                    <div class="absolute right-3 z-20 mt-2 min-w-44 overflow-hidden rounded-3xl border border-black/8 bg-white/94 p-1 text-left shadow-[0_20px_55px_rgba(45,53,46,0.2)] backdrop-blur-xl">
                      {#each availableLanguages as language}
                        {@const meta = getLanguageMeta(language.languageCode)}
                        <button
                          type="button"
                          class="flex w-full items-center gap-3 rounded-2xl px-3 py-2.5 text-left text-sm text-stone-700 transition-colors hover:bg-stone-100"
                          on:click={() => {
                            selectedLanguageCode = language.languageCode;
                            languageMenuOpen = false;
                          }}
                        >
                          <span class="text-base leading-none">{meta.flag}</span>
                          <span class="font-semibold uppercase">{language.languageCode}</span>
                          <span class="text-stone-500">{language.displayName}</span>
                        </button>
                      {/each}
                    </div>
                  {/if}
                </div>
              </div>
            {/if}
            <h1 class="relative max-w-3xl text-3xl font-semibold tracking-tight text-white drop-shadow-lg sm:text-5xl">{project.name}</h1>
          </header>
        {:else}
          <header class="rounded-[2rem] border border-black/6 bg-white/88 px-5 py-7 text-center shadow-[0_20px_50px_rgba(45,53,46,0.08)] backdrop-blur sm:px-8 sm:py-10">
            <h1 class="text-3xl font-semibold tracking-tight text-stone-950 sm:text-5xl">{project.name}</h1>
          </header>
        {/if}

        {#if visibleDigitalCategories.length > 1}
          <nav class="sticky top-2 z-10 -mx-1 mt-4 overflow-x-auto rounded-2xl border border-black/6 bg-white/90 p-2 shadow-[0_12px_30px_rgba(45,53,46,0.1)] backdrop-blur" aria-label="Menu sections">
            <div class="flex min-w-max gap-2">
              {#each visibleDigitalCategories as category}
                <a href={`#section-${category.id}`} class="rounded-full bg-stone-100 px-4 py-2 text-sm font-medium text-stone-700">{translatedCategoryName(category, selectedLanguageCode)}</a>
              {/each}
            </div>
          </nav>
        {/if}

        <section class="mt-5 space-y-5">
          {#if visibleDigitalCategories.length === 0}
            <div class="rounded-[2rem] border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)]">
              <p class="text-base text-stone-600">This menu is being prepared. Please check back soon.</p>
            </div>
          {:else}
            {#each visibleDigitalCategories as category}
              <article id={`section-${category.id}`} class="scroll-mt-24 rounded-[2rem] border border-black/6 bg-white/92 p-5 shadow-[0_18px_45px_rgba(45,53,46,0.07)] sm:p-8">
                <div class="border-b border-stone-200 pb-4">
                  <h2 class="text-2xl font-semibold tracking-tight text-stone-950 sm:text-3xl">{translatedCategoryName(category, selectedLanguageCode)}</h2>
                </div>
                <div class="divide-y divide-stone-100">
                  {#each category.items as item}
                    {@const translation = translatedItem(item, selectedLanguageCode)}
                    <div class="py-5">
                      <div class="flex items-start justify-between gap-5">
                        <div class="min-w-0">
                          <div class="flex flex-wrap items-center gap-2">
                            <h3 class="text-base font-semibold text-stone-900 sm:text-lg">{translation.name || "Untitled item"}</h3>
                          </div>
                          {#if translation.description}
                            <p class="mt-2 max-w-2xl text-sm leading-6 text-stone-600">{translation.description}</p>
                          {/if}
                        </div>
                        {#if item.priceText}
                          <span class="shrink-0 text-base font-semibold text-stone-900">{formatPrice(item.priceText, selectedLanguageCode)}</span>
                        {/if}
                      </div>
                    </div>
                  {/each}
                </div>
              </article>
            {/each}
          {/if}
        </section>
      {:else}
      <section class="space-y-4">
        {#if visibleAssets.length === 0}
          <div
            class="rounded-4xl border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)]"
          >
            <p class="text-base text-stone-600">
              No images have been published on this page yet.
            </p>
          </div>
        {:else}
          {#each visibleAssets as asset}
            <div class="overflow-hidden rounded-md">
              <img
                src={toApiUrl(asset.url)}
                alt={asset.originalFileName}
                class="mx-auto block h-auto max-w-full"
              />
            </div>
          {/each}
        {/if}
      </section>
      {/if}
    </main>
  {:else if state === "disabled"}
    <div
      class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center"
    >
      <div
        class="w-full rounded-4xl border border-black/6 bg-[rgba(239,236,230,0.96)] p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14"
      >
        <div
          class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-900 text-sm font-semibold text-white"
        >
          Paused
        </div>
        <h2
          class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl"
        >
          This page has stepped away from the table
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          The owner has disabled this project for now, so its public page is no
          longer available. If you want your own clean hosted QR page, you can
          start fresh with HostingQr.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Explore HostingQr
        </a>
      </div>
    </div>
  {:else if state === "missing"}
    <div
      class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center"
    >
      <div
        class="w-full rounded-4xl border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14"
      >
        <div
          class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-100 text-sm font-semibold text-stone-600"
        >
          404
        </div>
        <h2
          class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl"
        >
          This page never made it to the menu
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          We couldn’t find a hosted page for this URL. If you want a simple
          project page with your own images and QR code, HostingQr can set you
          up in minutes.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Visit HostingQr
        </a>
      </div>
    </div>
  {:else}
    <div
      class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center"
    >
      <div
        class="w-full rounded-4xl border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14"
      >
        <h2
          class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl"
        >
          This page is taking a short pause
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          Something went wrong while loading this hosted page. You can head back
          to HostingQr and try again in a moment.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Back to HostingQr
        </a>
      </div>
    </div>
  {/if}
</div>
