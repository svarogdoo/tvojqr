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
  <title>HostingQR</title>
  <meta
    name="description"
    content="HostingQR - your ultimate solution for seamless QR code generation and management. Create, customize, and track QR codes effortlessly with our user-friendly platform."
  />
  <meta
    name="keywords"
    content="QR code, QR code generator, QR code management, QR code tracking, custom QR codes, digital solutions, HostingQR"
  />
  <meta name="author" content="HostingQR" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
</svelte:head>
