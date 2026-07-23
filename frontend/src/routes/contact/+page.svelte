<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Seo from "$lib/components/Seo.svelte";
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
  const phoneHref = "tel:+35799180703";
  const whatsAppUrl = "https://wa.me/35799180703";
  const contactMetaDescriptions: Record<LanguageCode, string> = {
    en: "Tell HostingQr what you need and we will email you back shortly.",
    it: "Racconta a HostingQr cosa ti serve e ti risponderemo presto via email.",
    es: "Cuéntanos qué necesitas y te responderemos por email pronto.",
    hr: "Recite HostingQr-u što trebate i uskoro ćemo vam odgovoriti e-poštom.",
  };
  const selectedFilesLabels: Record<LanguageCode, string> = {
    en: "files selected",
    it: "file selezionati",
    es: "archivos seleccionados",
    hr: "odabranih datoteka",
  };
  const callLabels: Record<LanguageCode, string> = {
    en: "Call",
    it: "Chiama",
    es: "Llamar al",
    hr: "Nazovi",
  };

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
          : `${files.length} ${selectedFilesLabels[currentLang]}`;
  }
</script>

<Seo
  title={`${copy.title} - HostingQr`}
  description={contactMetaDescriptions[currentLang]}
  path="/contact"
/>

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

        <div class="mt-8 rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-800 shadow-sm sm:rounded-full">
          <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p class="text-center text-sm text-stone-600 sm:text-left">
              <span class="font-semibold text-[rgba(58,116,78,0.95)]">{copy.directContact.title}</span>
            </p>

            <div class="grid grid-cols-2 gap-2 sm:flex sm:flex-wrap sm:justify-end">
              <a
                href={whatsAppUrl}
                target="_blank"
                rel="noreferrer"
                class="inline-flex min-h-10 items-center justify-center gap-2 rounded-full bg-stone-900 px-4 py-2 text-xs font-medium text-white transition-all hover:-translate-y-0.5 hover:bg-stone-800 sm:text-sm"
              >
                <svg
                  class="h-4 w-4 text-[rgba(178,219,191,0.95)]"
                  viewBox="0 0 24 24"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    d="M12.04 2C6.58 2 2.13 6.36 2.13 11.72c0 1.71.46 3.38 1.33 4.85L2 22l5.57-1.43a10.1 10.1 0 0 0 4.47.99c5.46 0 9.91-4.36 9.91-9.72S17.5 2 12.04 2Zm0 17.9c-1.48 0-2.93-.39-4.2-1.14l-.3-.18-3.3.85.88-3.18-.2-.32a8.03 8.03 0 0 1-1.23-4.21c0-4.44 3.75-8.06 8.35-8.06s8.35 3.62 8.35 8.06-3.75 8.18-8.35 8.18Zm4.58-6.03c-.25-.12-1.48-.72-1.71-.8-.23-.09-.4-.12-.57.12-.17.25-.65.8-.8.97-.15.16-.3.18-.55.06-.25-.12-1.06-.38-2.02-1.21-.75-.65-1.25-1.45-1.4-1.7-.15-.25-.02-.38.11-.5.12-.11.25-.29.38-.43.13-.15.17-.25.25-.42.08-.16.04-.31-.02-.43-.06-.12-.57-1.35-.78-1.85-.21-.5-.42-.42-.57-.43h-.49c-.17 0-.44.06-.67.31-.23.25-.88.85-.88 2.07 0 1.22.9 2.4 1.03 2.56.13.16 1.78 2.67 4.31 3.74.6.25 1.07.4 1.44.52.6.19 1.15.16 1.58.1.48-.07 1.48-.6 1.69-1.18.21-.58.21-1.08.15-1.18-.06-.1-.23-.16-.48-.28Z"
                  />
                </svg>
                {copy.directContact.whatsapp}
              </a>
              <a
                href={phoneHref}
                class="inline-flex min-h-10 flex-col items-center justify-center rounded-full border border-stone-300 bg-white px-4 py-2 text-xs font-medium leading-tight text-stone-800 transition-all hover:border-stone-400 hover:bg-stone-50 sm:flex-row sm:gap-1 sm:text-sm sm:leading-normal"
              >
                <span>{callLabels[currentLang]}</span>
                <span>+357 99 180703</span>
              </a>
            </div>
          </div>
        </div>

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
