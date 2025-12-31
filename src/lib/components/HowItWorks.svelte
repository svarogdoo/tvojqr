<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- How It Works -->
<section class="py-20 px-4 sm:px-6 lg:px-8 bg-white">
  <div class="max-w-6xl mx-auto">
    <div class="text-center mb-16">
      <h2 class="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
        {t.howItWorks.title}
      </h2>
      <p class="text-gray-600 text-lg max-w-2xl mx-auto">
        A simple, transparent process to bring your vision to life
      </p>
    </div>

    <!-- Timeline with cards -->
    <div class="relative">
      <!-- Connecting line (hidden on mobile) -->
      <div
        class="hidden lg:block absolute top-24 left-0 right-0 h-1 bg-gradient-to-r from-olive-200 via-olive-400 to-olive-200"
      ></div>

      <!-- Steps Grid -->
      <div
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative z-10"
      >
        {#each t.howItWorks.steps as step, idx (step.title)}
          <div class="text-center">
            <!-- Icon Circle with number -->
            <div class="flex justify-center mb-6">
              <div
                class="relative w-24 h-24 bg-olive-700 rounded-full flex items-center justify-center shadow-lg transform hover:scale-110 transition-transform duration-300"
              >
                <div
                  class="absolute inset-0 bg-olive-600 rounded-full animate-pulse opacity-20"
                ></div>
                <span class="relative text-white text-3xl font-bold"
                  >{idx + 1}</span
                >
              </div>
            </div>

            <!-- Card -->
            <div
              class="bg-gray-50 p-6 rounded-xl border-2 border-transparent hover:border-olive-300 transition-all duration-300"
            >
              <h3 class="text-xl font-bold text-gray-900 mb-3">
                {step.title}
              </h3>
              <p class="text-gray-600 text-sm leading-relaxed">
                {step.description}
              </p>
            </div>

            <!-- Arrow pointing down (mobile only) -->
            {#if idx < t.howItWorks.steps.length - 1}
              <div class="lg:hidden mt-6 flex justify-center">
                <div class="text-olive-400 text-2xl">↓</div>
              </div>
            {/if}
          </div>
        {/each}
      </div>
    </div>

    <!-- Bottom CTA -->
    <div class="text-center mt-16">
      <p class="text-gray-600 mb-6">Ready to get started?</p>
      <a
        href="#contact"
        class="inline-block px-8 py-4 bg-olive-700 text-white rounded-lg font-semibold hover:bg-olive-800 hover:scale-105 transition-all duration-300 shadow-lg"
      >
        Start Your Project
      </a>
    </div>
  </div>
</section>
