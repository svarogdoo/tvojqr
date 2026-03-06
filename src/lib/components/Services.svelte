<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"] as any;
</script>

<!-- Services Section -->
<section id="services" class="px-4 py-20 sm:px-6 lg:px-8">
  <div class="max-w-6xl mx-auto">
    <div class="mx-auto mb-16 max-w-3xl text-center">
      <h2 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
        {t.services.title}
      </h2>
      <p class="mt-4 text-lg leading-8 text-stone-600">{t.services.subtitle}</p>
    </div>

    <div class="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
      {#each t.services.list as service, idx (service.title)}
        <div
          class="group relative overflow-hidden rounded-[1.75rem] border border-black/6 bg-white/78 p-8 shadow-[0_16px_40px_rgba(45,53,46,0.06)] backdrop-blur-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_24px_50px_rgba(45,53,46,0.09)]"
        >
          <div class="absolute right-0 top-0 h-28 w-28 rounded-full bg-[rgba(210,218,206,0.45)] blur-3xl transition-opacity duration-300 group-hover:opacity-100"></div>

          <div class="relative z-10">
            <div class="mb-6 inline-flex h-12 min-w-12 items-center justify-center rounded-2xl bg-stone-100 px-4 text-sm font-medium text-stone-700">
              {idx + 1}
            </div>
            <h3 class="mb-3 text-xl font-semibold text-stone-900">
              {service.title}
            </h3>
            <p class="mb-6 text-sm leading-7 text-stone-600">
              {service.description}
            </p>
            <a
              href="/create-new"
              class="text-sm font-medium text-stone-900 transition-colors duration-300 hover:text-stone-600"
            >
              {t.services.readMore} ->
            </a>
          </div>
        </div>
      {/each}
    </div>

    <div class="mt-12 text-center">
      <a
        href="/create-new"
        class="inline-flex items-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
      >
        {t.services.cta}
      </a>
    </div>
  </div>
</section>
