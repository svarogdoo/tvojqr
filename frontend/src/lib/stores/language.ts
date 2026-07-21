import { writable } from "svelte/store";

export type LanguageCode = "en" | "it" | "es" | "hr";

export const language = writable<LanguageCode>("en");

export const languages = [
  { code: "en", name: "English", flag: "🇬🇧" },
  { code: "it", name: "Italian", flag: "🇮🇹" },
  { code: "es", name: "Spanish", flag: "🇪🇸" },
  { code: "hr", name: "Croatian", flag: "🇭🇷" },
];
