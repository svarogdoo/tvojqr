<script lang="ts">
  import { language, type LanguageCode } from "$lib/stores/language";
  import { homepageCopy } from "$lib/homepageCopy";

  let currentLang: LanguageCode = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: copy = homepageCopy[currentLang].family;
  let faqs: any = [];
  $: faqs = copy.faq as any;

</script>

<section class="border-y border-black/8 bg-[rgba(236,240,234,0.88)] px-4 py-20 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-5xl text-center">
    <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
      {copy.label}
    </p>
    <h2 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
      {copy.title}
    </h2>
    <p class="mx-auto mt-5 max-w-3xl text-lg leading-8 text-stone-600">
      {copy.body}
    </p>

    <a
      href="/contact"
      class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3.5 text-sm font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
    >
      {copy.cta}
    </a>

    <div class="mt-16 text-left">
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500 text-center">
        {copy.faqLabel}
      </p>
      <div class="mx-auto mt-6 grid max-w-4xl gap-4">
        {#each faqs as faq}
          <details class="group overflow-hidden rounded-[1.5rem] border border-stone-200 bg-white/85 shadow-sm" open={faq.id === "how-does-it-work"}>
            <summary class="flex cursor-pointer list-none items-center justify-between gap-4 px-5 py-4 text-left">
              <span class="text-sm font-medium text-stone-900">{faq.question}</span>
              <svg
                class="h-4 w-4 shrink-0 text-stone-400 transition-transform duration-300 group-open:rotate-180"
                viewBox="0 0 20 20"
                fill="none"
                stroke="currentColor"
                stroke-width="1.8"
                aria-hidden="true"
              >
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 8l5 5 5-5" />
              </svg>
            </summary>

            <div class="px-5 pb-5">
              {#if faq.answer}
                <ol class="space-y-3 text-sm leading-7 text-stone-600">
                  {#each faq.answer as step, idx}
                    <li>
                      <span class="font-medium text-stone-900">{idx + 1}.</span> {step}
                    </li>
                  {/each}
                </ol>
                <p class="mt-3 text-sm leading-7 text-stone-600">{faq.closing}</p>
              {:else}
                <p class="text-sm leading-7 text-stone-600">{faq.answerText}</p>
              {/if}
            </div>
          </details>
        {/each}
      </div>
    </div>

  </div>
</section>
