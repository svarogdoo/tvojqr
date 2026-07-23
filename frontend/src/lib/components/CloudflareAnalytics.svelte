<script lang="ts">
  import { beforeNavigate } from "$app/navigation";
  import { page } from "$app/stores";
  import { cloudflareWebAnalyticsToken } from "$lib/config";
  import { onMount } from "svelte";

  const hasValidToken = /^[a-z0-9]{32}$/i.test(cloudflareWebAnalyticsToken);
  const publicPaths = new Set(["/", "/pricing", "/contact", "/terms", "/privacy"]);
  const beaconConfig = JSON.stringify({ token: cloudflareWebAnalyticsToken, spa: false });

  $: shouldMeasurePage = hasValidToken && publicPaths.has($page.url.pathname);

  beforeNavigate(({ cancel, to }) => {
    if (
      !shouldMeasurePage ||
      !to ||
      to.url.origin !== window.location.origin ||
      publicPaths.has(to.url.pathname)
    ) {
      return;
    }

    cancel();
    window.location.assign(to.url.href);
  });

  onMount(() => {
    localStorage.removeItem("hostingqr-analytics-consent");

    for (const cookie of document.cookie.split(";")) {
      const name = cookie.split("=")[0]?.trim();
      if (name === "_ga" || name?.startsWith("_ga_")) {
        const expiredCookie = `${name}=; Max-Age=0; path=/; SameSite=Lax`;
        const parentDomain = window.location.hostname.split(".").slice(-2).join(".");
        document.cookie = expiredCookie;
        document.cookie = `${expiredCookie}; domain=${window.location.hostname}`;
        document.cookie = `${expiredCookie}; domain=.${parentDomain}`;
      }
    }
  });
</script>

<svelte:head>
  {#if shouldMeasurePage}
    <script
      type="module"
      src="https://static.cloudflareinsights.com/beacon.min.js"
      data-cf-beacon={beaconConfig}
    ></script>
  {/if}
</svelte:head>
