import { writable } from "svelte/store";

export const language = writable<"en" | "sr" | "ru" | "el">("en");

export const languages = [
  { code: "en", name: "English", flag: "🇬🇧" },
  { code: "sr", name: "Srpski", flag: "🇷🇸" },
  { code: "ru", name: "Русский", flag: "🇷🇺" },
  { code: "el", name: "Ελληνικά", flag: "🇬🇷" },
];
