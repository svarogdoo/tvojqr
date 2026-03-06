<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<section id="examples" class="border-y border-black/8 bg-[rgba(220,228,216,0.92)] px-4 py-20 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-6xl">
    <div class="mx-auto mb-14 max-w-3xl text-center">
      <h2 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
        {t.examples.title}
      </h2>
      <p class="mt-4 text-lg leading-8 text-stone-600">
        Open a live sample page to see how a hosted menu looks when someone scans the QR code.
      </p>
    </div>

    <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
      {#each t.examples.list as example, idx (example.title)}
        <a
          href={idx === 0 ? "/restoran" : "/create-new"}
          class="group overflow-hidden rounded-[1.75rem] border border-black/8 bg-white/94 shadow-[0_16px_40px_rgba(45,53,46,0.08)] backdrop-blur-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_24px_50px_rgba(45,53,46,0.12)]"
        >
          <div class="border-b border-stone-200 bg-[rgba(248,247,243,0.96)] px-6 py-8">
            <div class="inline-flex rounded-full bg-[rgba(239,236,230,0.95)] px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-stone-600 shadow-sm">
              {idx === 0 ? "Live example" : "Use case"}
            </div>
            <h3 class="mt-4 text-2xl font-semibold text-stone-900">{example.title}</h3>
            <p class="mt-3 text-sm leading-7 text-stone-600">{example.description}</p>
          </div>
          <div class="flex items-center justify-between px-6 py-5">
            <span class="text-sm font-medium text-stone-900">
              {idx === 0 ? t.examples.viewDetails : "Start your own"}
            </span>
            <span class="text-stone-400 transition-transform duration-300 group-hover:translate-x-1">-></span>
          </div>
        </a>
      {/each}
    </div>
  </div>
</section>
