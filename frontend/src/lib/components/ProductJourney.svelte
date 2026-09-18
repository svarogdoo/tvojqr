<script lang="ts">
  import { homepageCopy } from "$lib/homepageCopy";
  import { language, type LanguageCode } from "$lib/stores/language";
  import { onMount } from "svelte";

  let currentLang: LanguageCode = "en";
  let qrMount: HTMLAnchorElement;
  const panameMenuImage =
    "https://pub-e2af383507a44b5b8e7458cf487429f7.r2.dev/projects/c9b71089cebc4dc7bce8bc4c0cceb8e6/languages/default/d3d3477cb54c42e8a9ba29cf2bb56df9.webp";

  function splitHighlight(title: string, highlight: string) {
    const start = title
      .toLocaleLowerCase()
      .indexOf(highlight.toLocaleLowerCase());
    return start < 0
      ? { before: title, highlight: "", after: "" }
      : {
          before: title.slice(0, start),
          highlight: title.slice(start, start + highlight.length),
          after: title.slice(start + highlight.length),
        };
  }

  language.subscribe((value) => {
    currentLang = value;
  });

  $: copy = homepageCopy[currentLang].journey;
  $: questionBreak = copy.question.lastIndexOf(" ");
  $: redesignTitle = splitHighlight(
    copy.redesign.title,
    copy.redesign.highlight,
  );
  $: setupTitle = splitHighlight(copy.setup.title, copy.setup.highlight);

  onMount(async () => {
    const { default: QRCodeStyling } = await import("qr-code-styling");
    const qrCode = new QRCodeStyling({
      data: "https://hostingqr.com/example",
      width: 116,
      height: 116,
      margin: 4,
      qrOptions: { errorCorrectionLevel: "Q" },
      dotsOptions: { color: "#3f4d40", type: "rounded" },
      backgroundOptions: { color: "#ffffff" },
      cornersSquareOptions: { color: "#3f4d40", type: "extra-rounded" },
      cornersDotOptions: { color: "#3f4d40", type: "dot" },
    });

    qrCode.append(qrMount);
  });
</script>

<section
  id="services"
  class="journey-section relative overflow-hidden border-b border-black/8 px-3 pb-20 pt-24 sm:px-6 sm:pb-24 sm:pt-24 lg:px-8"
>
  <div class="ambient-shape ambient-one" aria-hidden="true"></div>
  <div class="ambient-shape ambient-two" aria-hidden="true"></div>
  <div class="ambient-shape ambient-three" aria-hidden="true"></div>
  <div class="ambient-shape ambient-four" aria-hidden="true"></div>

  <div class="relative z-10 mx-auto max-w-5xl">
    <header class="mx-auto max-w-3xl text-center">
      <h1
        class="text-3xl font-semibold tracking-[-0.04em] text-stone-900 sm:text-5xl"
      >
        {copy.question.slice(0, questionBreak + 1)}<span class="headline-accent"
          >{copy.question.slice(questionBreak + 1)}</span
        >
      </h1>
      <p class="intro-copy">{copy.subtitle}</p>
      <a class="pricing-link" href="/pricing">{copy.pricingCta}</a>
    </header>

    <div class="journey mx-auto mt-5 max-w-4xl sm:mt-7">
      <div class="stem h-7 sm:h-8" aria-hidden="true"></div>

      <div class="fork fork-down" aria-hidden="true"></div>
      <div class="grid grid-cols-2 gap-3 sm:gap-8">
        <article class="journey-card branch-card product-card">
          <div class="product-copy">
            <h2>{copy.imageMenu.title}</h2>
            <p>{copy.imageMenu.description}</p>
            <a
              class="card-arrow"
              href="https://hostingqr.com/paname"
              target="_blank"
              rel="noreferrer"
              aria-label={copy.imageMenu.example}
            >
              <span aria-hidden="true">→</span>
            </a>
          </div>
          <div class="phone-preview" aria-hidden="true">
            <span class="phone-speaker" aria-hidden="true"></span>
            <span class="phone-screen image-menu-screen">
              <img src={panameMenuImage} alt="" />
            </span>
          </div>
        </article>

        <article class="journey-card branch-card product-card">
          <div class="product-copy">
            <h2>{copy.webMenu.title}</h2>
            <p>{copy.webMenu.description}</p>
            <a
              class="card-arrow"
              href="/example"
              target="_blank"
              rel="noreferrer"
              aria-label={copy.webMenu.example}
            >
              <span aria-hidden="true">→</span>
            </a>
          </div>
          <div class="phone-preview" aria-hidden="true">
            <span class="phone-speaker" aria-hidden="true"></span>
            <span class="phone-screen web-menu-screen" aria-hidden="true">
              <span class="web-menu-hero">
                <span class="web-menu-mark">L</span>
                <span>
                  <strong>Luna</strong>
                  <small>Kitchen & Bar</small>
                </span>
              </span>
              <span class="web-menu-tabs">
                <b>Menu</b>
                <span>Drinks</span>
                <span>Dessert</span>
              </span>
              <span class="web-menu-section">Starters</span>
              <span class="web-menu-item">
                <span
                  ><b>Bruschetta</b><small>Tomato, basil, olive oil</small
                  ></span
                >
                <strong>€8</strong>
              </span>
              <span class="web-menu-item">
                <span><b>Burrata</b><small>Seasonal tomatoes</small></span>
                <strong>€12</strong>
              </span>
              <span class="web-menu-section">Mains</span>
              <span class="web-menu-item">
                <span><b>Fresh pasta</b><small>Herbs, parmesan</small></span>
                <strong>€16</strong>
              </span>
            </span>
          </div>
        </article>
      </div>

      <div class="parallel-lines" aria-hidden="true"></div>
      <div class="grid grid-cols-2 gap-3 sm:gap-8">
        <article class="journey-card branch-card support-card">
          <h2>
            <span class="setup-title-icon" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path
                  d="m4 20 8.5-8.5M14 3l1.2 3.8L19 8l-3.8 1.2L14 13l-1.2-3.8L9 8l3.8-1.2L14 3Z"
                ></path>
              </svg>
            </span>
            <span>
              {redesignTitle.before}{#if redesignTitle.highlight}<mark
                  >{redesignTitle.highlight}</mark
                >{/if}{redesignTitle.after}
            </span>
          </h2>
          <p>{copy.redesign.description}</p>
        </article>

        <article class="journey-card branch-card support-card">
          <h2>
            <span class="setup-title-icon" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <circle cx="12" cy="12" r="3"></circle>
                <path
                  d="M12 3v3M12 18v3M3 12h3M18 12h3M5.6 5.6l2.1 2.1M16.3 16.3l2.1 2.1M18.4 5.6l-2.1 2.1M7.7 16.3l-2.1 2.1"
                ></path>
              </svg>
            </span>
            <span>
              {setupTitle.before}{#if setupTitle.highlight}<mark
                  >{setupTitle.highlight}</mark
                >{/if}{setupTitle.after}
            </span>
          </h2>
          <p>{copy.setup.description}</p>
        </article>
      </div>

      <div class="fork fork-up" aria-hidden="true"></div>
      <article class="journey-card shared-card visual-card">
        <div class="step-visual translation-list" aria-hidden="true">
          <div><span>🇬🇧</span><span>Menu</span></div>
          <div><span>🇪🇸</span><span>Menú</span></div>
          <div><span>🇮🇹</span><span>Menù</span></div>
          <div><span>🇭🇷</span><span>Jelovnik</span></div>
        </div>
        <div class="shared-copy">
          <h2>{copy.translations.title}</h2>
          <p>{copy.translations.description}</p>
        </div>
      </article>

      <div class="stem shared-gap" aria-hidden="true"></div>
      <article class="journey-card shared-card visual-card">
        <a
          class="step-visual qr-visual"
          href="/example"
          target="_blank"
          rel="noreferrer"
          aria-label={copy.webMenu.example}
          bind:this={qrMount}
        ></a>
        <div class="shared-copy">
          <h2>{copy.qrCode.title}</h2>
          <p>{copy.qrCode.description}</p>
        </div>
      </article>

      <div class="stem shared-gap" aria-hidden="true"></div>
      <article class="journey-card shared-card visual-card maps-card">
        <div class="step-visual maps-visual" aria-hidden="true">
          <div class="mini-map">
            <span class="maps-label">Google Maps</span>
            <svg class="map-pin" viewBox="0 0 24 24" fill="currentColor">
              <path
                d="M12 2a7 7 0 0 0-7 7c0 5.2 7 13 7 13s7-7.8 7-13a7 7 0 0 0-7-7Zm0 9.5A2.5 2.5 0 1 1 12 6a2.5 2.5 0 0 1 0 5.5Z"
              ></path>
            </svg>
          </div>
          <div class="business-preview">
            <div>
              <strong>Luna Kitchen & Bar</strong>
              <span><b>4.9</b> ★★★★★</span>
            </div>
            <span class="menu-link">Menu</span>
          </div>
        </div>
        <div class="shared-copy">
          <h2>{copy.googleMaps.title}</h2>
          <p>{copy.googleMaps.description}</p>
        </div>
      </article>

      <div class="stem shared-gap" aria-hidden="true"></div>
      <article class="journey-card changes-card">
        <h2>{copy.changes.title}</h2>
        <p>{copy.changes.description}</p>
      </article>

      <div class="stem shared-gap" aria-hidden="true"></div>
      <aside class="contact-callout">
        <h2>{copy.contact.title}</h2>
        <p>{copy.contact.description}</p>
        <div class="contact-actions">
          <a href="https://wa.me/35799180703" target="_blank" rel="noreferrer">
            {copy.contact.whatsapp}
          </a>
          <a href="mailto:support@hostingqr.com">{copy.contact.email}</a>
        </div>
      </aside>
    </div>
  </div>
</section>

<style>
  .journey-section {
    background: #f8f7f2;
  }

  .ambient-shape {
    position: absolute;
    border-radius: 50%;
    background: rgba(221, 228, 216, 0.5);
    filter: blur(1px);
    pointer-events: none;
  }

  .ambient-one {
    top: -8rem;
    left: -11rem;
    width: 22rem;
    height: 22rem;
  }

  .ambient-two {
    top: -5rem;
    right: -12rem;
    width: 24rem;
    height: 24rem;
  }

  .ambient-three {
    top: 58%;
    left: -13rem;
    width: 25rem;
    height: 25rem;
    opacity: 0.72;
  }

  .ambient-four {
    right: -14rem;
    bottom: 4rem;
    width: 27rem;
    height: 27rem;
    opacity: 0.62;
  }

  header h1,
  .journey-card h2 {
    font-family: Georgia, "Times New Roman", serif;
  }

  .headline-accent {
    color: #526856;
    font-weight: inherit;
  }

  .intro-copy {
    max-width: 38rem;
    margin: 0.65rem auto 0;
    color: #6c706a;
    font-size: 0.9rem;
    line-height: 1.65;
  }

  .pricing-link {
    display: inline-flex;
    min-height: 2.35rem;
    align-items: center;
    justify-content: center;
    margin-top: 0.7rem;
    border: 1px solid #ccd5c8;
    border-radius: 9999px;
    padding: 0.5rem 1rem;
    background: rgba(255, 255, 255, 0.62);
    color: #465849;
    font-size: 0.75rem;
    font-weight: 650;
    transition:
      background-color 180ms ease,
      border-color 180ms ease;
  }

  .pricing-link:hover {
    border-color: #aebdab;
    background: rgba(255, 255, 255, 0.9);
  }

  .pricing-link:focus-visible {
    outline: 3px solid rgba(95, 109, 96, 0.25);
    outline-offset: 3px;
  }

  .journey {
    --line: #83967f;
    --line-width: 1.5px;
  }

  .stem {
    width: var(--line-width);
    margin-inline: auto;
    background: var(--line);
  }

  .fork {
    position: relative;
    height: 2.25rem;
  }

  .fork::after {
    position: absolute;
    content: "";
  }

  .fork::after {
    left: 25%;
    right: 25%;
    height: 50%;
    border-left: var(--line-width) solid var(--line);
    border-right: var(--line-width) solid var(--line);
  }

  .fork-down {
    background: linear-gradient(var(--line), var(--line)) center top /
      var(--line-width) 50% no-repeat;
  }

  .fork-down::before {
    content: none;
  }

  .fork-down::after {
    bottom: 0;
    border-top: var(--line-width) solid var(--line);
    border-radius: 2rem 2rem 0 0;
  }

  .fork-up {
    background: linear-gradient(var(--line), var(--line)) center bottom /
      var(--line-width) 50% no-repeat;
  }

  .fork-up::before {
    content: none;
  }

  .fork-up::after {
    top: 0;
    border-bottom: var(--line-width) solid var(--line);
    border-radius: 0 0 2rem 2rem;
  }

  .parallel-lines {
    height: 2.25rem;
    background:
      linear-gradient(var(--line), var(--line)) 25% center / var(--line-width)
        100% no-repeat,
      linear-gradient(var(--line), var(--line)) 75% center / var(--line-width)
        100% no-repeat;
  }

  .journey-card {
    position: relative;
    z-index: 1;
    border: 1px solid rgba(255, 255, 255, 0.92);
    background: rgba(255, 255, 255, 0.78);
    box-shadow: 0 14px 34px rgba(45, 53, 46, 0.065);
    backdrop-filter: blur(14px);
  }

  .branch-card {
    min-width: 0;
    border-radius: 1.5rem;
    padding: 1rem;
    text-align: left;
  }

  .product-card {
    display: flex;
    flex-direction: column;
  }

  .product-copy {
    display: flex;
    min-width: 0;
    flex: 1 1 auto;
    flex-direction: column;
    align-items: flex-start;
  }

  .product-copy h2 {
    margin-top: 0;
  }

  .support-card {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
  }

  .support-card h2 {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.45rem;
    margin-top: 0;
  }

  .setup-title-icon {
    display: inline-grid;
    width: 1.8rem;
    height: 1.8rem;
    flex: 0 0 auto;
    place-items: center;
    border-radius: 0.55rem;
    background: #e5ede1;
    color: #49604d;
  }

  .setup-title-icon svg {
    width: 0.95rem;
    height: 0.95rem;
    stroke-width: 1.8;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  .support-card mark {
    border-radius: 0.35rem;
    padding: 0.04em 0.28em 0.08em;
    background: #dce7d7;
    color: #405744;
    font: inherit;
  }

  .support-card p {
    margin-top: 0.4rem;
  }

  .card-arrow {
    display: grid;
    width: 2.5rem;
    height: 2.5rem;
    place-items: center;
    border-radius: 50%;
    background: #e8eee4;
    color: #354337;
    font-family: Arial, sans-serif;
    font-size: 1.25rem;
  }

  .card-arrow {
    margin-top: 1rem;
    transition:
      background-color 180ms ease,
      transform 180ms ease;
  }

  .card-arrow:hover {
    background: #dbe5d6;
    transform: translateX(2px);
  }

  .card-arrow:focus-visible {
    outline: 3px solid rgba(95, 109, 96, 0.3);
    outline-offset: 3px;
  }

  .shared-card {
    display: flex;
    width: min(100%, 35rem);
    align-items: center;
    gap: 1rem;
    margin-inline: auto;
    border-radius: 1.4rem;
    padding: 1rem;
  }

  .journey-card h2 {
    margin-top: 0.8rem;
    color: #292c29;
    font-size: 1rem;
    font-weight: 600;
    line-height: 1.25;
    letter-spacing: -0.02em;
  }

  .shared-card h2 {
    margin-top: 0;
  }

  .shared-copy {
    flex: 1 1 auto;
    min-width: 0;
  }

  .journey-card p {
    margin-top: 0.5rem;
    color: #666b64;
    font-size: 0.75rem;
    line-height: 1.55;
  }

  .phone-preview {
    position: relative;
    display: block;
    width: min(100%, 11.5rem);
    aspect-ratio: 9 / 17;
    margin: 1rem auto 0;
    overflow: hidden;
    border: 0.2rem solid #30332f;
    border-radius: 1.4rem;
    padding: 0.24rem 0.12rem 0.12rem;
    background: #30332f;
    box-shadow: 0 14px 30px rgba(39, 44, 38, 0.2);
  }

  .phone-speaker {
    position: absolute;
    z-index: 2;
    top: 0.06rem;
    left: 50%;
    width: 22%;
    height: 0.12rem;
    border-radius: 9999px;
    background: #777b75;
    transform: translateX(-50%);
  }

  .phone-screen {
    display: block;
    width: 100%;
    height: 100%;
    overflow: hidden;
    border-radius: 1.05rem;
    background: #f7f3ea;
  }

  .image-menu-screen img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    object-position: top center;
  }

  .web-menu-screen {
    padding: 8% 7%;
    color: #373a34;
    font-size: clamp(0.4rem, 1.25vw, 0.58rem);
    text-align: left;
  }

  .web-menu-hero,
  .web-menu-item {
    display: flex;
    align-items: center;
  }

  .web-menu-hero {
    gap: 0.45em;
    padding: 5% 2% 8%;
  }

  .web-menu-hero strong,
  .web-menu-hero small,
  .web-menu-item b,
  .web-menu-item small {
    display: block;
  }

  .web-menu-hero strong {
    font-family: Georgia, serif;
    font-size: 1.5em;
  }

  .web-menu-hero small,
  .web-menu-item small {
    margin-top: 0.15em;
    color: #85877f;
    font-size: 0.8em;
  }

  .web-menu-mark {
    display: grid;
    width: 2.5em;
    height: 2.5em;
    place-items: center;
    border-radius: 50%;
    background: #566451;
    color: white;
    font-family: Georgia, serif;
    font-size: 1.1em;
  }

  .web-menu-tabs {
    display: flex;
    gap: 0.5em;
    overflow: hidden;
    border-block: 1px solid #e3ded2;
    padding-block: 0.65em;
    white-space: nowrap;
  }

  .web-menu-tabs b {
    color: #566451;
  }

  .web-menu-section {
    display: block;
    margin-top: 1.4em;
    font-family: Georgia, serif;
    font-size: 1.2em;
    font-weight: 700;
  }

  .web-menu-item {
    justify-content: space-between;
    gap: 0.5em;
    border-bottom: 1px solid #e7e2d7;
    padding-block: 0.85em;
  }

  .web-menu-item > span {
    min-width: 0;
  }

  .web-menu-item > strong {
    color: #566451;
  }

  .step-visual {
    flex: 0 0 auto;
    border: 1px solid #e0e3dc;
    border-radius: 1.2rem;
    background: #fff;
    box-shadow: 0 10px 24px rgba(45, 53, 46, 0.08);
  }

  .translation-list {
    display: grid;
    width: 7.25rem;
    gap: 0.5rem;
    padding: 0.75rem;
  }

  .translation-list div {
    display: grid;
    grid-template-columns: 1.25rem minmax(0, 1fr);
    gap: 0.45rem;
    align-items: center;
    color: #50564f;
    font-size: 0.62rem;
  }

  .qr-visual {
    display: grid;
    width: 6.75rem;
    height: 6.75rem;
    place-items: center;
    overflow: hidden;
    padding: 0.35rem;
    transition:
      transform 180ms ease,
      box-shadow 180ms ease;
  }

  .qr-visual:hover {
    box-shadow: 0 14px 30px rgba(45, 53, 46, 0.14);
    transform: translateY(-2px);
  }

  .qr-visual:focus-visible {
    outline: 3px solid rgba(95, 109, 96, 0.3);
    outline-offset: 3px;
  }

  .qr-visual :global(canvas),
  .qr-visual :global(svg) {
    display: block;
    max-width: 100%;
    height: auto;
  }

  .maps-visual {
    width: 8.75rem;
    overflow: hidden;
  }

  .mini-map {
    position: relative;
    min-height: 5.25rem;
    overflow: hidden;
    background-color: #e9eee6;
    background-image:
      linear-gradient(
        32deg,
        transparent 46%,
        rgba(255, 255, 255, 0.95) 47% 53%,
        transparent 54%
      ),
      linear-gradient(
        112deg,
        transparent 45%,
        rgba(255, 255, 255, 0.9) 46% 52%,
        transparent 53%
      ),
      linear-gradient(rgba(111, 133, 106, 0.12) 1px, transparent 1px),
      linear-gradient(90deg, rgba(111, 133, 106, 0.12) 1px, transparent 1px);
    background-size:
      auto,
      auto,
      1.7rem 1.7rem,
      1.7rem 1.7rem;
  }

  .maps-label {
    position: absolute;
    top: 0.45rem;
    left: 0.55rem;
    color: #5b625a;
    font-size: 0.52rem;
    font-weight: 700;
  }

  .map-pin {
    position: absolute;
    top: 50%;
    left: 54%;
    width: 1.6rem;
    color: #b95d51;
    filter: drop-shadow(0 3px 4px rgba(45, 53, 46, 0.2));
    transform: translate(-50%, -50%);
  }

  .business-preview {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.65rem;
  }

  .business-preview strong,
  .business-preview span {
    display: block;
  }

  .business-preview strong {
    color: #343934;
    font-size: 0.67rem;
  }

  .business-preview div > span {
    margin-top: 0.2rem;
    color: #ba7b34;
    font-size: 0.52rem;
    letter-spacing: 0.02em;
  }

  .menu-link {
    border-radius: 9999px;
    padding: 0.38rem 0.65rem;
    background: #e6ece2;
    color: #4c5c4e;
    font-size: 0.58rem;
    font-weight: 700;
  }

  .shared-gap {
    height: 2.5rem;
  }

  .changes-card {
    width: min(100%, 35rem);
    margin-inline: auto;
    border-radius: 1.4rem;
    padding: 1.5rem;
    text-align: center;
  }

  .changes-card h2 {
    margin-top: 0;
  }

  .contact-callout {
    width: min(100%, 42rem);
    margin-inline: auto;
    border: 1px solid rgba(255, 255, 255, 0.25);
    border-radius: 1.75rem;
    padding: 2rem 1.25rem;
    background: linear-gradient(135deg, #526856, #405445);
    color: white;
    box-shadow: 0 20px 45px rgba(45, 53, 46, 0.16);
    text-align: center;
  }

  .contact-callout h2 {
    font-family: Georgia, "Times New Roman", serif;
    font-size: 1.55rem;
    font-weight: 600;
    line-height: 1.2;
    letter-spacing: -0.025em;
  }

  .contact-callout p {
    max-width: 32rem;
    margin: 0.75rem auto 0;
    color: rgba(255, 255, 255, 0.78);
    font-size: 0.85rem;
    line-height: 1.65;
  }

  .contact-actions {
    display: flex;
    flex-direction: column;
    gap: 0.65rem;
    margin-top: 1.25rem;
  }

  .contact-actions a {
    display: inline-flex;
    min-height: 2.75rem;
    align-items: center;
    justify-content: center;
    border: 1px solid rgba(255, 255, 255, 0.45);
    border-radius: 9999px;
    padding: 0.7rem 1.1rem;
    color: white;
    font-size: 0.82rem;
    font-weight: 650;
    transition:
      background-color 180ms ease,
      transform 180ms ease;
  }

  .contact-actions a:first-child {
    border-color: white;
    background: white;
    color: #405445;
  }

  .contact-actions a:hover {
    background: rgba(255, 255, 255, 0.12);
    transform: translateY(-1px);
  }

  .contact-actions a:first-child:hover {
    background: #f2f5ef;
  }

  .contact-actions a:focus-visible {
    outline: 3px solid rgba(255, 255, 255, 0.45);
    outline-offset: 3px;
  }

  @media (max-width: 420px) {
    .maps-visual {
      width: 7.25rem;
      border-radius: 1rem;
    }

    .mini-map {
      min-height: 4rem;
    }

    .maps-label {
      top: 0.35rem;
      left: 0.4rem;
      font-size: 0.45rem;
    }

    .map-pin {
      width: 1.25rem;
    }

    .business-preview {
      gap: 0.35rem;
      padding: 0.45rem;
    }

    .business-preview strong {
      font-size: 0.55rem;
      line-height: 1.2;
    }

    .business-preview div > span {
      margin-top: 0.1rem;
      font-size: 0.42rem;
      white-space: nowrap;
    }

    .menu-link {
      padding: 0.28rem 0.42rem;
      font-size: 0.48rem;
    }
  }

  @media (min-width: 640px) {
    .fork,
    .parallel-lines {
      height: 3rem;
    }

    .branch-card {
      border-radius: 1.8rem;
      padding: 1.25rem;
    }

    .shared-card {
      justify-content: space-between;
      padding: 1.5rem 1.75rem;
    }

    .translation-list {
      width: 9.5rem;
      gap: 0.65rem;
      padding: 0.9rem;
    }

    .translation-list div {
      font-size: 0.74rem;
    }

    .qr-visual {
      width: 8rem;
      height: 8rem;
    }

    .maps-visual {
      width: 12rem;
    }

    .journey-card h2 {
      margin-top: 1rem;
      font-size: 1.25rem;
    }

    .product-copy h2,
    .support-card h2,
    .shared-card h2,
    .changes-card h2 {
      margin-top: 0;
    }

    .journey-card p {
      margin-top: 0.65rem;
      font-size: 0.9rem;
      line-height: 1.65;
    }

    .shared-gap {
      height: 3.5rem;
    }

    .changes-card {
      padding: 1.75rem 2.25rem;
    }

    .contact-callout {
      padding: 2.5rem;
    }

    .contact-callout h2 {
      font-size: 2rem;
    }

    .contact-actions {
      flex-direction: row;
      justify-content: center;
    }
  }

  @media (min-width: 768px) {
    .product-card {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(6.5rem, 7.5rem);
      gap: 1.25rem;
      align-items: center;
    }

    .product-card .phone-preview {
      width: 100%;
      margin: 0;
    }
  }
</style>
