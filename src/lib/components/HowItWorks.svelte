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
<section id="how-it-works" class="py-20 px-4 sm:px-6 lg:px-8 bg-white">
  <div class="max-w-6xl mx-auto">
    <div class="text-center mb-16">
      <h2 class="text-3xl sm:text-4xl font-bold text-olive-900 mb-4">
        {t.howItWorks.title}
      </h2>
      <p class="text-olive-700 text-lg max-w-2xl mx-auto">{t.howItWorks.subtitle}</p>
    </div>

    <!-- Timeline with cards -->
    <div class="relative">
      <!-- Connecting line (hidden on mobile) -->
      <div
        class="hidden lg:block absolute top-24 left-0 right-0 h-px bg-olive-300"
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
                class="relative w-24 h-24 bg-olive-700 rounded-full flex items-center justify-center shadow-md transform hover:scale-105 transition-transform duration-300"
              >
                <div
                  class="absolute inset-0 border border-white/10 rounded-full"
                ></div>
                <span class="relative text-white text-3xl font-bold"
                  >{idx + 1}</span
                >
              </div>
            </div>

            <!-- Card -->
            <div
              class="bg-white p-6 rounded-xl border border-olive-200 hover:border-olive-300 hover:shadow-md transition-all duration-300"
            >
              <h3 class="text-xl font-bold text-olive-900 mb-3">
                {step.title}
              </h3>
              <p class="text-olive-700 text-sm leading-relaxed">
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
      <p class="text-olive-700 mb-6">{t.howItWorks.ctaDescription}</p>
      <a
        href="/create-new"
        class="inline-block px-8 py-4 bg-olive-700 text-white rounded-xl font-semibold hover:bg-olive-800 hover:-translate-y-0.5 transition-all duration-300 shadow-md"
      >
        {t.howItWorks.cta}
      </a>
    </div>
  </div>
</section>
