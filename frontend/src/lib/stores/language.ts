import { writable } from "svelte/store";

export type LanguageCode = "en" | "es";

export const language = writable<LanguageCode>("en");

export const languages = [
  { code: "en", name: "English", flag: "🇬🇧" },
  { code: "es", name: "Spanish", flag: "🇪🇸" },
];
