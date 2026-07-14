<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import { language, type LanguageCode } from "$lib/stores/language";
  import { homepageCopy } from "$lib/homepageCopy";
  import { page } from "$app/stores";

  export let form: {
    success?: boolean;
    message?: string;
  } | null;

  let needsLanguages = false;
  let uploadFileInput: HTMLInputElement | null = null;
  let uploadLabel = "";

  let currentLang: LanguageCode = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: copy = homepageCopy[currentLang].contactPage;
  $: selectedPlan = $page.url.searchParams.get("plan") ?? "";
  $: selectedBillingCycle = $page.url.searchParams.get("billingCycle") ?? "";
  $: isPaidPlanRequest = selectedPlan === "standard" || selectedPlan === "plus";
  $: selectedPlanLabel = selectedPlan
    ? selectedPlan.charAt(0).toUpperCase() + selectedPlan.slice(1)
    : "";

  function handleFileChange(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    uploadLabel =
      files.length === 0
        ? ""
        : files.length === 1
          ? files[0].name
          : currentLang === "es"
            ? `${files.length} archivos seleccionados`
            : `${files.length} files selected`;
  }
</script>

<svelte:head>
  <title>{copy.title} - HostingQr</title>
  <meta
    name="description"
    content={currentLang === "es"
      ? "Cuéntanos qué necesitas y te responderemos por email pronto."
      : "Tell HostingQr what you need and we will email you back shortly."}
  />
</svelte:head>

<div class="flex min-h-screen flex-col bg-[rgba(243,244,246,0.98)]">
<Navigation />

<main class="flex-1 px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <section class="mx-auto max-w-6xl">
    <div class="mx-auto max-w-3xl rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_18px_50px_rgba(45,53,46,0.08)] sm:p-10">
      <div class="text-center">
        <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
          {copy.label}
        </p>
        <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
          {copy.title}
        </h1>
        <p class="mx-auto mt-5 max-w-2xl text-base leading-7 text-stone-600">
          {copy.subtitle}
        </p>
      </div>

      <div class="mt-8">
        {#if form?.success}
          <div class="rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm leading-7 text-emerald-900">
            {copy.success}
          </div>
        {:else if form?.message}
          <div class="rounded-2xl border border-rose-200 bg-rose-50 px-5 py-4 text-sm leading-7 text-rose-900">
            {form.message}
          </div>
        {/if}

        {#if isPaidPlanRequest}
          <div class="mt-8 rounded-2xl border border-amber-200 bg-amber-50 px-5 py-4 text-sm leading-7 text-amber-950">
            <span class="font-semibold">Payments are almost ready.</span>
            We are working on automating payment and it will be ready very soon.
            For now, fill out this form and we will help you activate the
            {selectedPlanLabel} plan manually.
          </div>
        {/if}

        <form method="POST" enctype="multipart/form-data" class="mt-8 space-y-5">
          <input type="hidden" name="plan" value={selectedPlan} />
          <input type="hidden" name="billingCycle" value={selectedBillingCycle} />

          <div class="grid gap-5 sm:grid-cols-2">
            <div>
              <label for="name" class="mb-2 block text-sm font-medium text-stone-700">{copy.fields.name}</label>
              <input
                id="name"
                name="name"
                type="text"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
                placeholder={copy.placeholders.name}
              />
            </div>

            <div>
              <label for="email" class="mb-2 block text-sm font-medium text-stone-700">{copy.fields.email}</label>
              <input
                id="email"
                name="email"
                type="email"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
                placeholder={copy.placeholders.email}
              />
            </div>
          </div>

          <div>
            <div class="mb-2 flex items-center justify-between gap-3 text-sm font-medium text-stone-700">
              <label for="files">{copy.fields.files}</label>
              <span class="text-stone-500">{copy.helper.files}</span>
            </div>
            <input
              bind:this={uploadFileInput}
              id="files"
              name="files"
              type="file"
              multiple
              accept="image/*,.pdf"
              class="sr-only"
              on:change={handleFileChange}
            />
            <div class="flex items-center gap-3 rounded-2xl border border-dashed border-stone-300 bg-stone-50/60 p-4">
              <button
                type="button"
                on:click={() => uploadFileInput?.click()}
                class="inline-flex items-center rounded-full bg-stone-900 px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-stone-800"
              >
                {copy.helper.browse}
              </button>
              <span class="text-sm text-stone-500">{uploadLabel || copy.placeholders.files}</span>
            </div>
          </div>

          <div class="rounded-[1.5rem] border border-stone-200 bg-stone-50/70 p-5">
            <label class="flex items-center gap-3 text-sm font-medium text-stone-800">
              <input
                bind:checked={needsLanguages}
                name="needsLanguages"
                type="checkbox"
                class="h-4 w-4 rounded border-stone-300 text-stone-900 accent-stone-900"
              />
              {copy.helper.languages}
            </label>

            {#if needsLanguages}
              <div class="mt-4">
                <label for="languages" class="mb-2 block text-sm font-medium text-stone-700">
                  {copy.fields.languages}
                </label>
                <input
                  id="languages"
                  name="languages"
                  type="text"
                  required={needsLanguages}
                  placeholder={copy.placeholders.languages}
                  class="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400"
                />
              </div>
            {/if}
          </div>

          <div>
            <label for="message" class="mb-2 block text-sm font-medium text-stone-700">{copy.fields.message}</label>
            <textarea
              id="message"
              name="message"
              rows="6"
              required
              class="w-full resize-none rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
              placeholder={copy.placeholders.message}
            ></textarea>
          </div>

          <button
            type="submit"
            class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
          >
            {copy.fields.submit}
          </button>
        </form>
      </div>
    </div>
  </section>
</main>

<Footer />
</div>
