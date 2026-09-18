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

<section
  class="border-y border-black/8 bg-[rgba(236,240,234,0.88)] px-4 py-20 sm:px-6 lg:px-8"
>
  <div class="mx-auto max-w-5xl text-center">
    <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
      {copy.label}
    </p>
    <h2
      class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl"
    >
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
      <p class="faq-eyebrow text-center">
        {copy.faqLabel}
      </p>
      <div class="faq-list mx-auto mt-6 max-w-4xl">
        {#each faqs as faq}
          <details class="faq-item" open={faq.id === "how-does-it-work"}>
            <summary class="faq-summary">
              <span class="faq-question">{faq.question}</span>
              <span class="faq-toggle" aria-hidden="true">
                <svg
                  viewBox="0 0 20 20"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="1.8"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M5 8l5 5 5-5"
                  />
                </svg>
              </span>
            </summary>

            <div class="faq-answer">
              {#if faq.answer}
                <ol class="space-y-3 text-sm leading-7 text-stone-600">
                  {#each faq.answer as step, idx}
                    <li>
                      <span class="font-medium text-stone-900">{idx + 1}.</span>
                      {step}
                    </li>
                  {/each}
                </ol>
                <p class="mt-3 text-sm leading-7 text-stone-600">
                  {faq.closing}
                </p>
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

<style>
  .faq-eyebrow {
    color: #697168;
    font-size: 0.75rem;
    font-weight: 650;
    letter-spacing: 0.22em;
    text-transform: uppercase;
  }

  .faq-list {
    display: grid;
    gap: 0.7rem;
  }

  .faq-item {
    overflow: hidden;
    border: 1px solid rgba(255, 255, 255, 0.9);
    border-radius: 1.2rem;
    background: rgba(255, 255, 255, 0.7);
    box-shadow: 0 10px 28px rgba(45, 53, 46, 0.055);
    backdrop-filter: blur(12px);
    transition:
      background-color 180ms ease,
      border-color 180ms ease,
      box-shadow 180ms ease;
  }

  .faq-item[open] {
    border-color: rgba(190, 201, 185, 0.6);
    background: rgba(255, 255, 255, 0.9);
    box-shadow: 0 16px 36px rgba(45, 53, 46, 0.08);
  }

  .faq-summary {
    display: flex;
    min-height: 3.75rem;
    cursor: pointer;
    list-style: none;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    padding: 0.85rem 1rem 0.85rem 1.15rem;
  }

  .faq-summary::-webkit-details-marker {
    display: none;
  }

  .faq-summary:focus-visible {
    outline: 3px solid rgba(95, 109, 96, 0.25);
    outline-offset: -3px;
  }

  .faq-question {
    color: #303630;
    font-family: Georgia, "Times New Roman", serif;
    font-size: 1rem;
    font-weight: 600;
    line-height: 1.35;
    letter-spacing: -0.015em;
  }

  .faq-toggle {
    display: grid;
    width: 2rem;
    height: 2rem;
    flex: 0 0 auto;
    place-items: center;
    border-radius: 50%;
    background: #e7ede3;
    color: #526153;
  }

  .faq-toggle svg {
    width: 0.9rem;
    height: 0.9rem;
    transition: transform 220ms ease;
  }

  .faq-item[open] .faq-toggle svg {
    transform: rotate(180deg);
  }

  .faq-answer {
    margin-inline: 1.15rem;
    border-top: 1px solid rgba(120, 132, 115, 0.14);
    padding: 1rem 0 1.15rem;
  }

  @media (min-width: 640px) {
    .faq-list {
      gap: 0.85rem;
    }

    .faq-summary {
      min-height: 4.25rem;
      padding-inline: 1.4rem 1.15rem;
    }

    .faq-question {
      font-size: 1.1rem;
    }

    .faq-answer {
      margin-inline: 1.4rem;
      padding-bottom: 1.35rem;
    }
  }
</style>
