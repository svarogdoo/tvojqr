<script lang="ts">
  import favicon from "$lib/assets/hostinqr-logo.svg";
  import Snackbar from "$lib/components/Snackbar.svelte";
  import { refreshSession } from "$lib/stores/auth";
  import { onMount } from "svelte";
  import "../app.css";

  let { children } = $props();

  onMount(() => {
    void refreshSession();
  });
</script>

<svelte:head>
  <link rel="icon" href={favicon} />
</svelte:head>

{@render children()}
<Snackbar />
