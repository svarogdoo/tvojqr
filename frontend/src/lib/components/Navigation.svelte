<script lang="ts">
  import { apiFetch } from "$lib/api";
  import logo from "$lib/assets/hostinqr-logo-black.svg";
  import { auth, signOut, startGoogleSignIn } from "$lib/stores/auth";
  import { language, languages, type LanguageCode } from "$lib/stores/language";
  import { translations } from "$lib/translations";
  import type { Entitlement } from "$lib/types/projects";
  import { onMount } from "svelte";

  let currentLang: LanguageCode = "en";
  let languageMenuOpen = false;
  let userMenuOpen = false;
  let entitlement: Entitlement | null = null;
  let loadedEntitlementUserId = "";
  const whatsAppUrl = "https://wa.me/35799180703";

  language.subscribe((value: LanguageCode) => {
    currentLang = value;
  });

  function changeLanguage(lang: LanguageCode) {
    language.set(lang);
    languageMenuOpen = false;
  }

  function toggleLanguageMenu() {
    languageMenuOpen = !languageMenuOpen;
    if (languageMenuOpen) {
      userMenuOpen = false;
    }
  }

  function toggleUserMenu() {
    userMenuOpen = !userMenuOpen;
    if (userMenuOpen) {
      languageMenuOpen = false;
    }
  }

  async function handleSignOut() {
    userMenuOpen = false;
    await signOut();
    entitlement = null;
  }

  async function loadEntitlement() {
    if ($auth.status !== "authenticated") {
      entitlement = null;
      loadedEntitlementUserId = "";
      return;
    }

    loadedEntitlementUserId = $auth.user.id;

    try {
      const response = await apiFetch("/api/billing/entitlement");
      entitlement = response.ok ? ((await response.json()) as Entitlement) : null;
    } catch {
      entitlement = null;
    }
  }

  onMount(loadEntitlement);

  const localizedTranslations = translations as any;

  $: t = localizedTranslations[currentLang];
  $: currentLanguage =
    languages.find((lang) => lang.code === currentLang) ?? languages[0];
  $: isAdmin = entitlement?.tier === "admin" && entitlement.isActive;
  $: if ($auth.status === "authenticated" && loadedEntitlementUserId !== $auth.user.id) {
    loadEntitlement();
  }
</script>

<!-- Navigation -->
<nav
  class="fixed top-0 w-full z-50 border-b border-black/5 bg-white/75 backdrop-blur-xl"
>
  <div
    class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between"
  >
    <a href="/" class="text-2xl font-semibold tracking-tight text-stone-700"
      ><div class="flex gap-x-2 items-center">
        <img src={logo} alt="logo" class="w-6 h-6" />
        <p>HostingQr</p>
      </div></a
    >
    <div class="hidden md:flex space-x-6 items-center">
      <a
        href="/#services"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.services}
      </a>
      <a
        href="/example"
        target="_blank"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.examples}
      </a>
      <a
        href="/pricing"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.pricing}
      </a>
      <a
        href="/contact"
        class="text-sm font-medium text-stone-600 transition-colors hover:text-stone-900"
      >
        {t.nav.contact}
      </a>
    </div>

    <div class="flex items-center gap-2">
      <a
        href={whatsAppUrl}
        target="_blank"
        rel="noreferrer"
        class="inline-flex h-10 items-center justify-center gap-2 rounded-full border border-stone-200 bg-white/90 px-3 text-sm font-medium text-stone-700 shadow-sm transition-all hover:border-stone-300 hover:bg-stone-50 hover:text-stone-900"
        aria-label="Message HostingQr on WhatsApp"
      >
        <svg
          class="h-4 w-4 text-[rgba(58,116,78,0.9)]"
          viewBox="0 0 24 24"
          fill="currentColor"
          aria-hidden="true"
        >
          <path
            d="M12.04 2C6.58 2 2.13 6.36 2.13 11.72c0 1.71.46 3.38 1.33 4.85L2 22l5.57-1.43a10.1 10.1 0 0 0 4.47.99c5.46 0 9.91-4.36 9.91-9.72S17.5 2 12.04 2Zm0 17.9c-1.48 0-2.93-.39-4.2-1.14l-.3-.18-3.3.85.88-3.18-.2-.32a8.03 8.03 0 0 1-1.23-4.21c0-4.44 3.75-8.06 8.35-8.06s8.35 3.62 8.35 8.06-3.75 8.18-8.35 8.18Zm4.58-6.03c-.25-.12-1.48-.72-1.71-.8-.23-.09-.4-.12-.57.12-.17.25-.65.8-.8.97-.15.16-.3.18-.55.06-.25-.12-1.06-.38-2.02-1.21-.75-.65-1.25-1.45-1.4-1.7-.15-.25-.02-.38.11-.5.12-.11.25-.29.38-.43.13-.15.17-.25.25-.42.08-.16.04-.31-.02-.43-.06-.12-.57-1.35-.78-1.85-.21-.5-.42-.42-.57-.43h-.49c-.17 0-.44.06-.67.31-.23.25-.88.85-.88 2.07 0 1.22.9 2.4 1.03 2.56.13.16 1.78 2.67 4.31 3.74.6.25 1.07.4 1.44.52.6.19 1.15.16 1.58.1.48-.07 1.48-.6 1.69-1.18.21-.58.21-1.08.15-1.18-.06-.1-.23-.16-.48-.28Z"
          />
        </svg>
        <span class="hidden lg:inline">WhatsApp</span>
      </a>

      <div
        class="relative py-2"
        role="presentation"
        on:mouseleave={() => (languageMenuOpen = false)}
      >
        <button
          type="button"
          on:click={toggleLanguageMenu}
          on:mouseenter={() => {
            languageMenuOpen = true;
            userMenuOpen = false;
          }}
          class="inline-flex items-center gap-2 rounded-full border border-stone-200 bg-white/90 px-3 py-2 text-sm text-stone-700 shadow-sm transition-all hover:border-stone-300 hover:text-stone-900"
        >
          <span>{currentLanguage.flag}</span>
          <span class="hidden sm:inline"
            >{currentLanguage.code.toUpperCase()}</span
          >
          <span class="text-xs text-stone-400">▾</span>
        </button>

        {#if languageMenuOpen}
          <div
            class="absolute right-0 top-full min-w-44 overflow-hidden rounded-2xl border border-stone-200 bg-white/98 shadow-[0_18px_45px_rgba(45,53,46,0.14)] backdrop-blur-sm"
          >
            {#each languages as lang (lang.code)}
              <button
                type="button"
                on:click={() => changeLanguage(lang.code as LanguageCode)}
                class="flex w-full items-center justify-between px-4 py-3 text-left text-sm transition-colors hover:bg-stone-50 {currentLang ===
                lang.code
                  ? 'text-stone-900'
                  : 'text-stone-600'}"
              >
                <span class="flex items-center gap-2">
                  <span>{lang.flag}</span>
                  <span>{lang.name}</span>
                </span>
                {#if currentLang === lang.code}
                  <span class="text-stone-400">•</span>
                {/if}
              </button>
            {/each}
          </div>
        {/if}
      </div>

      <div
        class="relative py-2"
        role="presentation"
        on:mouseleave={() => (userMenuOpen = false)}
      >
        <button
          type="button"
          on:click={toggleUserMenu}
          on:mouseenter={() => {
            userMenuOpen = true;
            languageMenuOpen = false;
          }}
          class="inline-flex h-10 w-10 items-center justify-center rounded-full border border-stone-200 bg-white/90 text-sm font-medium text-stone-700 shadow-sm transition-all hover:border-stone-300 hover:text-stone-900"
          aria-label="User menu"
        >
          {#if $auth.status === "authenticated"}
            {$auth.user.displayName.charAt(0).toUpperCase()}
          {:else}
            <svg
              class="h-4 w-4"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M20 21a8 8 0 0 0-16 0"
              />
              <circle cx="12" cy="8" r="4" />
            </svg>
          {/if}
        </button>

        {#if userMenuOpen}
          <div
            class="absolute right-0 top-full min-w-52 overflow-hidden rounded-2xl border border-stone-200 bg-white/98 shadow-[0_18px_45px_rgba(45,53,46,0.14)] backdrop-blur-sm"
          >
            {#if $auth.status === "authenticated"}
              <div class="border-b border-stone-100 px-4 py-3">
                <p class="text-sm font-medium text-stone-900">
                  {$auth.user.displayName}
                </p>
                <p class="mt-1 text-xs text-stone-500">{$auth.user.email}</p>
              </div>
              <a
                href="/dashboard"
                class="block px-4 py-3 text-sm text-stone-700 transition-colors hover:bg-stone-50"
              >
                Your projects
              </a>
              <a
                href="/account"
                class="block border-t border-stone-100 px-4 py-3 text-sm text-stone-700 transition-colors hover:bg-stone-50"
              >
                Account & billing
              </a>
              {#if isAdmin}
                <a
                  href="/admin/overview"
                  class="block border-t border-stone-100 px-4 py-3 text-sm text-stone-700 transition-colors hover:bg-stone-50"
                >
                  Admin overview
                </a>
              {/if}
              <button
                type="button"
                on:click={handleSignOut}
                class="block w-full border-t border-stone-100 px-4 py-3 text-left text-sm text-stone-700 transition-colors hover:bg-stone-50"
              >
                Log out
              </button>
            {:else if $auth.status === "loading"}
              <div class="px-4 py-3 text-sm text-stone-500">
                Checking session...
              </div>
            {:else}
              <button
                type="button"
                on:click={startGoogleSignIn}
                class="block w-full px-4 py-3 text-left text-sm text-stone-700 transition-colors hover:bg-stone-50"
              >
                Sign in
              </button>
              <button
                type="button"
                on:click={startGoogleSignIn}
                class="block w-full border-t border-stone-100 px-4 py-3 text-left text-sm text-stone-700 transition-colors hover:bg-stone-50"
              >
                Register
              </button>
            {/if}
          </div>
        {/if}
      </div>
    </div>
  </div>
</nav>
