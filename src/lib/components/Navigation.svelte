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
<nav class="fixed top-0 w-full bg-white/90 backdrop-blur-md z-50 shadow-sm border-b border-olive-200">
  <div
    class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between"
  >
    <a href="/" class="text-2xl font-bold text-olive-900">HostingQR</a>
    <div class="hidden md:flex space-x-8 items-center">
      <a
        href="/create-new"
        class="text-white font-semibold bg-olive-700 px-4 py-2 rounded-xl hover:bg-olive-800 transition-all shadow-sm hover:-translate-y-0.5"
      >
        {t.hero.cta}
      </a>
      <a
        href="/#services"
        class="text-olive-700 hover:text-olive-900 transition-colors"
      >
        {t.nav.services}
      </a>
      <a
        href="/restoran"
        class="text-olive-700 hover:text-olive-900 transition-colors"
      >
        {t.nav.examples}
      </a>
      <a
        href="/#contact"
        class="text-olive-700 hover:text-olive-900 transition-colors"
      >
        {t.nav.contact}
      </a>
    </div>
    <div class="flex gap-2">
      {#each languages as lang (lang.code)}
        <button
          on:click={() =>
            changeLanguage(lang.code as "en" | "sr" | "ru" | "el")}
          class="px-3 py-1 rounded-lg transition-all {currentLang === lang.code
            ? 'bg-olive-700 text-white'
            : 'border border-olive-200 bg-white text-olive-800 shadow-sm hover:-translate-y-0.5 hover:border-olive-300'}"
        >
          {lang.flag}
        </button>
      {/each}
    </div>
  </div>
</nav>
