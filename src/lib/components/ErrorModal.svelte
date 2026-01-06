<script lang="ts">
  import { fade, scale } from "svelte/transition";

  export let show = false;
  export let onClose: () => void;
  export let emailAddress = "myemail@gmail.com";

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
    class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/60 backdrop-blur-sm"
    role="button"
    tabindex="0"
    on:click|self={onClose}
    on:keydown={(e) => e.key === "Escape" && onClose()}
  >
    <div
      transition:scale={{ duration: 300, start: 0.95 }}
      class="bg-white rounded-2xl p-8 max-w-sm w-full shadow-2xl text-center"
    >
      <div
        class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100 mb-6"
      >
        <svg
          class="h-10 w-10 text-red-600"
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

      <h2 class="text-2xl font-bold text-gray-900 mb-2">Oops!</h2>
      <p class="text-gray-600 mb-6 leading-relaxed text-lg">
        Something unfortunately went wrong. You can write to us directly at:
      </p>

      <div
        class="flex items-center gap-2 p-2 bg-gray-50 border border-gray-200 rounded-lg mb-6"
      >
        <code class="flex-1 text-sm font-semibold text-gray-800 break-all"
          >{emailAddress}</code
        >
        <button
          on:click={copyEmail}
          class="px-3 py-1 bg-white border border-gray-300 rounded md text-xs font-medium hover:bg-gray-50 transition-colors"
        >
          {copied ? "Copied!" : "Copy"}
        </button>
      </div>

      <button
        on:click={onClose}
        class="w-full py-3 px-4 bg-gray-900 text-white font-semibold rounded-xl hover:bg-black transition-colors"
      >
        Close
      </button>
    </div>
  </div>
{/if}
