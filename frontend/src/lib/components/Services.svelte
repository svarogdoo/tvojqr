<script lang="ts">
  const traditionalSteps = [
    "Print new menus every time prices or items change",
    "Replace stickers, table tents, or cards again and again",
    "Keep paying for new print runs",
    "Extra prints for special short term offering",
  ];

  const ourSteps = [
    "Update the offer in seconds",
    "Keep the same QR code forever",
    "Add translations without reprinting",
    "Add and remove special offers in few clicks",
  ];
</script>

<!-- Services Section -->
<section
  id="services"
  class="border-y border-black/8 bg-[rgba(220,228,216,0.92)] px-4 py-20 sm:px-6 lg:px-8"
>
  <div class="max-w-6xl mx-auto">
    <div class="mx-auto mb-16 max-w-3xl text-center">
      <h2 class="text-4xl font-semibold tracking-tight text-stone-900 sm:text-5xl">
        Why use us?
      </h2>
      <p class="mt-4 text-lg leading-8 text-stone-600">
        Faster updates, no reprints, and easy translations when you need them.
      </p>
    </div>

    <div class="grid gap-6 lg:grid-cols-2">
      <div class="rounded-[2rem] border border-stone-200 bg-[linear-gradient(180deg,rgba(255,255,255,0.96),rgba(244,244,243,0.96))] p-8 shadow-[0_16px_40px_rgba(45,53,46,0.07)] sm:p-10">
        <p class="text-xs font-medium uppercase tracking-[0.24em] text-stone-500">Traditional solution</p>
        <h3 class="mt-4 text-3xl font-semibold tracking-tight text-stone-900">What you usually have to do</h3>
        <ul class="mt-8 space-y-4">
          {#each traditionalSteps as step}
            <li class="flex gap-4 text-sm leading-7 text-stone-600">
              <span class="mt-2 h-2.5 w-2.5 shrink-0 rounded-full bg-stone-300"></span>
              <span>{step}</span>
            </li>
          {/each}
        </ul>
      </div>

      <div class="rounded-[2rem] border border-stone-200 bg-[linear-gradient(135deg,rgba(245,250,244,0.99),rgba(232,241,230,0.97))] p-8 text-stone-900 shadow-[0_20px_55px_rgba(94,117,97,0.05)] sm:p-10">
        <p class="text-xs font-medium uppercase tracking-[0.24em] text-stone-500">Our solution</p>
        <h3 class="mt-4 text-3xl font-semibold tracking-tight">What you do with us</h3>
        <ul class="mt-8 space-y-4">
          {#each ourSteps as step}
            <li class="flex gap-4 text-sm leading-7 text-stone-700">
              <span class="mt-2 h-2.5 w-2.5 shrink-0 rounded-full bg-[rgba(198,208,194,0.95)]"></span>
              <span>{step}</span>
            </li>
          {/each}
        </ul>
      </div>
    </div>

    <div class="mt-10 flex justify-center">
      <a
        href="/pricing"
        class="inline-flex items-center gap-2 rounded-full bg-stone-900 px-6 py-3.5 text-base font-medium text-white transition-all duration-300 hover:-translate-y-0.5 hover:bg-stone-800"
      >
        <span class="h-2 w-2 rounded-full bg-white/70 animate-pulse"></span>
        See pricing
      </a>
    </div>
  </div>
</section>
