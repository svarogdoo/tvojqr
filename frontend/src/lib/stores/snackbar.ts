import { writable } from "svelte/store";

export type SnackbarTone = "success" | "error" | "info";

export type SnackbarState = {
  visible: boolean;
  message: string;
  tone: SnackbarTone;
};

const initialState: SnackbarState = {
  visible: false,
  message: "",
  tone: "info",
};

export const snackbar = writable<SnackbarState>(initialState);

let timeoutHandle: ReturnType<typeof setTimeout> | null = null;

export function showSnackbar(message: string, tone: SnackbarTone = "info", duration = 3200) {
  if (timeoutHandle) {
    clearTimeout(timeoutHandle);
  }

  snackbar.set({
    visible: true,
    message,
    tone,
  });

  timeoutHandle = setTimeout(() => {
    snackbar.set(initialState);
    timeoutHandle = null;
  }, duration);
}

export function hideSnackbar() {
  if (timeoutHandle) {
    clearTimeout(timeoutHandle);
    timeoutHandle = null;
  }

  snackbar.set(initialState);
}
