<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Hero from "$lib/components/Hero.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import Seo from "$lib/components/Seo.svelte";
  import Services from "$lib/components/Services.svelte";
  import WhoAreWe from "$lib/components/WhoAreWe.svelte";
  import { siteUrl } from "$lib/config";

  const title = "HostingQr - QR Code Menu and File Hosting";
  const description =
    "Create a hosted page for menus, images, and documents, share it with a QR code, and update the content without reprinting.";
  const structuredData = JSON.stringify({
    "@context": "https://schema.org",
    "@graph": [
      {
        "@type": "Organization",
        "@id": `${siteUrl}/#organization`,
        name: "HostingQr",
        legalName: "Svarog d.o.o.",
        url: siteUrl,
        email: "support@hostingqr.com",
      },
      {
        "@type": "WebSite",
        "@id": `${siteUrl}/#website`,
        url: siteUrl,
        name: "HostingQr",
        publisher: { "@id": `${siteUrl}/#organization` },
      },
      {
        "@type": "SoftwareApplication",
        "@id": `${siteUrl}/#software`,
        name: "HostingQr",
        url: siteUrl,
        applicationCategory: "BusinessApplication",
        operatingSystem: "Web",
        description,
        provider: { "@id": `${siteUrl}/#organization` },
      },
    ],
  }).replace(/</g, "\\u003c");
</script>

<Seo {title} {description} path="/" />

<svelte:head>
  {@html `<script type="application/ld+json">${structuredData}</script>`}
</svelte:head>

<div class="flex min-h-screen flex-col">
  <Navigation />
  <main class="flex-1">
    <Hero />
    <Services />
    <WhoAreWe />
  </main>
  <Footer />
</div>
