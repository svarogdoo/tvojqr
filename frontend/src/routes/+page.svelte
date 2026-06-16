<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Hero from "$lib/components/Hero.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Services from "$lib/components/Services.svelte";
  import WhoAreWe from "$lib/components/WhoAreWe.svelte";
</script>

<div class="flex min-h-screen flex-col">
  <Navigation />
  <main class="flex-1">
    <Hero />
    <Services />
    <WhoAreWe />
  </main>
  <Footer />
</div>

<svelte:head>
  <title>HostingQr</title>
  <meta
    name="description"
    content="HostingQr lets you upload menus or files, choose a public slug, and share them instantly with a QR code."
  />
  <meta
    name="keywords"
    content="HostingQr, QR menu hosting, file hosting, menu QR code, public slug, multilingual menus"
  />
  <meta name="author" content="HostingQr" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
</svelte:head>
