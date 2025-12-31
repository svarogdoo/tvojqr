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
<section id="services" class="py-20 px-4 sm:px-6 lg:px-8 bg-olive-50">
  <div class="max-w-6xl mx-auto">
    <!-- Title Section -->
    <div class="text-center mb-16">
      <h2 class="text-4xl sm:text-5xl font-bold text-olive-900 mb-4">
        {t.services.title}
      </h2>
      <p class="text-olive-700 text-lg max-w-2xl mx-auto">
        Comprehensive solutions to elevate your brand and reach
      </p>
    </div>

    <!-- Services Grid: 3 columns, 2 rows -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-12">
      {#each t.services.list as service, idx (service.title)}
        <!-- Featured cards: indices 0, 2, 4 (Graphic Design, QR Codes, Online Hosting) -->
        {#if idx === 0 || idx === 2 || idx === 4}
          <!-- Dark featured cards with olive accents -->
          <div
            class="group relative bg-linear-to-br from-lime-900 to-lime-800 p-8 rounded-3xl hover:scale-105 hover:shadow-2xl transition-all duration-300 border border-olive-800 hover:border-olive-600 overflow-hidden"
          >
            <!-- Animated background -->
            <div
              class="absolute top-0 right-0 w-40 h-40 bg-olive-700/20 rounded-full blur-3xl group-hover:scale-150 transition-transform duration-500"
            ></div>

            <div class="relative z-10">
              <div
                class="w-12 h-12 bg-olive-300 rounded-xl flex items-center justify-center mb-4 text-olive-900 text-2xl group-hover:scale-110 transition-transform duration-300"
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
                href="#contact"
                class="text-olive-300 font-semibold text-sm hover:text-olive-200 transition-colors duration-300"
              >
                Read more →
              </a>
            </div>
          </div>
        {:else}
          <!-- Light cards with olive accents -->
          <div
            class="group relative bg-white p-8 rounded-3xl hover:shadow-xl hover:scale-105 transition-all duration-300 border-2 border-olive-200 hover:border-olive-400 overflow-hidden"
          >
            <!-- Subtle background gradient -->
            <div
              class="absolute top-0 right-0 w-32 h-32 bg-olive-100 rounded-full blur-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-300"
            ></div>

            <div class="relative z-10">
              <div
                class="w-12 h-12 bg-linear-to-br from-olive-700 to-olive-600 rounded-xl flex items-center justify-center mb-4 text-white text-2xl group-hover:scale-110 transition-transform duration-300"
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
                href="#contact"
                class="text-olive-700 font-semibold text-sm hover:text-olive-800 transition-colors duration-300"
              >
                Read more →
              </a>
            </div>
          </div>
        {/if}
      {/each}
    </div>

    <!-- Call to action -->
    <div class="text-center">
      <a
        href="#contact"
        class="inline-block px-8 py-4 bg-olive-700 text-white rounded-full font-bold hover:bg-olive-800 hover:scale-105 transition-all duration-300 shadow-lg"
      >
        Let's Talk
      </a>
    </div>
  </div>
</section>
