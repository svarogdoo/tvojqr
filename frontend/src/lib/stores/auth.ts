import { apiFetch } from "$lib/api";
import { toApiUrl } from "$lib/config";
import { writable } from "svelte/store";

export type AuthUser = {
  id: string;
  email: string;
  displayName: string;
};

export type AuthState =
  | { status: "loading"; user: null }
  | { status: "anonymous"; user: null }
  | { status: "authenticated"; user: AuthUser };

export const auth = writable<AuthState>({ status: "loading", user: null });
export const authNavigationStartedEvent = "hostingqr:auth-navigation-started";

export async function refreshSession() {
  auth.set({ status: "loading", user: null });

  try {
    const response = await apiFetch("/api/auth/me", { method: "GET" });
    if (response.status === 401) {
      auth.set({ status: "anonymous", user: null });
      return;
    }

    if (!response.ok) {
      throw new Error(`Auth request failed with status ${response.status}`);
    }

    const user = (await response.json()) as AuthUser;
    auth.set({ status: "authenticated", user });
  } catch {
    auth.set({ status: "anonymous", user: null });
  }
}

export function startGoogleSignIn() {
  window.dispatchEvent(new CustomEvent(authNavigationStartedEvent));
  window.location.href = toApiUrl("/api/auth/google");
}

export async function signOut() {
  await apiFetch("/api/auth/sign-out", { method: "POST" });
  auth.set({ status: "anonymous", user: null });
}
