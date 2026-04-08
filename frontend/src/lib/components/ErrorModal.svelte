<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";
  import { fade, scale } from "svelte/transition";

  export let show = false;
  export let onClose: () => void;
  export let emailAddress = "myemail@gmail.com";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";
  language.subscribe((value) => {
    currentLang = value;
  });
  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];

  let copied = false;

  async function copyEmail() {
    try {
      await navigator.clipboard.writeText(emailAddress);
      copied = true;
      setTimeout(() => (copied = false), 2000); // Reset button text after 2s
    } catch (err) {
      console.error("Failed to copy", err);
    }
  }
</script>

{#if show}
  <div
    transition:fade={{ duration: 200 }}
    class="fixed inset-0 z-50 flex items-center justify-center bg-[rgba(34,39,34,0.28)] p-4 backdrop-blur-md"
    role="button"
    tabindex="0"
    on:click|self={onClose}
    on:keydown={(e) => e.key === "Escape" && onClose()}
  >
    <div
      transition:scale={{ duration: 300, start: 0.95 }}
      class="w-full max-w-sm rounded-[1.75rem] border border-black/6 bg-white/95 p-8 text-center shadow-[0_24px_60px_rgba(45,53,46,0.16)]"
    >
      <div
        class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-[color:var(--error-soft)]"
      >
        <svg
          class="h-10 w-10 text-[color:var(--error-strong)]"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      </div>

      <h2 class="mb-2 text-2xl font-semibold text-stone-900">
        {t.modals.uploadError.title}
      </h2>
      <p class="mb-6 text-lg leading-relaxed text-stone-600">
        {t.modals.uploadError.description}
      </p>

      <div
        class="mb-6 flex items-center gap-2 rounded-2xl border border-stone-200 bg-stone-50 p-2"
      >
        <code class="flex-1 break-all text-sm font-medium text-stone-700"
          >{emailAddress}</code
        >
        <button
          on:click={copyEmail}
          class="rounded-full border border-stone-200 bg-white px-3 py-1.5 text-xs font-medium text-stone-700 transition-colors hover:border-stone-300 hover:text-stone-900"
        >
          {copied ? "Copied!" : "Copy"}
        </button>
      </div>

      <button
        on:click={onClose}
        class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-4 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
      >
        {t.modals.uploadError.close}
      </button>
    </div>
  </div>
{/if}
