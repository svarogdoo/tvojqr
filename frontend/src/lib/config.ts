export const apiBaseUrl = import.meta.env.PUBLIC_API_BASE_URL || "http://localhost:5115";

export function toApiUrl(path: string) {
  return `${apiBaseUrl}${path.startsWith("/") ? path : `/${path}`}`;
}
