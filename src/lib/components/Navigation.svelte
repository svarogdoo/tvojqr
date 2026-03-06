<script lang="ts">
  import { language, languages } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  function changeLanguage(lang: "en" | "sr" | "ru" | "el") {
    language.set(lang);
  }

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Navigation -->
<nav class="fixed top-0 w-full z-50 border-b border-black/5 bg-white/75 backdrop-blur-xl">
  <div
    class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between"
  >
    <a href="/" class="text-2xl font-semibold tracking-tight text-stone-900">HostingQr</a>
    <div class="hidden md:flex space-x-8 items-center">
      <a
        href="/create-new"
        class="inline-flex items-center rounded-full bg-stone-900 px-4 py-2 text-sm font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
      >
        {t.hero.cta}
      </a>
      <a
        href="/#services"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.services}
      </a>
      <a
        href="/restoran"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.examples}
      </a>
      <a
        href="/#contact"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.contact}
      </a>
    </div>
    <div class="flex gap-2">
      {#each languages as lang (lang.code)}
        <button
          on:click={() =>
            changeLanguage(lang.code as "en" | "sr" | "ru" | "el")}
          class="rounded-full px-3 py-1 text-sm transition-all {currentLang === lang.code
            ? 'bg-stone-900 text-white shadow-sm'
            : 'border border-stone-200 bg-white/90 text-stone-600 hover:border-stone-300 hover:text-stone-900'}"
        >
          {lang.flag}
        </button>
      {/each}
    </div>
  </div>
</nav>
