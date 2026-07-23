<script lang="ts">
  import { browser } from "$app/environment";
  import { afterNavigate } from "$app/navigation";
  import { gaMeasurementId } from "$lib/config";
  import { onMount } from "svelte";

  const consentKey = "hostingqr-analytics-consent";
  const hasValidMeasurementId = /^G-[A-Z0-9]+$/i.test(gaMeasurementId);
  const trackablePaths = new Set(["/", "/pricing", "/contact", "/terms", "/privacy"]);
  const allowedCampaignParameters = new Set([
    "utm_source",
    "utm_medium",
    "utm_campaign",
    "utm_term",
    "utm_content",
    "utm_id",
  ]);

  let showChoices = false;
  let analyticsLoaded = false;
  let scriptInstalled = false;
  let lastTrackedUrl = "";
  let consentChoice: string | null = null;

  function gtag(...args: unknown[]) {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push(args);
  }

  function trackPageView() {
    if (!analyticsLoaded || !trackablePaths.has(window.location.pathname)) {
      return;
    }

    const analyticsUrl = new URL(window.location.origin + window.location.pathname);
    for (const [key, value] of new URLSearchParams(window.location.search)) {
      if (allowedCampaignParameters.has(key)) {
        analyticsUrl.searchParams.set(key, value);
      }
    }

    if (lastTrackedUrl === analyticsUrl.href) {
      return;
    }

    lastTrackedUrl = analyticsUrl.href;
    gtag("event", "page_view", {
      page_location: analyticsUrl.href,
      page_path: `${analyticsUrl.pathname}${analyticsUrl.search}`,
      page_title: document.title,
    });
  }

  function enableAnalytics() {
    if (
      !hasValidMeasurementId ||
      analyticsLoaded ||
      !trackablePaths.has(window.location.pathname)
    ) {
      return;
    }

    (window as unknown as Record<string, boolean>)[`ga-disable-${gaMeasurementId}`] = false;

    if (!scriptInstalled) {
      const script = document.createElement("script");
      script.async = true;
      script.src = `https://www.googletagmanager.com/gtag/js?id=${encodeURIComponent(gaMeasurementId)}`;
      document.head.appendChild(script);

      gtag("js", new Date());
      gtag("config", gaMeasurementId, {
        anonymize_ip: true,
        send_page_view: false,
      });
      scriptInstalled = true;
    }

    analyticsLoaded = true;
    trackPageView();
  }

  function suspendAnalytics() {
    analyticsLoaded = false;
    lastTrackedUrl = "";
    (window as unknown as Record<string, boolean>)[`ga-disable-${gaMeasurementId}`] = true;
  }

  function disableAnalytics() {
    suspendAnalytics();

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
  }

  function saveChoice(choice: "accepted" | "rejected") {
    localStorage.setItem(consentKey, choice);
    consentChoice = choice;
    showChoices = false;

    if (choice === "accepted") {
      enableAnalytics();
    } else {
      disableAnalytics();
    }
  }

  afterNavigate(() => {
    if (!browser || !hasValidMeasurementId) {
      return;
    }

    if (!trackablePaths.has(window.location.pathname)) {
      showChoices = false;
      suspendAnalytics();
      return;
    }

    if (consentChoice === null) {
      showChoices = true;
    } else if (consentChoice === "accepted") {
      enableAnalytics();
      trackPageView();
    }
  });

  onMount(() => {
    if (!hasValidMeasurementId) {
      return;
    }

    consentChoice = localStorage.getItem(consentKey);
    showChoices =
      trackablePaths.has(window.location.pathname) &&
      consentChoice !== "accepted" &&
      consentChoice !== "rejected";

    if (consentChoice === "accepted") {
      enableAnalytics();
    }

    const openChoices = () => {
      showChoices = true;
    };
    window.addEventListener("hostingqr:open-analytics-consent", openChoices);

    return () => {
      window.removeEventListener("hostingqr:open-analytics-consent", openChoices);
    };
  });
</script>

{#if showChoices}
  <aside
    class="fixed inset-x-4 bottom-4 z-50 mx-auto max-w-xl rounded-3xl border border-stone-200 bg-white/95 p-5 shadow-[0_20px_60px_rgba(45,53,46,0.2)] backdrop-blur-lg sm:p-6"
    aria-label="Analytics preferences"
  >
    <h2 class="text-lg font-semibold text-stone-900">Optional analytics</h2>
    <p class="mt-2 text-sm leading-6 text-stone-600">
      We use Google Analytics only with your permission to understand visits and traffic sources. You can change this choice later in Cookie settings.
    </p>
    <div class="mt-4 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
      <button
        type="button"
        class="rounded-full border border-stone-300 px-5 py-2.5 text-sm font-medium text-stone-700 transition-colors hover:bg-stone-100"
        on:click={() => saveChoice("rejected")}
      >
        Reject optional analytics
      </button>
      <button
        type="button"
        class="rounded-full bg-stone-900 px-5 py-2.5 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        on:click={() => saveChoice("accepted")}
      >
        Accept analytics
      </button>
    </div>
  </aside>
{/if}
