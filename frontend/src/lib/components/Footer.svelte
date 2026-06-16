<script lang="ts">
  import { language, type LanguageCode } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  const currentYear = new Date().getFullYear();

  let currentLang: LanguageCode = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  const localizedTranslations = translations as any;

  $: t = localizedTranslations[currentLang];
  $: copyright = t.footer.copyright.replace("2025", String(currentYear));
</script>

<!-- Footer -->
<footer class="bg-[rgba(243,244,246,0.98)] px-4 py-10 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-6xl pt-8 text-center text-stone-500">
    <p class="font-medium text-stone-700">{copyright}</p>
  </div>
</footer>
