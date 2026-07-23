// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace NodeJS {
		interface ProcessEnv {
			PUBLIC_API_BASE_URL?: string;
			PUBLIC_SITE_URL?: string;
			PUBLIC_CLOUDFLARE_WEB_ANALYTICS_TOKEN?: string;
		}
	}

	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}
}

export {};
