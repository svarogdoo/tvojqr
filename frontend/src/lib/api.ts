import { toApiUrl } from "$lib/config";

export async function apiFetch(path: string, init?: RequestInit) {
  return fetch(toApiUrl(path), {
    credentials: "include",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
  });
}
