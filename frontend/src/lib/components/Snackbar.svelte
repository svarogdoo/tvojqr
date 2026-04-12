<script lang="ts">
  import { fly } from "svelte/transition";
  import { hideSnackbar, snackbar } from "$lib/stores/snackbar";

  $: toneClasses =
    $snackbar.tone === "success"
      ? "border-[rgba(77,106,83,0.18)] bg-[rgba(236,245,238,0.96)] text-[color:var(--success-strong)]"
      : $snackbar.tone === "error"
        ? "border-[rgba(165,93,79,0.16)] bg-[rgba(249,238,234,0.96)] text-[color:var(--error-strong)]"
        : "border-stone-200 bg-white/96 text-stone-700";
</script>

{#if $snackbar.visible}
  <div class="pointer-events-none fixed inset-x-0 bottom-6 z-[60] flex justify-center px-4">
    <div
      transition:fly={{ y: 16, duration: 180 }}
      class={`pointer-events-auto flex max-w-lg items-center gap-3 rounded-2xl border px-4 py-3 shadow-[0_18px_45px_rgba(45,53,46,0.14)] backdrop-blur-sm ${toneClasses}`}
      role="status"
      aria-live="polite"
    >
      <p class="text-sm font-medium leading-6">{$snackbar.message}</p>
      <button
        type="button"
        on:click={hideSnackbar}
        class="rounded-full px-2 py-1 text-xs font-semibold text-current/70 transition-colors hover:text-current"
      >
        Close
      </button>
    </div>
  </div>
{/if}
