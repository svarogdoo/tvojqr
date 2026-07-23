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
    <nav class="mt-4 flex flex-wrap items-center justify-center gap-x-5 gap-y-2 text-sm" aria-label="Footer navigation">
      <a href="/terms" class="transition-colors hover:text-stone-900">Terms</a>
      <a href="/privacy" class="transition-colors hover:text-stone-900">Privacy</a>
      <a href="/contact" class="transition-colors hover:text-stone-900">Contact</a>
    </nav>
  </div>
</footer>
