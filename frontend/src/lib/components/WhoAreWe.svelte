<script lang="ts">
  import { slide } from "svelte/transition";

  const faqs = [
    {
      id: "how-does-it-work",
      question: "How does this work?",
      answer: [
        "You create your account",
        "You click to start your new project",
        "Upload your offer, select url, background color",
        "Add translated offering in the language section, or do it later",
        "QR is already there, pick your style and download it for printing",
      ],
      closing: "You are all ready to go!",
    },
    {
      id: "translate-page",
      question: "Does the software translate the page?",
      answerText:
        "No. But we can help with that as well, just send us over what you need.",
    },
    {
      id: "more-languages",
      question: "Can I add more languages later?",
      answerText:
        "Yes. You can add translations when you need them, without printing a second version of everything.",
    },
    {
      id: "tech-setup",
      question: "Do I need to understand technical setup?",
      answerText: "No. Send us what you need and we’ll help you get it ready.",
    },
  ];

  let openFaq = "how-does-it-work";
</script>

<section class="border-y border-black/8 bg-[rgba(236,240,234,0.88)] px-4 py-20 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-5xl text-center">
    <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
      Who are we?
    </p>
    <h2 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
      A small family-run team, built to care.
    </h2>
    <p class="mx-auto mt-5 max-w-3xl text-lg leading-8 text-stone-600">
      We’re experienced coders and graphic designers working together under one roof. We keep it humble, stay hands-on, and put real care into the details.
    </p>

    <a
      href="/contact"
      class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3.5 text-sm font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
    >
      Get in touch
    </a>

    <div class="mt-16 text-left">
      <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500 text-center">
        FAQ
      </p>
      <div class="mx-auto mt-6 grid max-w-4xl gap-4">
        {#each faqs as faq}
          <div class="overflow-hidden rounded-[1.5rem] border border-stone-200 bg-white/85 shadow-sm">
            <button
              type="button"
              on:click={() => (openFaq = openFaq === faq.id ? "" : faq.id)}
              class="flex w-full items-center justify-between gap-4 px-5 py-4 text-left"
              aria-expanded={openFaq === faq.id}
            >
              <span class="text-sm font-medium text-stone-900">{faq.question}</span>
              <svg
                class={`h-4 w-4 shrink-0 text-stone-400 transition-transform duration-300 ${openFaq === faq.id ? "rotate-180" : ""}`}
                viewBox="0 0 20 20"
                fill="none"
                stroke="currentColor"
                stroke-width="1.8"
                aria-hidden="true"
              >
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 8l5 5 5-5" />
              </svg>
            </button>

            {#if openFaq === faq.id}
              <div class="px-5 pb-5" transition:slide>
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
            {/if}
          </div>
        {/each}
      </div>
    </div>

  </div>
</section>
