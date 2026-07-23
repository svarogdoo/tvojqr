<script lang="ts">
  import socialImage from "$lib/assets/hero-image.jpg";
  import { siteUrl } from "$lib/config";

  let {
    title,
    description,
    path,
    robots = "index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1",
  }: {
    title: string;
    description: string;
    path: string;
    robots?: string;
  } = $props();

  const canonicalUrl = $derived(`${siteUrl}${path}`);
  const socialImageUrl = `${siteUrl}${socialImage}`;
</script>

<svelte:head>
  <title>{title}</title>
  <meta name="description" content={description} />
  <meta name="robots" content={robots} />
  <link rel="canonical" href={canonicalUrl} />

  <meta property="og:type" content="website" />
  <meta property="og:site_name" content="HostingQr" />
  <meta property="og:title" content={title} />
  <meta property="og:description" content={description} />
  <meta property="og:url" content={canonicalUrl} />
  <meta property="og:image" content={socialImageUrl} />
  <meta property="og:image:alt" content="A sample digital menu that can be shared with HostingQr" />

  <meta name="twitter:card" content="summary_large_image" />
  <meta name="twitter:title" content={title} />
  <meta name="twitter:description" content={description} />
  <meta name="twitter:image" content={socialImageUrl} />
</svelte:head>
