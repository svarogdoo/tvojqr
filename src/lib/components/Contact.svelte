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
<section id="contact" class="py-20 px-4 sm:px-6 lg:px-8 bg-olive-700">
  <div class="max-w-2xl mx-auto">
    <h2 class="text-3xl sm:text-4xl font-bold text-center text-white mb-2">
      {t.contact.title}
    </h2>
    <p class="text-center text-olive-100 mb-10">
      {t.contact.description}
    </p>

    {#if formSubmitted}
      <div
        class="mb-6 p-4 bg-green-100 border border-green-400 text-green-700 rounded-lg animate-bounce"
      >
        {t.contact.success}
      </div>
    {/if}

    <form on:submit={handleSubmit} class="space-y-6">
      <div>
        <label for="name" class="block text-white font-medium mb-2"
          >{t.contact.form.name}</label
        >
        <input
          id="name"
          type="text"
          bind:value={contactForm.name}
          required
          class="w-full px-4 py-3 rounded-lg bg-white/95 text-gray-900 focus:outline-none focus:ring-2 focus:ring-olive-300 transition-all"
          placeholder={t.contact.form.name}
        />
      </div>
      <div>
        <label for="email" class="block text-white font-medium mb-2"
          >{t.contact.form.email}</label
        >
        <input
          id="email"
          type="email"
          bind:value={contactForm.email}
          required
          class="w-full px-4 py-3 rounded-lg bg-white/95 text-gray-900 focus:outline-none focus:ring-2 focus:ring-olive-300 transition-all"
          placeholder={t.contact.form.email}
        />
      </div>
      <div>
        <label for="message" class="block text-white font-medium mb-2"
          >{t.contact.form.message}</label
        >
        <textarea
          id="message"
          bind:value={contactForm.message}
          required
          rows="5"
          class="w-full px-4 py-3 rounded-lg bg-white/95 text-gray-900 focus:outline-none focus:ring-2 focus:ring-olive-300 transition-all resize-none"
          placeholder={t.contact.form.message}
        ></textarea>
      </div>
      <button
        type="submit"
        class="w-full px-6 py-3 bg-white text-olive-700 font-semibold rounded-lg hover:bg-gray-50 hover:scale-105 transition-all duration-300 shadow-lg"
      >
        {t.contact.form.send}
      </button>
    </form>

    <div
      class="mt-12 pt-8 border-t border-olive-600 grid grid-cols-1 md:grid-cols-3 gap-8 text-center text-white"
    >
      <div>
        <div class="text-3xl mb-2">📧</div>
        <p class="text-olive-100">{t.contact.info.email}</p>
      </div>
      <div>
        <div class="text-3xl mb-2">📱</div>
        <p class="text-olive-100">{t.contact.info.phone}</p>
      </div>
      <div>
        <div class="text-3xl mb-2">🌐</div>
        <p class="text-olive-100">{t.contact.info.website}</p>
      </div>
    </div>
  </div>
</section>
