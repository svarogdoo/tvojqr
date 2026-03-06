<script lang="ts">
  import heroImage from "$lib/assets/hero-image.jpg";
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Hero Section -->
<section
  class="relative overflow-hidden px-4 pt-24 sm:px-6 lg:px-8"
>
  <div class="max-w-7xl mx-auto">
    <div class="grid items-center gap-14 pb-12 lg:grid-cols-[1.05fr_0.95fr] lg:pb-20">
      <div class="max-w-2xl">
        <div class="mb-5 inline-flex items-center rounded-full border border-stone-200 bg-white/80 px-4 py-1.5 text-sm font-medium text-stone-600 shadow-sm backdrop-blur-sm">
          Simple hosting for QR pages
        </div>
        <h1
          class="max-w-3xl text-5xl font-semibold leading-tight tracking-tight text-stone-900 sm:text-6xl lg:text-7xl animate-fade-in"
        >
          {t.hero.title}
        </h1>
        <p class="mt-6 max-w-xl text-lg leading-8 text-stone-600 sm:text-xl">
          {t.hero.description}
        </p>
        <div class="mt-10 flex flex-col gap-3 sm:flex-row">
          <a
            href="/create-new"
            class="inline-flex items-center justify-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
          >
            {t.hero.cta}
          </a>
          <a
            href="/#how-it-works"
            class="inline-flex items-center justify-center rounded-full border border-stone-200 bg-white/85 px-6 py-3.5 text-base font-medium text-stone-700 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-stone-300 hover:text-stone-900"
          >
            {t.hero.secondary}
          </a>
        </div>
        <div class="mt-12 grid max-w-xl grid-cols-1 gap-3 sm:grid-cols-3">
          <div class="rounded-2xl border border-stone-200/80 bg-white/75 px-4 py-4 shadow-sm backdrop-blur-sm">
            <div class="text-sm text-stone-500">Step 1</div>
            <div class="mt-1 font-medium text-stone-900">Upload files</div>
          </div>
          <div class="rounded-2xl border border-stone-200/80 bg-white/75 px-4 py-4 shadow-sm backdrop-blur-sm">
            <div class="text-sm text-stone-500">Step 2</div>
            <div class="mt-1 font-medium text-stone-900">Choose slug</div>
          </div>
          <div class="rounded-2xl border border-stone-200/80 bg-white/75 px-4 py-4 shadow-sm backdrop-blur-sm">
            <div class="text-sm text-stone-500">Step 3</div>
            <div class="mt-1 font-medium text-stone-900">Share QR</div>
          </div>
        </div>
      </div>

      <div class="relative mx-auto w-full max-w-xl lg:max-w-none">
        <div
          class="absolute inset-x-10 top-8 h-64 rounded-full bg-[rgba(204,212,201,0.55)] blur-3xl"
        ></div>

        <div class="relative overflow-hidden rounded-[2rem] border border-white/60 bg-white/70 p-3 shadow-[0_24px_60px_rgba(45,53,46,0.12)] backdrop-blur-sm">
          <div class="mb-3 flex items-center gap-2 px-2 py-1">
            <span class="h-2.5 w-2.5 rounded-full bg-stone-300"></span>
            <span class="h-2.5 w-2.5 rounded-full bg-stone-300"></span>
            <span class="h-2.5 w-2.5 rounded-full bg-stone-300"></span>
          </div>
          <img
            src={heroImage}
            alt="Menu Design"
            class="relative aspect-[4/5] w-full rounded-[1.5rem] object-cover object-center"
          />
        </div>
      </div>
    </div>
  </div>
</section>

<style>
  @keyframes fadeIn {
    from {
      opacity: 0;
      transform: translateY(20px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  :global(.animate-fade-in) {
    animation: fadeIn 0.8s ease-out;
  }
</style>
