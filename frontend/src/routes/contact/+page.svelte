<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";

  export let form: {
    success?: boolean;
    message?: string;
  } | null;

  let needsLanguages = false;
</script>

<svelte:head>
  <title>Contact - HostingQr</title>
  <meta
    name="description"
    content="Tell HostingQr what you need and we will email you back shortly."
  />
</svelte:head>

<div class="flex min-h-screen flex-col bg-[rgba(243,244,246,0.98)]">
<Navigation />

<main class="flex-1 px-4 pb-16 pt-28 sm:px-6 lg:px-8">
  <section class="mx-auto max-w-6xl">
    <div class="mx-auto max-w-3xl rounded-[2rem] border border-stone-200 bg-white p-8 shadow-[0_18px_50px_rgba(45,53,46,0.08)] sm:p-10">
      <div class="text-center">
        <p class="text-sm font-medium uppercase tracking-[0.24em] text-stone-500">
          Contact
        </p>
        <h1 class="mt-4 text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
          Tell us what you need
        </h1>
        <p class="mx-auto mt-5 max-w-2xl text-base leading-7 text-stone-600">
          Share the file you want hosted, any language needs, and anything else you want us to know.
        </p>
      </div>

      <div class="mt-8">
        {#if form?.success}
          <div class="rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm leading-7 text-emerald-900">
            {form.message}
          </div>
        {:else if form?.message}
          <div class="rounded-2xl border border-rose-200 bg-rose-50 px-5 py-4 text-sm leading-7 text-rose-900">
            {form.message}
          </div>
        {/if}

        <form method="POST" enctype="multipart/form-data" class="mt-8 space-y-5">
          <div class="grid gap-5 sm:grid-cols-2">
            <div>
              <label for="name" class="mb-2 block text-sm font-medium text-stone-700">Name</label>
              <input
                id="name"
                name="name"
                type="text"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
                placeholder="Your name"
              />
            </div>

            <div>
              <label for="email" class="mb-2 block text-sm font-medium text-stone-700">Email</label>
              <input
                id="email"
                name="email"
                type="email"
                required
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
                placeholder="you@example.com"
              />
            </div>
          </div>

          <div>
            <label for="files" class="mb-2 block text-sm font-medium text-stone-700">
              Upload image or PDF (optional)
            </label>
            <input
              id="files"
              name="files"
              type="file"
              multiple
              accept="image/*,.pdf"
              class="block w-full cursor-pointer rounded-2xl border border-dashed border-stone-300 bg-stone-50/60 px-4 py-3 text-sm text-stone-500 file:mr-4 file:rounded-full file:border-0 file:bg-stone-900 file:px-4 file:py-2.5 file:text-sm file:font-medium file:text-white hover:file:bg-stone-800"
            />
          </div>

          <div class="rounded-[1.5rem] border border-stone-200 bg-stone-50/70 p-5">
            <label class="flex items-center gap-3 text-sm font-medium text-stone-800">
              <input
                bind:checked={needsLanguages}
                name="needsLanguages"
                type="checkbox"
                class="h-4 w-4 rounded border-stone-300 text-stone-900 accent-stone-900"
              />
              I need multiple languages
            </label>

            {#if needsLanguages}
              <div class="mt-4">
                <label for="languages" class="mb-2 block text-sm font-medium text-stone-700">
                  Which languages?
                </label>
                <input
                  id="languages"
                  name="languages"
                  type="text"
                  required={needsLanguages}
                  placeholder="English, Spanish, German..."
                  class="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400"
                />
              </div>
            {/if}
          </div>

          <div>
            <label for="message" class="mb-2 block text-sm font-medium text-stone-700">What do you need?</label>
            <textarea
              id="message"
              name="message"
              rows="6"
              required
              class="w-full resize-none rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
              placeholder="Tell us anything else we should know"
            ></textarea>
          </div>

          <button
            type="submit"
            class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
          >
            Send request
          </button>
        </form>
      </div>
    </div>
  </section>
</main>

<Footer />
</div>
