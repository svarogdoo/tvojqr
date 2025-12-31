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
<nav class="fixed top-0 w-full bg-white/95 backdrop-blur-md z-50 shadow-sm">
  <div
    class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between"
  >
    <div class="text-2xl font-bold text-olive-700">TvojQR</div>
    <div class="hidden md:flex space-x-8 items-center">
      <a
        href="#services"
        class="text-gray-700 hover:text-olive-700 transition-colors"
      >
        {t.nav.services}
      </a>
      <a
        href="#examples"
        class="text-gray-700 hover:text-olive-700 transition-colors"
      >
        {t.nav.examples}
      </a>
      <a
        href="#contact"
        class="text-gray-700 hover:text-olive-700 transition-colors"
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
            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}"
        >
          {lang.flag}
        </button>
      {/each}
    </div>
  </div>
</nav>
