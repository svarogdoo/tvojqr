import { siteUrl } from "$lib/config";
import type { RequestHandler } from "./$types";

const publicPaths = ["/", "/pricing", "/contact", "/terms", "/privacy"];

export const GET: RequestHandler = () => {
  const urls = publicPaths
    .map((path) => `  <url><loc>${siteUrl}${path}</loc></url>`)
    .join("\n");
  const body = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${urls}
</urlset>
`;

  return new Response(body, {
    headers: {
      "Content-Type": "application/xml; charset=utf-8",
      "Cache-Control": "public, max-age=3600",
    },
  });
};
