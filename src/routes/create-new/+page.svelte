<script lang="ts">
  import { enhance } from "$app/forms"; // 1. Added enhance
  import ErrorModal from "$lib/components/ErrorModal.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import SuccessModal from "$lib/components/SuccessModal.svelte";
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations.js";

  // 2. Added the form prop to receive server data
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

  // 3. Reactive logic: show modal when server returns success
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

  let allFiles: File[] = []; // This stores the actual File objects
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

    // 1. Add new files to our master list (prevents resetting)
    allFiles = [...allFiles, ...newFiles];

    // 2. Update previews
    const newPreviews = newFiles.map((file) => {
      const isImage = file.type.startsWith("image/");
      return {
        id: Math.random().toString(36).substr(2, 9), // unique ID for removal
        name: file.name,
        url: isImage ? URL.createObjectURL(file) : "",
        isImage,
      };
    });

    previewFiles = [...previewFiles, ...newPreviews];

    // Clear the input value so the user can select the same file again if they want
    input.value = "";
  }

  function removeFile(index: number) {
    // Revoke URL to save memory
    if (previewFiles[index].url) {
      URL.revokeObjectURL(previewFiles[index].url);
    }

    // Remove from both arrays
    allFiles = allFiles.filter((_, i) => i !== index);
    previewFiles = previewFiles.filter((_, i) => i !== index);
  }
</script>

<Navigation />

<div class="bg-gray-50 min-h-screen py-32 px-4 sm:px-6 lg:px-8">
  <div
    class="max-w-md mx-auto bg-white rounded-xl flex flex-col gap-y-6 shadow-md overflow-hidden md:max-w-2xl p-8"
  >
    <h1 class="text-3xl font-bold text-gray-900 mb-6 text-center">
      {t.createNew.title}
    </h1>

    <form
      method="POST"
      enctype="multipart/form-data"
      class="space-y-6"
      use:enhance={({ formData }) => {
        loading = true;

        // 1. Clear any 'attachments' already in formData
        formData.delete("attachments");

        // 2. Manually append all files from our internal array
        allFiles.forEach((file) => {
          formData.append("attachments", file);
        });

        return async ({ update }) => {
          await update();
          loading = false;
          // Reset our custom list on success
          if (form?.success) {
            allFiles = [];
            previewFiles = [];
          }
        };
      }}
    >
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label for="firstName" class="block text-lg font-medium text-gray-700"
            >{t.createNew.firstName}</label
          >
          <input
            type="text"
            name="firstName"
            id="firstName"
            required
            class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:ring-olive-500 focus:border-olive-500"
          />
        </div>
        <div>
          <label for="lastName" class="block text-lg font-medium text-gray-700"
            >{t.createNew.lastName}</label
          >
          <input
            type="text"
            name="lastName"
            id="lastName"
            required
            class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:ring-olive-500 focus:border-olive-500"
          />
        </div>
      </div>

      <div>
        <label for="email" class="block text-lg font-medium text-gray-700"
          >{t.createNew.email}</label
        >
        <input
          type="email"
          name="email"
          id="email"
          required
          class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:ring-olive-500 focus:border-olive-500"
        />
      </div>

      <div>
        <label for="domain" class="block text-lg font-medium text-gray-700"
          >{t.createNew.domainEndingTitle}</label
        >
        <div class="mt-1 flex rounded-md shadow-sm">
          <span
            class="inline-flex items-center px-3 rounded-l-md border border-r-0 border-gray-300 bg-gray-50 text-gray-500 text-lg"
          >
            hostingqr.com/
          </span>
          <input
            type="text"
            name="domain"
            id="domain"
            bind:value={domainChoice}
            required
            placeholder={t.createNew.domainEnding}
            class="flex-1 block w-full border border-gray-300 rounded-none rounded-r-md p-2 focus:ring-olive-500 focus:border-olive-500"
          />
        </div>
        <p class="mt-2 text-sm text-gray-500 italic">
          hostingqr.com/{domainChoice || "..."}
        </p>
      </div>

      <div class="space-y-4">
        <label
          for="attachments"
          class="block text-lg font-medium text-gray-700"
        >
          {t.createNew.fileUpload}
        </label>

        <input
          id="attachments"
          type="file"
          multiple
          accept=".pdf,image/*"
          on:change={handleFileChange}
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-olive-50 file:text-olive-700 hover:file:bg-olive-100 cursor-pointer"
        />

        {#if previewFiles.length > 0}
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-4 mt-4">
            {#each previewFiles as file, i}
              <div
                class="relative group aspect-square bg-gray-100 rounded-lg overflow-hidden border border-gray-200"
              >
                {#if file.isImage}
                  <img
                    src={file.url}
                    alt={file.name}
                    class="w-full h-full object-cover"
                  />
                {:else}
                  <div
                    class="flex flex-col items-center justify-center h-full p-2"
                  >
                    <svg
                      class="w-8 h-8 text-red-400"
                      fill="currentColor"
                      viewBox="0 0 20 20"
                    >
                      <path
                        d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z"
                      />
                    </svg>
                    <span
                      class="text-[10px] text-gray-500 mt-1 truncate w-full text-center px-1"
                      >{file.name}</span
                    >
                  </div>
                {/if}

                <button
                  type="button"
                  on:click={() => removeFile(i)}
                  class="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 shadow-lg opacity-0 group-hover:opacity-100 transition-opacity hover:bg-red-600"
                  aria-label="Remove file"
                >
                  <svg
                    class="w-4 h-4"
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
        class="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-lg font-medium text-white bg-olive-600 hover:bg-olive-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-olive-500 disabled:bg-gray-400"
      >
        {loading ? t.createNew.sending : t.createNew.submit}
      </button>
    </form>
  </div>

  <SuccessModal show={showSuccess} onClose={closeModals} />
  <ErrorModal
    show={showError}
    onClose={closeModals}
    emailAddress="hostingqr@gmail.com"
  />
</div>
