<script lang="ts">
  import { page } from "$app/stores";
  import { restaurants, type RestaurantSlug } from "$lib/restaurants";
  import { language } from "$lib/stores/language";

  let currentLang: "en" | "sr" | "ru" | "el" = "en";

  language.subscribe((value) => {
    currentLang = value;
  });

  $: slug = $page.params.slug as RestaurantSlug;
  $: restaurant = restaurants[slug];
  $: notFound = !restaurant;
</script>

<svelte:head>
  {#if restaurant}
    <title>{restaurant.name} - HostingQr</title>
    <meta name="description" content={restaurant.description} />
  {:else}
    <title>Not Found - HostingQr</title>
  {/if}
</svelte:head>

<div class="min-h-screen bg-gray-50 flex flex-col">
  {#if notFound}
    <!-- 404 Page -->
    <div class="flex-1 flex flex-col items-center justify-center px-4">
      <div class="text-center max-w-md">
        <h1 class="text-6xl font-bold text-olive-900 mb-4">404</h1>
        <h2 class="text-2xl font-bold text-gray-900 mb-4">Menu Not Found</h2>
        <p class="text-gray-600 mb-8">
          The file you're looking for doesn't exist. Please check the URL or
          return to the homepage.
        </p>
        <a
          href="/"
          class="inline-block px-8 py-3 bg-olive-700 text-white rounded-lg font-semibold hover:bg-olive-800 transition-colors"
        >
          Back to Home
        </a>
      </div>
    </div>
  {:else}
    <!-- Menu Display -->
    <main class="flex-1 flex flex-col items-center justify-center px-4 py-12">
      <div class="max-w-4xl w-full">
        <!-- Restaurant Header -->
        <div class="mb-8 text-center">
          <h1 class="text-4xl sm:text-5xl font-bold text-olive-900 mb-2">
            {restaurant.name}
          </h1>
        </div>

        <!-- Menu Image -->
        <div class="bg-white rounded-lg shadow-xl overflow-hidden mb-8">
          <img
            src={restaurant.image}
            alt="{restaurant.name} Menu"
            class="w-full h-auto"
          />
        </div>
      </div>
    </main>
  {/if}
</div>

<style>
  :global(body) {
    margin: 0;
    padding: 0;
  }
</style>
