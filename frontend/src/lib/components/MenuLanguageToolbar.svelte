<script lang="ts">
  import type { ProjectLanguageVariant } from "$lib/types/projects";

  export let languages: ProjectLanguageVariant[];
  export let activeLanguageCode: string;
  export let options: Array<{ code: string; name: string; flag: string }>;
  export let adding = false;
  export let removingLanguageCode = "";
  export let changingDefaultLanguageCode = "";
  export let onAdd: (languageCode: string) => void | Promise<void>;
  export let onRemove: (language: ProjectLanguageVariant) => void | Promise<void>;
  export let onMakeDefault: (language: ProjectLanguageVariant) => void | Promise<void>;

  let dropdownOpen = false;

  $: sortedLanguages = [...languages].sort((a, b) => a.sortOrder - b.sortOrder);
  $: availableOptions = options.filter((option) => !languages.some((language) => language.languageCode === option.code));
  $: mutationBusy = adding || Boolean(removingLanguageCode) || Boolean(changingDefaultLanguageCode);

  function languageMeta(languageCode: string) {
    return options.find((option) => option.code === languageCode) ?? {
      code: languageCode,
      name: languageCode.toUpperCase(),
      flag: "🌐",
    };
  }

  async function addLanguage(languageCode: string) {
    dropdownOpen = false;
    await onAdd(languageCode);
    activeLanguageCode = languageCode;
  }
</script>

<div class="rounded-2xl border border-stone-200 bg-white p-3 shadow-sm">
  <div class="flex items-center gap-2">
    <span class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-stone-100 text-stone-600" aria-hidden="true">
      <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
        <circle cx="12" cy="12" r="9" />
        <path stroke-linecap="round" d="M3.5 9h17M3.5 15h17M12 3c2.2 2.5 3.3 5.5 3.3 9S14.2 18.5 12 21M12 3C9.8 5.5 8.7 8.5 8.7 12S9.8 18.5 12 21" />
      </svg>
    </span>

    <div class="min-w-0 flex-1 overflow-x-auto pb-1">
      <div class="flex min-w-max items-center gap-2" role="tablist" aria-label="Menu language">
        {#each sortedLanguages as language}
          {@const meta = languageMeta(language.languageCode)}
          <div class={`flex items-center rounded-full border transition-colors ${activeLanguageCode === language.languageCode ? "border-stone-900 bg-stone-900 text-white" : "border-stone-200 bg-stone-50 text-stone-600"}`}>
            <button type="button" role="tab" aria-selected={activeLanguageCode === language.languageCode} on:click={() => activeLanguageCode = language.languageCode} class="flex min-h-10 items-center gap-2 px-3 text-sm font-medium">
              <span>{meta.flag}</span>
              <span>{language.displayName}</span>
              {#if language.isDefault}<span class={`text-[0.62rem] uppercase tracking-[0.1em] ${activeLanguageCode === language.languageCode ? "text-white/60" : "text-stone-400"}`}>Default</span>{/if}
            </button>
            {#if !language.isDefault}
              <button type="button" on:click={() => onMakeDefault(language)} disabled={mutationBusy} class={`flex h-8 w-8 items-center justify-center rounded-full text-sm ${activeLanguageCode === language.languageCode ? "text-white/65 hover:bg-white/10 hover:text-white" : "text-stone-400 hover:bg-stone-200 hover:text-stone-700"}`} aria-label={`Make ${language.displayName} the default language`} title="Make default">
                {changingDefaultLanguageCode === language.languageCode ? "…" : "☆"}
              </button>
              <button type="button" on:click={() => onRemove(language)} disabled={mutationBusy} class={`mr-1 flex h-8 w-8 items-center justify-center rounded-full text-base ${activeLanguageCode === language.languageCode ? "text-white/65 hover:bg-white/10 hover:text-white" : "text-stone-400 hover:bg-stone-200 hover:text-stone-700"}`} aria-label={`Remove ${language.displayName}`}>
                {removingLanguageCode === language.languageCode ? "…" : "×"}
              </button>
            {/if}
          </div>
        {/each}
      </div>
    </div>

    <div class="relative shrink-0">
      <button type="button" on:click={() => dropdownOpen = !dropdownOpen} disabled={mutationBusy || availableOptions.length === 0} class="flex h-10 w-10 items-center justify-center rounded-full border border-stone-300 bg-white text-xl leading-none text-stone-700 transition-colors hover:bg-stone-100 disabled:opacity-40" aria-label="Add menu language" aria-expanded={dropdownOpen}>+</button>
      {#if dropdownOpen}
        <div class="absolute right-0 z-30 mt-2 max-h-64 min-w-52 overflow-y-auto rounded-2xl border border-stone-200 bg-white p-1.5 shadow-[0_18px_45px_rgba(45,53,46,0.16)]">
          {#each availableOptions as option}
            <button type="button" on:click={() => addLanguage(option.code)} class="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm text-stone-700 hover:bg-stone-100">
              <span>{option.flag}</span><span>{option.name}</span>
            </button>
          {/each}
        </div>
      {/if}
    </div>
  </div>
</div>
