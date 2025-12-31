<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Examples Section -->
<section id="examples" class="py-20 px-4 sm:px-6 lg:px-8 bg-white">
  <div class="max-w-6xl mx-auto">
    <h2 class="text-3xl sm:text-4xl font-bold text-center text-gray-900 mb-12">
      {t.examples.title}
    </h2>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
      {#each t.examples.list as example (example.title)}
        <div
          class="group overflow-hidden rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300"
        >
          <div
            class="h-48 bg-linear-to-br from-olive-700 to-olive-800 flex items-center justify-center group-hover:scale-110 transition-transform duration-500"
          >
            <div class="text-center text-white">
              <h3 class="text-2xl font-bold">{example.title}</h3>
              <p class="mt-2 text-olive-100">{example.description}</p>
            </div>
          </div>
          <div class="p-6 bg-gray-50">
            <button
              class="w-full px-4 py-2 border border-olive-700 text-olive-700 rounded-lg hover:bg-olive-50 transition-colors duration-300"
            >
              {t.examples.viewDetails}
            </button>
          </div>
        </div>
      {/each}
    </div>
  </div>
</section>
