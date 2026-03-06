<script lang="ts">
  import { page } from "$app/stores";
  import { restaurants, type RestaurantSlug } from "$lib/restaurants";
  import { language, languages } from "$lib/stores/language";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  const pageCopy = {
    en: {
      label: "Hosted page",
      preview: "Example public page",
      languageTitle: "Interface language",
      description: "This is the current shared page view for a hosted menu or document.",
      notFoundTitle: "Page not found",
      backHome: "Back to home",
    },
    sr: {
      label: "Hostovana stranica",
      preview: "Primer javne stranice",
      languageTitle: "Jezik interfejsa",
      description: "Ovo je trenutni prikaz deljene stranice za hostovani meni ili dokument.",
      notFoundTitle: "Stranica nije pronađena",
      backHome: "Nazad na početnu",
    },
    ru: {
      label: "Хостируемая страница",
      preview: "Пример публичной страницы",
      languageTitle: "Язык интерфейса",
      description: "Это текущий вид общей страницы для размещенного меню или документа.",
      notFoundTitle: "Страница не найдена",
      backHome: "Назад на главную",
    },
    el: {
      label: "Φιλοξενούμενη σελίδα",
      preview: "Παράδειγμα δημόσιας σελίδας",
      languageTitle: "Γλώσσα διεπαφής",
      description: "Αυτή είναι η τρέχουσα κοινόχρηστη προβολή για ένα φιλοξενούμενο μενού ή έγγραφο.",
      notFoundTitle: "Η σελίδα δεν βρέθηκε",
      backHome: "Επιστροφή στην αρχική",
    },
  };

  language.subscribe((value) => {
    currentLang = value;
  });

  $: slug = $page.params.slug as RestaurantSlug;
  $: restaurant = restaurants[slug];
  $: notFound = !restaurant;
  $: copy = pageCopy[currentLang];
</script>

<svelte:head>
  {#if restaurant}
    <title>{restaurant.name} - HostingQr</title>
    <meta name="description" content={restaurant.description} />
  {:else}
    <title>Not Found - HostingQr</title>
  {/if}
</svelte:head>

<div class="min-h-screen px-4 pb-16 pt-10 sm:px-6 lg:px-8">
  {#if notFound}
    <div class="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center">
      <div class="w-full rounded-[2rem] border border-black/6 bg-white/86 p-10 text-center shadow-[0_24px_60px_rgba(45,53,46,0.08)] backdrop-blur-sm sm:p-14">
        <div class="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-100 text-sm font-semibold text-stone-600">
          404
        </div>
        <h2 class="text-3xl font-semibold tracking-tight text-stone-900 sm:text-4xl">{copy.notFoundTitle}</h2>
        <p class="mx-auto mt-4 max-w-md text-base leading-7 text-stone-600">
          The file you're looking for doesn't exist. Please check the URL or return to the homepage.
        </p>
        <a
          href="/"
          class="mt-8 inline-flex items-center rounded-full bg-stone-900 px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-stone-800"
        >
          {copy.backHome}
        </a>
      </div>
    </div>
  {:else}
    <main class="mx-auto max-w-5xl py-6 sm:py-10">
      <div class="mb-5 flex flex-wrap items-center justify-between gap-3 px-1">
        <div>
          <p class="text-sm font-medium text-stone-500">hostingqr.com/{slug}</p>
          <p class="mt-1 text-xl font-semibold text-stone-900">{restaurant.name}</p>
        </div>
        <div class="flex flex-wrap gap-2">
          {#each languages as lang (lang.code)}
            <button
              type="button"
              on:click={() => language.set(lang.code as "en" | "sr" | "ru" | "el")}
              class="rounded-full px-3 py-1.5 text-sm transition-all {currentLang === lang.code
                ? 'bg-stone-900 text-white shadow-sm'
                : 'border border-stone-200 bg-white text-stone-600 hover:border-stone-300 hover:text-stone-900'}"
            >
              {lang.flag}
            </button>
          {/each}
          <a
            href="/"
            class="rounded-full border border-stone-200 bg-white px-4 py-2 text-sm font-medium text-stone-700 transition-colors hover:border-stone-300 hover:text-stone-900"
          >
            {copy.backHome}
          </a>
        </div>
      </div>

      <section class="overflow-hidden rounded-[2rem] border border-black/6 bg-white/90 p-3 shadow-[0_24px_60px_rgba(45,53,46,0.08)] backdrop-blur-sm sm:p-5">
        <img
          src={restaurant.image}
          alt="{restaurant.name} Menu"
          class="w-full rounded-[1.5rem] h-auto"
        />
      </section>
    </main>
  {/if}
</div>

<style>
  :global(body) {
    margin: 0;
    padding: 0;
  }
</style>
