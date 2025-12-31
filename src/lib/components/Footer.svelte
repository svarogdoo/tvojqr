<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Footer -->
<footer class="bg-gray-900 text-gray-400 py-8 px-4 sm:px-6 lg:px-8">
  <div class="max-w-6xl mx-auto text-center">
    <p>{t.footer.copyright}</p>
    <p class="text-sm mt-2">{t.footer.tagline}</p>
  </div>
</footer>
