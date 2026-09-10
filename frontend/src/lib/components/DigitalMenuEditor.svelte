<script lang="ts">
  import { apiFetch } from "$lib/api";
  import { showSnackbar } from "$lib/stores/snackbar";
  import type {
    DigitalMenu,
    DigitalMenuCategory,
    DigitalMenuItem,
    ProjectLanguageVariant,
  } from "$lib/types/projects";
  import { onMount } from "svelte";

  export let projectId: string;
  export let languages: ProjectLanguageVariant[];
  export let initialTimeZone = "UTC";
  export let dirty = false;

  const weekdays = [
    { value: 1, short: "Mon" },
    { value: 2, short: "Tue" },
    { value: 3, short: "Wed" },
    { value: 4, short: "Thu" },
    { value: 5, short: "Fri" },
    { value: 6, short: "Sat" },
    { value: 0, short: "Sun" },
  ];

  let menu: DigitalMenu = { timeZone: initialTimeZone, categories: [] };
  let baseline = JSON.stringify(menu);
  let loading = true;
  let saving = false;
  let error = "";
  let activeLanguageCode = "en";
  let expandedCategoryId = "";
  let savedItemIds = new Set<string>();
  let loadedSuccessfully = false;

  $: sortedLanguages = [...languages].sort((a, b) => a.sortOrder - b.sortOrder);
  $: if (!sortedLanguages.some((language) => language.languageCode === activeLanguageCode)) {
    activeLanguageCode = sortedLanguages.find((language) => language.isDefault)?.languageCode ?? sortedLanguages[0]?.languageCode ?? "en";
  }
  $: hasChanges = JSON.stringify(menu) !== baseline;
  $: dirty = hasChanges;

  onMount(loadMenu);

  async function loadMenu() {
    loading = true;
    error = "";
    loadedSuccessfully = false;
    try {
      const response = await apiFetch(`/api/projects/${projectId}/digital-menu`);
      if (!response.ok) {
        let message = response.status === 404
          ? "The Digital Menu service or project could not be found. Refresh after restarting the backend."
          : `Digital Menu could not be loaded (${response.status}).`;
        try {
          const body = (await response.json()) as { message?: string };
          message = body.message ?? message;
        } catch {
          // Empty API responses still get the status-specific message above.
        }
        throw new Error(message);
      }

      menu = (await response.json()) as DigitalMenu;
      baseline = JSON.stringify(menu);
      savedItemIds = new Set(menu.categories.flatMap((category) => category.items.map((item) => item.id)));
      expandedCategoryId = menu.categories[0]?.id ?? "";
      loadedSuccessfully = true;
    } catch (caught) {
      error = caught instanceof Error ? caught.message : "Unable to load the digital menu right now.";
    } finally {
      loading = false;
    }
  }

  function categoryName(category: DigitalMenuCategory, languageCode = activeLanguageCode) {
    return category.translations.find((translation) => translation.languageCode === languageCode)?.name ?? "";
  }

  function itemText(item: DigitalMenuItem, languageCode = activeLanguageCode) {
    return item.translations.find((translation) => translation.languageCode === languageCode) ?? { languageCode, name: "", description: "" };
  }

  function updateCategoryName(categoryId: string, value: string) {
    menu = {
      ...menu,
      categories: menu.categories.map((category) => category.id === categoryId
        ? {
            ...category,
            translations: upsertCategoryTranslation(category, activeLanguageCode, value),
          }
        : category),
    };
  }

  function upsertCategoryTranslation(category: DigitalMenuCategory, languageCode: string, name: string) {
    const exists = category.translations.some((translation) => translation.languageCode === languageCode);
    return exists
      ? category.translations.map((translation) => translation.languageCode === languageCode ? { ...translation, name } : translation)
      : [...category.translations, { languageCode, name }];
  }

  function updateItem(itemId: string, changes: Partial<DigitalMenuItem>) {
    menu = {
      ...menu,
      categories: menu.categories.map((category) => ({
        ...category,
        items: category.items.map((item) => item.id === itemId ? { ...item, ...changes } : item),
      })),
    };
  }

  function updateItemTranslation(item: DigitalMenuItem, field: "name" | "description", value: string) {
    const exists = item.translations.some((translation) => translation.languageCode === activeLanguageCode);
    const translations = exists
      ? item.translations.map((translation) => translation.languageCode === activeLanguageCode ? { ...translation, [field]: value } : translation)
      : [...item.translations, { languageCode: activeLanguageCode, name: field === "name" ? value : "", description: field === "description" ? value : "" }];
    updateItem(item.id, { translations });
  }

  function addCategory() {
    if (!loadedSuccessfully) return;
    const id = crypto.randomUUID();
    const defaultLanguage = sortedLanguages.find((language) => language.isDefault)?.languageCode ?? activeLanguageCode;
    menu = {
      ...menu,
      categories: [...menu.categories, {
        id,
        sortOrder: menu.categories.length,
        translations: [{ languageCode: defaultLanguage, name: "New section" }],
        schedules: [],
        items: [],
      }],
    };
    activeLanguageCode = defaultLanguage;
    expandedCategoryId = id;
  }

  function removeCategory(categoryId: string) {
    if (!window.confirm("Remove this section and all of its items?")) {
      return;
    }

    menu = { ...menu, categories: menu.categories.filter((category) => category.id !== categoryId) };
  }

  function addItem(categoryId: string) {
    const defaultLanguage = sortedLanguages.find((language) => language.isDefault)?.languageCode ?? activeLanguageCode;
    menu = {
      ...menu,
      categories: menu.categories.map((category) => category.id === categoryId
        ? {
            ...category,
            items: [...category.items, {
              id: crypto.randomUUID(),
              priceText: "",
              isOutOfStock: false,
              sortOrder: category.items.length,
              translations: [{ languageCode: defaultLanguage, name: "New item", description: "" }],
            }],
          }
        : category),
    };
    activeLanguageCode = defaultLanguage;
  }

  function removeItem(categoryId: string, itemId: string) {
    menu = {
      ...menu,
      categories: menu.categories.map((category) => category.id === categoryId
        ? { ...category, items: category.items.filter((item) => item.id !== itemId) }
        : category),
    };
  }

  function moveCategory(index: number, direction: -1 | 1) {
    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= menu.categories.length) return;
    const categories = [...menu.categories];
    [categories[index], categories[nextIndex]] = [categories[nextIndex], categories[index]];
    menu = { ...menu, categories: categories.map((category, sortOrder) => ({ ...category, sortOrder })) };
  }

  function moveItem(categoryId: string, index: number, direction: -1 | 1) {
    menu = {
      ...menu,
      categories: menu.categories.map((category) => {
        if (category.id !== categoryId) return category;
        const nextIndex = index + direction;
        if (nextIndex < 0 || nextIndex >= category.items.length) return category;
        const items = [...category.items];
        [items[index], items[nextIndex]] = [items[nextIndex], items[index]];
        return { ...category, items: items.map((item, sortOrder) => ({ ...item, sortOrder })) };
      }),
    };
  }

  function setScheduleEnabled(category: DigitalMenuCategory, enabled: boolean) {
    const schedules = enabled
      ? weekdays.map((day) => ({ dayOfWeek: day.value, startsAt: "07:00", endsAt: "12:00" }))
      : [];
    updateCategorySchedules(category.id, schedules);
  }

  function setScheduleDay(category: DigitalMenuCategory, dayOfWeek: number, enabled: boolean) {
    const template = category.schedules[0] ?? { startsAt: "07:00", endsAt: "12:00" };
    const schedules = enabled
      ? [...category.schedules, { dayOfWeek, startsAt: template.startsAt, endsAt: template.endsAt }]
      : category.schedules.filter((schedule) => schedule.dayOfWeek !== dayOfWeek);
    updateCategorySchedules(category.id, schedules.sort((a, b) => a.dayOfWeek - b.dayOfWeek));
  }

  function setScheduleTime(category: DigitalMenuCategory, field: "startsAt" | "endsAt", value: string) {
    updateCategorySchedules(category.id, category.schedules.map((schedule) => ({ ...schedule, [field]: value })));
  }

  function updateCategorySchedules(categoryId: string, schedules: DigitalMenuCategory["schedules"]) {
    menu = { ...menu, categories: menu.categories.map((category) => category.id === categoryId ? { ...category, schedules } : category) };
  }

  async function toggleAvailability(item: DigitalMenuItem) {
    const nextValue = !item.isOutOfStock;
    updateItem(item.id, { isOutOfStock: nextValue });
    if (!savedItemIds.has(item.id)) return;

    try {
      const response = await apiFetch(`/api/projects/${projectId}/digital-menu/items/${item.id}/availability`, {
        method: "PATCH",
        body: JSON.stringify({ isOutOfStock: nextValue }),
      });
      if (!response.ok) throw new Error();

      const saved = JSON.parse(baseline) as DigitalMenu;
      saved.categories.forEach((category) => category.items.forEach((savedItem) => {
        if (savedItem.id === item.id) savedItem.isOutOfStock = nextValue;
      }));
      baseline = JSON.stringify(saved);
      showSnackbar(nextValue ? "Item marked out of stock." : "Item is available again.", "success");
    } catch {
      updateItem(item.id, { isOutOfStock: item.isOutOfStock });
      showSnackbar("Unable to update item availability.", "error");
    }
  }

  async function saveMenu() {
    if (!loadedSuccessfully) return;
    saving = true;
    error = "";
    try {
      const response = await apiFetch(`/api/projects/${projectId}/digital-menu`, {
        method: "PUT",
        body: JSON.stringify(menu),
      });
      if (!response.ok) {
        const body = (await response.json()) as { message?: string };
        throw new Error(body.message ?? "Unable to save the digital menu.");
      }

      menu = (await response.json()) as DigitalMenu;
      baseline = JSON.stringify(menu);
      savedItemIds = new Set(menu.categories.flatMap((category) => category.items.map((item) => item.id)));
      showSnackbar("Digital menu saved.", "success");
    } catch (caught) {
      error = caught instanceof Error ? caught.message : "Unable to save the digital menu.";
      showSnackbar(error, "error");
    } finally {
      saving = false;
    }
  }
</script>

<section class="rounded-[1.5rem] border border-emerald-200 bg-emerald-50/40 px-4 py-5 shadow-sm sm:px-6">
  <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
    <div>
      <p class="text-xs font-semibold uppercase tracking-[0.18em] text-emerald-700">Digital menu</p>
      <h2 class="mt-2 text-2xl font-semibold tracking-tight text-stone-900">Sections and items</h2>
      <p class="mt-2 text-sm leading-6 text-stone-600">Edit the selected language, manage availability, and decide when each section is served.</p>
    </div>
    <button type="button" class="btn-primary w-full text-sm sm:w-auto" on:click={addCategory} disabled={!loadedSuccessfully}>+ Add section</button>
  </div>

  {#if loading}
    <div class="mt-5 rounded-2xl border border-stone-200 bg-white p-5 text-sm text-stone-600">Loading digital menu...</div>
  {:else if !loadedSuccessfully}
    <div class="mt-5 rounded-2xl border border-red-200 bg-red-50 p-5 text-sm text-red-700">
      <p>{error || "Unable to load the digital menu right now."}</p>
      <button type="button" class="mt-3 rounded-full bg-stone-900 px-4 py-2 text-sm font-medium text-white" on:click={loadMenu}>Try again</button>
    </div>
  {:else}
    <div class="mt-5 overflow-x-auto pb-1">
      <div class="flex min-w-max gap-2" role="tablist" aria-label="Menu language">
        {#each sortedLanguages as language}
          <button type="button" on:click={() => activeLanguageCode = language.languageCode} class={`rounded-full px-4 py-2 text-sm font-medium transition-colors ${activeLanguageCode === language.languageCode ? "bg-stone-900 text-white" : "border border-stone-200 bg-white text-stone-600"}`}>
            {language.displayName}
          </button>
        {/each}
      </div>
    </div>

    <label class="mt-5 block rounded-2xl border border-stone-200 bg-white p-4">
      <span class="text-xs font-semibold uppercase tracking-[0.16em] text-stone-500">Restaurant timezone</span>
      <input bind:value={menu.timeZone} list="digital-menu-timezones" class="mt-2 block w-full rounded-xl border border-stone-200 bg-stone-50 px-3 py-2.5 text-sm text-stone-900 outline-none focus:border-stone-400" />
      <datalist id="digital-menu-timezones">
        <option value="Europe/Nicosia"></option><option value="Europe/Zagreb"></option><option value="Europe/Rome"></option><option value="Europe/Madrid"></option><option value="UTC"></option>
      </datalist>
    </label>

    {#if error}
      <p class="mt-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>
    {/if}

    {#if menu.categories.length === 0}
      <button type="button" on:click={addCategory} class="mt-5 w-full rounded-2xl border border-dashed border-stone-300 bg-white px-5 py-8 text-center text-sm text-stone-600 transition-colors hover:border-stone-400">
        Your menu has no sections yet. Add the first one.
      </button>
    {:else}
      <div class="mt-5 space-y-4">
        {#each menu.categories as category, categoryIndex}
          <article class="overflow-hidden rounded-[1.35rem] border border-stone-200 bg-white shadow-sm">
            <div class="flex items-center gap-2 p-3 sm:p-4">
              <button type="button" class="min-w-0 flex-1 text-left" on:click={() => expandedCategoryId = expandedCategoryId === category.id ? "" : category.id}>
                <span class="block truncate text-lg font-semibold text-stone-900">{categoryName(category) || "Untitled section"}</span>
                <span class="mt-1 block text-xs text-stone-500">{category.items.length} {category.items.length === 1 ? "item" : "items"}{category.schedules.length ? " · Timed" : " · Always visible"}</span>
              </button>
              <button type="button" class="h-10 w-10 rounded-full border border-stone-200 text-stone-600 disabled:opacity-30" on:click={() => moveCategory(categoryIndex, -1)} disabled={categoryIndex === 0} aria-label="Move section up">↑</button>
              <button type="button" class="h-10 w-10 rounded-full border border-stone-200 text-stone-600 disabled:opacity-30" on:click={() => moveCategory(categoryIndex, 1)} disabled={categoryIndex === menu.categories.length - 1} aria-label="Move section down">↓</button>
              <button type="button" class="h-10 w-10 rounded-full border border-red-100 text-red-600" on:click={() => removeCategory(category.id)} aria-label="Remove section">×</button>
            </div>

            {#if expandedCategoryId === category.id}
              <div class="border-t border-stone-100 bg-stone-50/60 p-3 sm:p-5">
                <label class="block">
                  <span class="text-xs font-semibold uppercase tracking-[0.14em] text-stone-500">Section name · {activeLanguageCode.toUpperCase()}</span>
                  <input value={categoryName(category)} on:input={(event) => updateCategoryName(category.id, event.currentTarget.value)} class="mt-2 block w-full rounded-xl border border-stone-200 bg-white px-3 py-2.5 text-stone-900 outline-none focus:border-stone-400" placeholder="Breakfast" />
                </label>

                <div class="mt-4 rounded-xl border border-stone-200 bg-white p-4">
                  <label class="flex items-center justify-between gap-4">
                    <span><span class="block text-sm font-semibold text-stone-900">Timed section</span><span class="mt-1 block text-xs text-stone-500">Only show this section during serving hours.</span></span>
                    <input type="checkbox" checked={category.schedules.length > 0} on:change={(event) => setScheduleEnabled(category, event.currentTarget.checked)} class="h-5 w-5 accent-emerald-700" />
                  </label>
                  {#if category.schedules.length > 0}
                    <div class="mt-4 flex flex-wrap gap-2">
                      {#each weekdays as day}
                        <label class={`cursor-pointer rounded-full border px-3 py-2 text-xs font-medium ${category.schedules.some((schedule) => schedule.dayOfWeek === day.value) ? "border-emerald-300 bg-emerald-50 text-emerald-800" : "border-stone-200 text-stone-500"}`}>
                          <input type="checkbox" class="sr-only" checked={category.schedules.some((schedule) => schedule.dayOfWeek === day.value)} on:change={(event) => setScheduleDay(category, day.value, event.currentTarget.checked)} />{day.short}
                        </label>
                      {/each}
                    </div>
                    <div class="mt-3 grid grid-cols-2 gap-3">
                      <label class="text-xs font-medium text-stone-600">From<input type="time" value={category.schedules[0]?.startsAt ?? "07:00"} on:input={(event) => setScheduleTime(category, "startsAt", event.currentTarget.value)} class="mt-1 block w-full rounded-xl border border-stone-200 px-3 py-2 text-sm" /></label>
                      <label class="text-xs font-medium text-stone-600">Until<input type="time" value={category.schedules[0]?.endsAt ?? "12:00"} on:input={(event) => setScheduleTime(category, "endsAt", event.currentTarget.value)} class="mt-1 block w-full rounded-xl border border-stone-200 px-3 py-2 text-sm" /></label>
                    </div>
                  {/if}
                </div>

                <div class="mt-4 space-y-3">
                  {#each category.items as item, itemIndex}
                    {@const translation = itemText(item)}
                    <div class={`rounded-xl border bg-white p-3 ${item.isOutOfStock ? "border-amber-200" : "border-stone-200"}`}>
                      <div class="grid gap-3 sm:grid-cols-[1fr_8rem]">
                        <input value={translation.name} on:input={(event) => updateItemTranslation(item, "name", event.currentTarget.value)} class="min-w-0 rounded-xl border border-stone-200 px-3 py-2.5 text-sm font-medium outline-none focus:border-stone-400" placeholder={`Item name · ${activeLanguageCode.toUpperCase()}`} />
                        <input value={item.priceText} on:input={(event) => updateItem(item.id, { priceText: event.currentTarget.value })} class="rounded-xl border border-stone-200 px-3 py-2.5 text-sm outline-none focus:border-stone-400" placeholder="€8.50" />
                      </div>
                      <textarea value={translation.description} on:input={(event) => updateItemTranslation(item, "description", event.currentTarget.value)} rows="2" class="mt-3 block w-full resize-none rounded-xl border border-stone-200 px-3 py-2.5 text-sm outline-none focus:border-stone-400" placeholder={`Description · ${activeLanguageCode.toUpperCase()}`}></textarea>
                      <div class="mt-3 flex flex-wrap items-center gap-2">
                        <button type="button" on:click={() => toggleAvailability(item)} class={`min-h-10 rounded-full px-4 text-xs font-semibold ${item.isOutOfStock ? "bg-amber-100 text-amber-900" : "bg-emerald-50 text-emerald-800"}`}>{item.isOutOfStock ? "Out of stock" : "Available"}</button>
                        <div class="ml-auto flex gap-2">
                          <button type="button" class="h-10 w-10 rounded-full border border-stone-200 disabled:opacity-30" on:click={() => moveItem(category.id, itemIndex, -1)} disabled={itemIndex === 0} aria-label="Move item up">↑</button>
                          <button type="button" class="h-10 w-10 rounded-full border border-stone-200 disabled:opacity-30" on:click={() => moveItem(category.id, itemIndex, 1)} disabled={itemIndex === category.items.length - 1} aria-label="Move item down">↓</button>
                          <button type="button" class="h-10 w-10 rounded-full border border-red-100 text-red-600" on:click={() => removeItem(category.id, item.id)} aria-label="Remove item">×</button>
                        </div>
                      </div>
                    </div>
                  {/each}
                </div>
                <button type="button" class="mt-4 w-full rounded-xl border border-dashed border-stone-300 bg-white px-4 py-3 text-sm font-medium text-stone-700 hover:border-stone-400" on:click={() => addItem(category.id)}>+ Add item</button>
              </div>
            {/if}
          </article>
        {/each}
      </div>
    {/if}

    <div class="sticky bottom-4 z-10 mt-5 rounded-2xl border border-stone-200 bg-white/95 p-3 shadow-[0_16px_40px_rgba(45,53,46,0.16)] backdrop-blur sm:flex sm:items-center sm:justify-between">
      <p class="px-2 text-sm text-stone-600">{hasChanges ? "You have unsaved menu changes." : "Digital menu is saved."}</p>
      <button type="button" class={`mt-3 w-full text-sm sm:mt-0 sm:w-auto ${hasChanges ? "btn-primary" : "btn-secondary"}`} on:click={saveMenu} disabled={!hasChanges || saving}>{saving ? "Saving..." : hasChanges ? "Save menu" : "Saved"}</button>
    </div>
  {/if}
</section>
