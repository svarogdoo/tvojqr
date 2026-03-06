<script lang="ts">
  import { enhance } from "$app/forms";
  import ErrorModal from "$lib/components/ErrorModal.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import SuccessModal from "$lib/components/SuccessModal.svelte";
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations.js";

  export let form;

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];

  let loading = false;
  let domainChoice = "";

  let showSuccess = false;
  let showError = false;

  $: if (form) {
    if (form.success) {
      showSuccess = true;
      showError = false;
    } else if (form.success === false) {
      showError = true;
      showSuccess = false;
    }
  }

  function closeModals() {
    showSuccess = false;
    showError = false;
  }

  let allFiles: File[] = [];
  let previewFiles: {
    id: string;
    name: string;
    url: string;
    isImage: boolean;
  }[] = [];

  function handleFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const newFiles = Array.from(input.files);

    allFiles = [...allFiles, ...newFiles];

    const newPreviews = newFiles.map((file) => {
      const isImage = file.type.startsWith("image/");
      return {
        id: Math.random().toString(36).substr(2, 9),
        name: file.name,
        url: isImage ? URL.createObjectURL(file) : "",
        isImage,
      };
    });

    previewFiles = [...previewFiles, ...newPreviews];

    input.value = "";
  }

  function removeFile(index: number) {
    if (previewFiles[index].url) {
      URL.revokeObjectURL(previewFiles[index].url);
    }

    allFiles = allFiles.filter((_, i) => i !== index);
    previewFiles = previewFiles.filter((_, i) => i !== index);
  }
</script>

<Navigation />

<div class="min-h-screen px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <div class="mx-auto max-w-6xl">
    <div class="grid gap-8 lg:grid-cols-[0.85fr_1.15fr] lg:items-start">
      <section class="rounded-[2rem] border border-black/6 bg-[rgba(218,226,212,0.9)] p-8 shadow-[0_20px_50px_rgba(45,53,46,0.06)] backdrop-blur-sm sm:p-10">
        <p class="mb-4 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">
          Upload flow
        </p>
        <h1 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
          {t.createNew.title}
        </h1>
        <p class="mt-5 max-w-md text-base leading-7 text-stone-600">
          Start with the basics: your contact details, the public slug you want, and the files that should appear behind the QR code.
        </p>

        <div class="mt-10 grid gap-4">
          <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
            <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Step 1</p>
            <p class="mt-2 font-medium text-stone-900">Add your details</p>
          </div>
          <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
            <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Step 2</p>
            <p class="mt-2 font-medium text-stone-900">Choose your public slug</p>
          </div>
          <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
            <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Step 3</p>
            <p class="mt-2 font-medium text-stone-900">Upload images or PDFs</p>
          </div>
        </div>
      </section>

      <section class="rounded-[2rem] border border-black/6 bg-white/82 p-8 shadow-[0_20px_50px_rgba(45,53,46,0.07)] backdrop-blur-sm sm:p-10">
        <form
          method="POST"
          enctype="multipart/form-data"
          class="space-y-8"
          use:enhance={({ formData }) => {
            loading = true;

            formData.delete("attachments");

            allFiles.forEach((file) => {
              formData.append("attachments", file);
            });

            return async ({ update }) => {
              await update();
              loading = false;
              if (form?.success) {
                allFiles = [];
                previewFiles = [];
              }
            };
          }}
        >
          <div class="grid grid-cols-1 gap-5 sm:grid-cols-2">
            <div>
              <label for="firstName" class="mb-2 block text-sm font-medium text-stone-700"
                >{t.createNew.firstName}</label
              >
              <input
                type="text"
                name="firstName"
                id="firstName"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
              />
            </div>
            <div>
              <label for="lastName" class="mb-2 block text-sm font-medium text-stone-700"
                >{t.createNew.lastName}</label
              >
              <input
                type="text"
                name="lastName"
                id="lastName"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
              />
            </div>
          </div>

          <div>
            <label for="email" class="mb-2 block text-sm font-medium text-stone-700"
              >{t.createNew.email}</label
            >
            <input
              type="email"
              name="email"
              id="email"
              required
              class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
            />
          </div>

          <div>
            <label for="domain" class="mb-2 block text-sm font-medium text-stone-700"
              >{t.createNew.domainEndingTitle}</label
            >
            <div class="overflow-hidden rounded-2xl border border-stone-200 bg-stone-50/70 sm:flex sm:items-center">
              <span class="block border-b border-stone-200 px-4 py-3 text-sm text-stone-500 sm:border-b-0 sm:border-r">
                hostingqr.com/
              </span>
              <input
                type="text"
                name="domain"
                id="domain"
                bind:value={domainChoice}
                required
                placeholder={t.createNew.domainEnding}
                class="block w-full bg-transparent px-4 py-3 text-stone-900 outline-none placeholder:text-stone-400"
              />
            </div>
            <p class="mt-3 text-sm text-stone-500">
              Final URL: <span class="font-medium text-stone-700">hostingqr.com/{domainChoice || "..."}</span>
            </p>
          </div>

          <div>
            <label
              for="attachments"
              class="mb-3 block text-sm font-medium text-stone-700"
            >
              {t.createNew.fileUpload}
            </label>

            <div class="rounded-[1.75rem] border border-dashed border-stone-300 bg-stone-50/60 p-5">
              <input
                id="attachments"
                type="file"
                multiple
                accept=".pdf,image/*"
                on:change={handleFileChange}
                class="block w-full cursor-pointer text-sm text-stone-500 file:mr-4 file:rounded-full file:border-0 file:bg-stone-900 file:px-4 file:py-2.5 file:text-sm file:font-medium file:text-white hover:file:bg-stone-800"
              />
              <p class="mt-3 text-sm leading-6 text-stone-500">
                Add one or more images or PDF files. You can remove any item before submitting.
              </p>
            </div>

            {#if previewFiles.length > 0}
              <div class="mt-5 grid grid-cols-2 gap-4 sm:grid-cols-3">
                {#each previewFiles as file, i}
                  <div
                    class="group relative overflow-hidden rounded-[1.25rem] border border-stone-200 bg-white shadow-sm"
                  >
                    {#if file.isImage}
                      <img
                        src={file.url}
                        alt={file.name}
                        class="aspect-square w-full object-cover"
                      />
                    {:else}
                      <div
                        class="flex aspect-square flex-col items-center justify-center bg-stone-50 px-3 text-center"
                      >
                        <div class="rounded-2xl bg-stone-100 px-4 py-3 text-sm font-medium text-stone-700">
                          PDF
                        </div>
                        <span
                          class="mt-3 w-full truncate text-xs text-stone-500"
                          >{file.name}</span
                        >
                      </div>
                    {/if}

                    <div class="border-t border-stone-100 px-3 py-3">
                      <p class="truncate text-sm font-medium text-stone-700">{file.name}</p>
                    </div>

                    <button
                      type="button"
                      on:click={() => removeFile(i)}
                      class="absolute right-3 top-3 inline-flex h-8 w-8 items-center justify-center rounded-full bg-white/95 text-stone-700 shadow-sm transition-all hover:bg-white hover:text-stone-900"
                      aria-label="Remove file"
                    >
                      <svg
                        class="h-4 w-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M6 18L18 6M6 6l12 12"
                        />
                      </svg>
                    </button>
                  </div>
                {/each}
              </div>
            {/if}
          </div>

          <button
            type="submit"
            disabled={loading}
            class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800 disabled:cursor-not-allowed disabled:bg-stone-400"
          >
            {loading ? t.createNew.sending : t.createNew.submit}
          </button>
        </form>
      </section>
    </div>
  </div>

  <SuccessModal show={showSuccess} onClose={closeModals} />
  <ErrorModal
    show={showError}
    onClose={closeModals}
    emailAddress="hostingqr@gmail.com"
  />
</div>
