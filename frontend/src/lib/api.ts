import { toApiUrl } from "$lib/config";

export async function apiFetch(path: string, init?: RequestInit) {
  const isFormData = init?.body instanceof FormData;

  return fetch(toApiUrl(path), {
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
