<script lang="ts">
  import Snackbar from "$lib/components/Snackbar.svelte";
  import favicon from "$lib/assets/favicon.svg";
  import { refreshSession } from "$lib/stores/auth";
  import "../app.css";
  import { onMount } from "svelte";

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
