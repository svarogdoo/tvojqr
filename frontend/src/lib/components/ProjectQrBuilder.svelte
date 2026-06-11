<script lang="ts">
  import { onMount } from "svelte";
  import { showSnackbar } from "$lib/stores/snackbar";

  export let slug = "";
  export let projectName = "";

  type PresetKey = "clean" | "rounded" | "bold";

  type PresetConfig = {
    dotsType: "square" | "rounded" | "extra-rounded" | "classy" | "classy-rounded" | "dots";
    cornersSquareType: "square" | "extra-rounded" | "dot";
    cornersDotType: "square" | "dot";
    label: string;
  };

  const presets: Record<PresetKey, PresetConfig> = {
    clean: {
      dotsType: "square",
      cornersSquareType: "square",
      cornersDotType: "square",
      label: "Clean",
    },
    rounded: {
      dotsType: "rounded",
      cornersSquareType: "extra-rounded",
      cornersDotType: "dot",
      label: "Rounded",
    },
    bold: {
      dotsType: "classy-rounded",
      cornersSquareType: "dot",
      cornersDotType: "dot",
      label: "Bold",
    },
  };

  const presetEntries: Array<{ key: PresetKey; preset: PresetConfig }> = [
    { key: "clean" as PresetKey, preset: presets.clean },
    { key: "rounded" as PresetKey, preset: presets.rounded },
    { key: "bold" as PresetKey, preset: presets.bold },
  ];

  let qrMount: HTMLDivElement;
  let QRCodeStylingCtor: any = null;
  let qrCode: {
    append: (element: HTMLElement) => void;
    update: (options: Record<string, unknown>) => void;
    download: (options: { name: string; extension: "png" | "svg" }) => void;
  } | null = null;

  let selectedPreset: PresetKey = "clean";
  let dotColor = "#2f362f";
  let backgroundColor = "#ffffff";

  function buildTargetUrl() {
    if (typeof window === "undefined") {
      return "";
    }

    return slug.trim() ? `${window.location.origin}/${slug.trim()}` : "";
  }

  function buildOptions() {
    const preset = presets[selectedPreset];

    return {
      data: buildTargetUrl() || "https://hostingqr.com",
      width: 220,
      height: 220,
      margin: 8,
      qrOptions: {
        errorCorrectionLevel: "Q",
      },
      dotsOptions: {
        color: dotColor,
        type: preset.dotsType,
      },
      backgroundOptions: {
        color: backgroundColor,
      },
      cornersSquareOptions: {
        type: preset.cornersSquareType,
        color: dotColor,
      },
      cornersDotOptions: {
        type: preset.cornersDotType,
        color: dotColor,
      },
    };
  }

  async function downloadQr(extension: "png" | "svg") {
    if (!qrCode || !slug.trim()) {
      showSnackbar("Add a slug first so the QR code has a destination.", "error");
      return;
    }

    qrCode.download({
      name: `${(projectName || "hostingqr-project").trim().toLowerCase().replace(/\s+/g, "-") || "hostingqr-project"}-qr`,
      extension,
    });
  }

  async function copyTargetUrl() {
    const targetUrl = buildTargetUrl();
    if (!targetUrl) {
      showSnackbar("Add a slug first so there is a URL to copy.", "error");
      return;
    }

    try {
      await navigator.clipboard.writeText(targetUrl);
      showSnackbar("Public URL copied.", "success");
    } catch {
      showSnackbar("Unable to copy public URL.", "error");
    }
  }

  function renderQr() {
    if (!QRCodeStylingCtor || !qrMount) {
      return;
    }

    qrMount.innerHTML = "";
    const instance = new QRCodeStylingCtor(buildOptions());
    instance.append(qrMount);
    qrCode = instance;
  }

  onMount(async () => {
    const module = await import("qr-code-styling");
    QRCodeStylingCtor = module.default as any;
    renderQr();
  });

  $: if (QRCodeStylingCtor && qrMount) {
    selectedPreset;
    dotColor;
    backgroundColor;
    slug;
    renderQr();
  }
</script>

<div class="rounded-[1.5rem] border border-stone-200 bg-[rgba(248,247,243,0.96)] px-5 py-5 shadow-sm sm:px-6">
  <div class="grid gap-5 xl:grid-cols-[0.85fr_1.15fr] xl:items-start">
    <div class="rounded-[1.5rem] border border-stone-200 bg-white px-5 py-5 shadow-sm">
      <p class="text-xs uppercase tracking-[0.18em] text-stone-500">QR builder</p>
      <p class="mt-2 text-sm leading-7 text-stone-600">
        Preview how your QR code will look, choose a style and colors, then download the version you want to print or display.
      </p>
      <div class="mt-5 grid gap-4">
        <div>
        <p class="mb-2 text-sm font-medium text-stone-700">Style</p>
        <div class="flex flex-wrap gap-2">
          {#each presetEntries as entry}
            <button
              type="button"
              class={`rounded-full border px-4 py-2 text-sm font-medium transition-colors ${selectedPreset === entry.key
                ? "border-stone-900 bg-stone-900 text-white"
                : "border-stone-200 bg-white text-stone-700 hover:border-stone-300 hover:text-stone-900"}`}
              on:click={() => (selectedPreset = entry.key)}
            >
              {entry.preset.label}
            </button>
          {/each}
        </div>
        </div>

        <div class="grid gap-4 sm:grid-cols-2">
        <label class="block">
          <span class="mb-2 block text-sm font-medium text-stone-700">QR color</span>
          <div class="flex items-center gap-3 rounded-2xl border border-stone-200 bg-white px-4 py-3">
            <input type="color" bind:value={dotColor} class="h-9 w-12 rounded border-0 bg-transparent p-0" />
            <span class="text-sm text-stone-600">{dotColor}</span>
          </div>
        </label>

        <label class="block">
          <span class="mb-2 block text-sm font-medium text-stone-700">Background</span>
          <div class="flex items-center gap-3 rounded-2xl border border-stone-200 bg-white px-4 py-3">
            <input type="color" bind:value={backgroundColor} class="h-9 w-12 rounded border-0 bg-transparent p-0" />
            <span class="text-sm text-stone-600">{backgroundColor}</span>
          </div>
        </label>
        </div>
      </div>
    </div>

    <div class="grid gap-4">
      <div class="rounded-[1.5rem] border border-stone-200 bg-white p-4 shadow-sm">
        <div bind:this={qrMount} class="flex min-h-[200px] items-center justify-center sm:min-h-[220px]"></div>
      </div>

      <div class="rounded-[1.5rem] border border-stone-200 bg-white px-5 py-5 shadow-sm">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div class="min-w-0">
          <p class="text-sm font-medium text-stone-700">Target</p>
          <p class="mt-2 break-all text-sm leading-7 text-stone-600">
            {buildTargetUrl() || "Set a slug to generate a project QR code."}
          </p>
        </div>
        <button
          type="button"
          class="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-stone-200 bg-white text-stone-500 shadow-sm transition-colors hover:border-stone-300 hover:text-stone-900"
          on:click={copyTargetUrl}
          aria-label="Copy public URL"
          title="Copy public URL"
        >
          <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="9" y="9" width="11" height="11" rx="2" />
            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
          </svg>
        </button>
      </div>

      <div class="mt-5 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
        <button type="button" class="btn-primary w-full text-sm sm:w-auto" on:click={() => downloadQr("png")}>
          Download PNG
        </button>
        <button type="button" class="btn-secondary w-full text-sm sm:w-auto" on:click={() => downloadQr("svg")}>
          Download SVG
        </button>
      </div>
      </div>
    </div>
  </div>
</div>
