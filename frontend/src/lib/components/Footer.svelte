<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  const currentYear = new Date().getFullYear();

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
  $: copyright = t.footer.copyright.replace("2025", String(currentYear));
</script>

<!-- Footer -->
<footer class="px-4 py-10 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-6xl pt-8 text-center text-stone-500">
    <p class="font-medium text-stone-700">{copyright}</p>
  </div>
</footer>
