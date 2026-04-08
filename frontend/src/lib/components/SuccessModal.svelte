<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";
  import { fade, scale } from "svelte/transition";

  export let show = false;
  export let onClose: () => void;

  let currentLang: "en" | "sr" | "ru" | "el" = "en";
  language.subscribe((value) => {
    currentLang = value;
  });
  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];

  function handleBackdropClick() {
    onClose();
  }
</script>

{#if show}
  <div
    transition:fade={{ duration: 200 }}
    class="fixed inset-0 z-50 flex items-center justify-center bg-[rgba(34,39,34,0.28)] p-4 backdrop-blur-md"
    role="button"
    tabindex="0"
    aria-label="Close modal"
    on:click|self={handleBackdropClick}
    on:keydown={(e) => e.key === "Escape" && onClose()}
  >
    <div
      transition:scale={{ duration: 300, start: 0.95 }}
      class="w-full max-w-sm rounded-[1.75rem] border border-black/6 bg-white/95 p-8 text-center shadow-[0_24px_60px_rgba(45,53,46,0.16)]"
    >
      <div
        class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-[rgba(236,245,238,0.95)]"
      >
        <svg
          class="h-10 w-10 text-[color:var(--success-strong)]"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M5 13l4 4L19 7"
          />
        </svg>
      </div>

      <h2 class="mb-2 text-2xl font-semibold text-stone-900">
        {t.modals.uploadSuccess.title}
      </h2>
      <p class="mb-6 text-lg leading-relaxed text-stone-600">
        {t.modals.uploadSuccess.description}
      </p>

      <button
        on:click={onClose}
        class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-4 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
      >
        {t.modals.uploadSuccess.close}
      </button>
    </div>
  </div>
{/if}
