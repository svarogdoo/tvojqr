import { env } from "$env/dynamic/public";
import { dev } from "$app/environment";

export const apiBaseUrl = env.PUBLIC_API_BASE_URL || (dev ? "http://localhost:5115" : undefined);
export const siteUrl = (env.PUBLIC_SITE_URL || "https://hostingqr.com").replace(/\/$/, "");
export const cloudflareWebAnalyticsToken =
  env.PUBLIC_CLOUDFLARE_WEB_ANALYTICS_TOKEN?.trim() || "0f99e401c95b4c4ca243a9d376c0f902";

export function toApiUrl(path: string) {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  if (!apiBaseUrl) {
    throw new Error("PUBLIC_API_BASE_URL is not configured.");
  }

  return `${apiBaseUrl}${path.startsWith("/") ? path : `/${path}`}`;
}
