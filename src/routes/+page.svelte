<script lang="ts">
  import Contact from "$lib/components/Contact.svelte";
  import Footer from "$lib/components/Footer.svelte";
  import Hero from "$lib/components/Hero.svelte";
  import HowItWorks from "$lib/components/HowItWorks.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Services from "$lib/components/Services.svelte";
</script>

<Navigation />
<Hero />
<Services />
<HowItWorks />
<Contact />
<Footer />

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
