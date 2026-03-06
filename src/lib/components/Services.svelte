<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Services Section -->
<section id="services" class="py-20 px-4 sm:px-6 lg:px-8 bg-olive-100">
  <div class="max-w-6xl mx-auto">
    <!-- Title Section -->
    <div class="text-center mb-16">
      <h2 class="text-4xl sm:text-5xl font-bold text-olive-900 mb-4">
        {t.services.title}
      </h2>
      <p class="text-olive-700 text-lg max-w-2xl mx-auto">{t.services.subtitle}</p>
    </div>

    <!-- Services Grid: 3 columns, 2 rows -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-12">
      {#each t.services.list as service, idx (service.title)}
        <!-- Featured cards: indices 0, 2, 4 (Graphic Design, QR Codes, Online Hosting) -->
        {#if idx === 0 || idx === 2 || idx === 4}
          <!-- Featured cards (minimal accent version) -->
          <div
            class="group relative bg-olive-800 p-8 rounded-3xl hover:-translate-y-1 hover:shadow-xl transition-all duration-300 border border-olive-700 overflow-hidden"
          >
            <div
              class="absolute top-0 right-0 w-32 h-32 bg-olive-300 rounded-full blur-3xl opacity-15 group-hover:opacity-25 transition-opacity duration-300"
            ></div>

            <div class="relative z-10">
              <div
                class="w-12 h-12 bg-white/10 border border-white/10 rounded-xl flex items-center justify-center mb-4 text-white text-2xl group-hover:scale-105 transition-transform duration-300"
              >
                {service.icon}
              </div>
              <h3 class="text-xl font-bold text-white mb-3">
                {service.title}
              </h3>
              <p class="text-olive-100 text-sm mb-6 leading-relaxed">
                {service.description}
              </p>
              <a
                href="/create-new"
                class="text-olive-300 font-semibold text-sm hover:text-olive-100 transition-colors duration-300"
              >
                {t.services.readMore} ->
              </a>
            </div>
          </div>
        {:else}
          <!-- Standard cards -->
          <div
            class="group relative bg-white p-8 rounded-3xl hover:shadow-lg hover:-translate-y-1 transition-all duration-300 border border-olive-200 hover:border-olive-300 overflow-hidden"
          >
            <div
              class="absolute top-0 right-0 w-28 h-28 bg-olive-100 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300"
            ></div>

            <div class="relative z-10">
              <div
                class="w-12 h-12 bg-olive-100 rounded-xl flex items-center justify-center mb-4 text-olive-800 text-2xl group-hover:scale-105 transition-transform duration-300"
              >
                {service.icon}
              </div>
              <h3 class="text-xl font-bold text-olive-900 mb-3">
                {service.title}
              </h3>
              <p class="text-olive-700 text-sm mb-6 leading-relaxed">
                {service.description}
              </p>
              <a
                href="/create-new"
                class="text-olive-800 font-semibold text-sm hover:text-olive-900 transition-colors duration-300"
              >
                {t.services.readMore} ->
              </a>
            </div>
          </div>
        {/if}
      {/each}
    </div>

    <!-- Call to action -->
    <div class="text-center">
      <a
        href="/create-new"
        class="inline-block px-8 py-4 bg-olive-700 text-white rounded-xl font-bold hover:bg-olive-800 hover:-translate-y-0.5 transition-all duration-300 shadow-md"
      >
        {t.services.cta}
      </a>
    </div>
  </div>
</section>
