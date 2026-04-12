import { env } from "$env/dynamic/public";

export const apiBaseUrl = env.PUBLIC_API_BASE_URL;

export function toApiUrl(path: string) {
  return `${apiBaseUrl}${path.startsWith("/") ? path : `/${path}`}`;
}
