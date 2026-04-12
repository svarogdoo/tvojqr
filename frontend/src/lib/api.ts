import { toApiUrl } from "$lib/config";

let hasLoggedApiBaseUrl = false;

export async function apiFetch(path: string, init?: RequestInit) {
  const isFormData = init?.body instanceof FormData;
  const url = toApiUrl(path);

  if (!hasLoggedApiBaseUrl && typeof window !== "undefined") {
    hasLoggedApiBaseUrl = true;
    console.info("[HostingQr] PUBLIC_API_BASE_URL", {
      url,
    });
  }

  return fetch(url, {
    credentials: "include",
    ...init,
    headers: isFormData
      ? init?.headers
      : {
          "Content-Type": "application/json",
          ...(init?.headers ?? {}),
        },
  });
}
