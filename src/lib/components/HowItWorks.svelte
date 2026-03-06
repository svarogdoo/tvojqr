<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"] as any;
</script>

<!-- How It Works -->
<section
  id="how-it-works"
  class="border-y border-black/8 bg-[rgba(236,240,234,0.88)] px-4 py-20 sm:px-6 lg:px-8"
>
  <div class="max-w-6xl mx-auto">
    <div class="mx-auto mb-16 max-w-3xl text-center">
      <h2 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
        {t.howItWorks.title}
      </h2>
      <p class="mt-4 text-lg leading-8 text-stone-600">{t.howItWorks.subtitle}</p>
    </div>

    <div class="relative">
      <div
        class="absolute left-0 right-0 top-11 hidden h-px bg-stone-200 lg:block"
      ></div>

      <div
        class="relative z-10 grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4"
      >
        {#each t.howItWorks.steps as step, idx (step.title)}
          <div class="text-center lg:text-left">
            <div class="mb-6 flex justify-center lg:justify-start">
              <div
                class="flex h-12 w-12 items-center justify-center rounded-2xl border border-stone-300 bg-white text-sm font-semibold text-stone-900 shadow-sm"
              >
                {idx + 1}
              </div>
            </div>

            <div
              class="rounded-[1.5rem] border border-black/8 bg-white/92 p-6 shadow-[0_16px_40px_rgba(45,53,46,0.07)] backdrop-blur-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_24px_50px_rgba(45,53,46,0.1)]"
            >
              <h3 class="mb-3 text-xl font-semibold text-stone-900">
                {step.title}
              </h3>
              <p class="text-sm leading-7 text-stone-600">
                {step.description}
              </p>
            </div>

            {#if idx < t.howItWorks.steps.length - 1}
              <div class="lg:hidden mt-6 flex justify-center">
                <div class="text-stone-300 text-2xl">↓</div>
              </div>
            {/if}
          </div>
        {/each}
      </div>
    </div>

    <div class="text-center mt-16">
      <p class="mb-6 text-stone-600">{t.howItWorks.ctaDescription}</p>
      <a
        href="/create-new"
        class="inline-flex items-center rounded-full border border-stone-300 bg-white px-6 py-3.5 text-base font-medium text-stone-900 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-stone-400"
      >
        {t.howItWorks.cta}
      </a>
    </div>
  </div>
</section>
