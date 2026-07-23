import { env } from "$env/dynamic/public";
import { dev } from "$app/environment";

export const apiBaseUrl = env.PUBLIC_API_BASE_URL || (dev ? "http://localhost:5115" : undefined);
export const siteUrl = (env.PUBLIC_SITE_URL || "https://hostingqr.com").replace(/\/$/, "");
export const gaMeasurementId = env.PUBLIC_GA_MEASUREMENT_ID?.trim() || "";

export function toApiUrl(path: string) {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  if (!apiBaseUrl) {
    throw new Error("PUBLIC_API_BASE_URL is not configured.");
  }

  return `${apiBaseUrl}${path.startsWith("/") ? path : `/${path}`}`;
}
