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
  class="relative pt-20 px-4 sm:px-6 lg:px-8 bg-linear-to-b from-gray-50 to-white overflow-hidden"
>
  <div class="max-w-7xl mx-auto">
    <!-- Content -->
    <div class="text-center mb-12 max-w-3xl mx-auto">
      <h1
        class="text-5xl sm:text-6xl lg:text-7xl font-bold text-gray-900 mb-6 leading-tight animate-fade-in"
      >
        {t.hero.title}
      </h1>
      <p class="text-lg sm:text-xl text-gray-600 mb-8 leading-relaxed">
        {t.hero.description}
      </p>
      <div class="flex flex-col sm:flex-row gap-4 justify-center">
        <a
          href="/create-new"
          class="px-8 py-4 bg-olive-700 text-white rounded-lg font-semibold hover:bg-olive-800 hover:scale-105 transition-all duration-300 shadow-lg text-center"
        >
          {t.hero.cta}
        </a>
        <a
          href="/restoran"
          class="px-8 py-4 border-2 border-olive-700 text-olive-700 rounded-lg font-semibold hover:bg-olive-50 transition-all duration-300 text-center"
        >
          {t.hero.secondary}
        </a>
      </div>
    </div>

    <!-- Image Below with Top Half Visible, Bottom Half Cut Off -->
    <div
      class="relative mx-auto flex items-start justify-center -mb-64"
      style="width: 70%; height: 768px; max-width: none;"
    >
      <!-- Shadow Background -->
      <div
        class="absolute top-0 left-1/2 transform -translate-x-1/2 w-full h-96 bg-olive-600/10 rounded-full blur-3xl"
      ></div>

      <!-- Image Container -->
      <div class="relative w-full h-full">
        <!-- Shadow Layer -->
        <div
          class="absolute bottom-0 left-4 right-4 w-5/6 h-20 bg-gray-900/20 rounded-full blur-2xl mx-auto"
        ></div>

        <!-- Image -->
        <img
          src={heroImage}
          alt="Menu Design"
          class="relative w-full h-full object-cover object-center rounded-3xl shadow-2xl transform hover:scale-105 transition-transform duration-500"
        />
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
