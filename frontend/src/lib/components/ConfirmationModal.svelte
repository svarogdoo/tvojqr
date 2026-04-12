<script lang="ts">
  import { fade, scale } from "svelte/transition";

  export let show = false;
  export let title = "Confirm action";
  export let description = "Are you sure you want to continue?";
  export let confirmLabel = "Confirm";
  export let cancelLabel = "Cancel";
  export let loading = false;
  export let destructive = false;
  export let onConfirm: () => void | Promise<void>;
  export let onClose: () => void;

  $: confirmClasses = destructive
    ? "bg-[color:var(--error-strong)] text-white hover:opacity-95"
    : "bg-stone-900 text-white hover:bg-stone-800";
</script>

{#if show}
  <div
    transition:fade={{ duration: 200 }}
    class="fixed inset-0 z-50 flex items-center justify-center bg-[rgba(34,39,34,0.28)] p-4 backdrop-blur-md"
    role="button"
    tabindex="0"
    aria-label="Close confirmation modal"
    on:click|self={onClose}
    on:keydown={(e) => e.key === "Escape" && onClose()}
  >
    <div
      transition:scale={{ duration: 300, start: 0.95 }}
      class="w-full max-w-md rounded-[1.75rem] border border-black/6 bg-white/95 p-8 shadow-[0_24px_60px_rgba(45,53,46,0.16)]"
    >
      <div class="mb-6">
        <h2 class="text-2xl font-semibold text-stone-900">{title}</h2>
        <p class="mt-3 text-base leading-7 text-stone-600">{description}</p>
      </div>

      <div class="flex flex-wrap justify-end gap-3">
        <button
          type="button"
          on:click={onClose}
          class="inline-flex items-center rounded-full border border-stone-200 bg-white px-4 py-2.5 text-sm font-medium text-stone-700 transition-colors hover:border-stone-300 hover:text-stone-900"
          disabled={loading}
        >
          {cancelLabel}
        </button>
        <button
          type="button"
          on:click={onConfirm}
          class={`inline-flex items-center rounded-full px-4 py-2.5 text-sm font-medium transition-colors ${confirmClasses}`}
          disabled={loading}
        >
          {loading ? "Working..." : confirmLabel}
        </button>
      </div>
    </div>
  </div>
{/if}
