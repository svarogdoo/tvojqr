<script lang="ts">
  import { onMount } from "svelte";
  import { apiFetch } from "$lib/api";
  import { toApiUrl } from "$lib/config";

  type PublicAsset = {
    id: string;
    originalFileName: string;
    url: string;
  };

  type PublicProject = {
    projectId: string;
    name: string;
    slug: string;
    ownerDisplayName: string;
    status: "active" | "disabled";
    backgroundColor: string;
    assets: PublicAsset[];
  };

  type PageState = "loading" | "active" | "disabled" | "missing" | "error";

  let slug = "";
  let state: PageState = "loading";
  let project: PublicProject | null = null;

  onMount(async () => {
    slug = window.location.pathname.replace(/^\//, "");

    try {
      const response = await apiFetch(`/api/public/${encodeURIComponent(slug)}`);

      if (response.status === 404) {
        state = "missing";
        return;
      }

      if (response.status === 410) {
        project = (await response.json()) as PublicProject;
        state = "disabled";
        return;
      }

      if (!response.ok) {
        state = "error";
        return;
      }

      project = (await response.json()) as PublicProject;
      state = "active";
    } catch {
      state = "error";
    }
  });
</script>

<svelte:head>
  {#if state === "active" && project}
    <title>{project.name || "Hosted page"} - HostingQr</title>
    <meta name="description" content={`View the hosted page for ${project.name || project.slug}.`} />
  {:else if state === "disabled" && project}
    <title>{project.name || "Hosted page"} unavailable - HostingQr</title>
  {:else}
    <title>HostingQr</title>
  {/if}
</svelte:head>

<div class="min-h-screen px-4 py-10 sm:px-6 lg:px-8" style={`background-color: ${project?.backgroundColor ?? "#f8f7f3"};`}>
  {#if state === "loading"}
    <div class="mx-auto max-w-3xl rounded-[2rem] border border-black/6 bg-white/86 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)]">
      <p class="text-lg text-stone-600">Loading page...</p>
    </div>
  {:else if state === "active" && project}
    <main class="mx-auto max-w-5xl">
      <section class="space-y-4">
        {#if project.assets.length === 0}
          <div class="rounded-[2rem] border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)]">
            <p class="text-base text-stone-600">No images have been published on this page yet.</p>
          </div>
        {:else}
          {#each project.assets as asset}
            <div class="overflow-hidden rounded-md">
              <img
                src={toApiUrl(asset.url)}
                alt={asset.originalFileName}
                class="block h-auto w-full"
              />
            </div>
          {/each}
        {/if}
      </section>
    </main>
  {:else if state === "disabled"}
    <div class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center">
      <div class="w-full rounded-[2rem] border border-black/6 bg-[rgba(239,236,230,0.96)] p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14">
        <div class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-900 text-sm font-semibold text-white">
          Paused
        </div>
        <h2 class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl">
          This page has stepped away from the table
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          The owner has disabled this project for now, so its public page is no longer available. If you want your own clean hosted QR page, you can start fresh with HostingQr.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Explore HostingQr
        </a>
      </div>
    </div>
  {:else if state === "missing"}
    <div class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center">
      <div class="w-full rounded-[2rem] border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14">
        <div class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-100 text-sm font-semibold text-stone-600">
          404
        </div>
        <h2 class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl">
          This page never made it to the menu
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          We couldn’t find a hosted page for this URL. If you want a simple project page with your own images and QR code, HostingQr can set you up in minutes.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Visit HostingQr
        </a>
      </div>
    </div>
  {:else}
    <div class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center">
      <div class="w-full rounded-[2rem] border border-black/6 bg-white/90 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] sm:p-14">
        <h2 class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl">
          This page is taking a short pause
        </h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          Something went wrong while loading this hosted page. You can head back to HostingQr and try again in a moment.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          Back to HostingQr
        </a>
      </div>
    </div>
  {/if}
</div>
