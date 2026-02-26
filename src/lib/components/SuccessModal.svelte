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
    class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/60 backdrop-blur-sm"
    role="button"
    tabindex="0"
    aria-label="Close modal"
    on:click|self={handleBackdropClick}
    on:keydown={(e) => e.key === "Escape" && onClose()}
  >
    <div
      transition:scale={{ duration: 300, start: 0.95 }}
      class="bg-white rounded-2xl p-8 max-w-sm w-full shadow-2xl text-center"
    >
      <div
        class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-olive-100 mb-6"
      >
        <svg
          class="h-10 w-10 text-olive-700"
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

      <h2 class="text-2xl font-bold text-gray-900 mb-2">
        {t.modals.uploadSuccess.title}
      </h2>
      <p class="text-gray-600 mb-6 leading-relaxed text-lg">
        {t.modals.uploadSuccess.description}
      </p>

      <button
        on:click={onClose}
        class="w-full py-3 px-4 bg-olive-700 text-white font-semibold rounded-xl hover:bg-olive-800 transition-colors shadow-lg shadow-olive-200/60"
      >
        {t.modals.uploadSuccess.close}
      </button>
    </div>
  </div>
{/if}
