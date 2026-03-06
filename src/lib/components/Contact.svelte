<script lang="ts">
  import { language } from "$lib/stores/language";
  import { translations } from "$lib/translations";
  import type { FormEventHandler } from "svelte/elements";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";
  let contactForm = { name: "", email: "", message: "" };
  let formSubmitted = false;

  language.subscribe((value) => {
    currentLang = value;
  });

  const handleSubmit: FormEventHandler<HTMLFormElement> = (e) => {
    e.preventDefault();
    formSubmitted = true;
    contactForm = { name: "", email: "", message: "" };
    setTimeout(() => {
      formSubmitted = false;
    }, 3000);
  };

  $: t = translations[currentLang as "en" | "sr" | "ru" | "el"];
</script>

<!-- Contact Section -->
<section id="contact" class="px-4 py-20 sm:px-6 lg:px-8">
  <div class="mx-auto grid max-w-6xl gap-8 lg:grid-cols-[0.85fr_1.15fr] lg:items-start">
    <div class="rounded-[2rem] border border-black/6 bg-[rgba(218,226,212,0.9)] p-8 shadow-[0_20px_50px_rgba(45,53,46,0.06)] backdrop-blur-sm sm:p-10">
      <div class="max-w-md">
        <p class="mb-4 text-sm font-medium uppercase tracking-[0.2em] text-stone-500">
          Contact
        </p>
        <h2 class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl">
          {t.contact.title}
        </h2>
        <p class="mt-4 text-base leading-7 text-stone-600">
          {t.contact.description}
        </p>
      </div>

      <div class="mt-10 grid gap-4 text-stone-700">
        <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
          <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Email</p>
          <p class="mt-2 font-medium text-stone-900">{t.contact.info.email}</p>
        </div>
        <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
          <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Phone</p>
          <p class="mt-2 font-medium text-stone-900">{t.contact.info.phone}</p>
        </div>
        <div class="rounded-2xl border border-white/70 bg-white/80 px-5 py-4 shadow-sm">
          <p class="text-xs uppercase tracking-[0.18em] text-stone-500">Website</p>
          <p class="mt-2 font-medium text-stone-900">{t.contact.info.website}</p>
        </div>
      </div>
    </div>

    <div class="rounded-[2rem] border border-black/6 bg-white/82 p-8 shadow-[0_20px_50px_rgba(45,53,46,0.07)] backdrop-blur-sm sm:p-10">
      <h3 class="text-2xl font-semibold tracking-tight text-stone-900">
        Send a message
      </h3>
      <p class="mt-3 text-sm leading-7 text-stone-600">
        For now this is a simple contact placeholder while the product onboarding is being rebuilt.
      </p>

      {#if formSubmitted}
        <div
          class="mt-6 rounded-2xl border border-[rgba(94,117,97,0.18)] bg-[rgba(236,245,238,0.9)] px-4 py-3 text-sm font-medium text-[color:var(--success-strong)]"
        >
          {t.contact.success}
        </div>
      {/if}

      <form on:submit={handleSubmit} class="mt-8 space-y-5">
        <div>
          <label for="name" class="mb-2 block text-sm font-medium text-stone-700"
            >{t.contact.form.name}</label
          >
          <input
            id="name"
            type="text"
            bind:value={contactForm.name}
            required
            class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
            placeholder={t.contact.form.name}
          />
        </div>
        <div>
          <label for="email" class="mb-2 block text-sm font-medium text-stone-700"
            >{t.contact.form.email}</label
          >
          <input
            id="email"
            type="email"
            bind:value={contactForm.email}
            required
            class="w-full rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
            placeholder={t.contact.form.email}
          />
        </div>
        <div>
          <label for="message" class="mb-2 block text-sm font-medium text-stone-700"
            >{t.contact.form.message}</label
          >
          <textarea
            id="message"
            bind:value={contactForm.message}
            required
            rows="5"
            class="w-full resize-none rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3 text-stone-900 outline-none transition-all focus:border-stone-400 focus:bg-white"
            placeholder={t.contact.form.message}
          ></textarea>
        </div>
        <button
          type="submit"
          class="inline-flex w-full items-center justify-center rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
        >
          {t.contact.form.send}
        </button>
      </form>
    </div>
  </div>
</section>
