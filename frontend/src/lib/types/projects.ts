export type MenuType = "image" | "digital";

export type ProjectListItem = {
  id: string;
  name: string;
  slug: string;
  status: "active" | "disabled";
  menuType: MenuType;
  updatedAt: string;
  viewCount: number;
  lastViewedAt: string | null;
};

export type ProjectDetail = {
  id: string;
  name: string;
  slug: string;
  status: "active" | "disabled";
  menuType: MenuType;
  timeZone: string;
  backgroundColor: string;
  createdAt: string;
  updatedAt: string;
  viewCount: number;
  lastViewedAt: string | null;
  languages: ProjectLanguageVariant[];
  assets: Asset[];
  coverImage: Asset | null;
};

export type ProjectLanguageVariant = {
  id: string;
  languageCode: string;
  displayName: string;
  isDefault: boolean;
  sortOrder: number;
};

export type Asset = {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
  languageCode: string;
  sortOrder: number;
  createdAt: string;
};

export type UpdateProjectRequest = {
  name: string;
  slug: string;
  backgroundColor: string;
};

export type CreateProjectRequest = UpdateProjectRequest & {
  defaultLanguageCode: string;
  defaultLanguageDisplayName: string;
  menuType: MenuType;
};

export type UpdateProjectStatusRequest = {
  status: "active" | "disabled";
};

export type SlugAvailabilityResponse = {
  slug: string;
  isAvailable: boolean;
};

export type GeneratedSlugResponse = {
  slug: string;
};

export type DigitalMenu = {
  timeZone: string;
  timeZoneConfigured: boolean;
  currencyCode: string;
  categories: DigitalMenuCategory[];
};

export type DigitalMenuCategory = {
  id: string;
  sortOrder: number;
  translations: DigitalMenuCategoryTranslation[];
  schedules: DigitalMenuSchedule[];
  items: DigitalMenuItem[];
};

export type DigitalMenuCategoryTranslation = {
  languageCode: string;
  name: string;
};

export type DigitalMenuSchedule = {
  dayOfWeek: number;
  startsAt: string;
  endsAt: string;
};

export type DigitalMenuItem = {
  id: string;
  priceText: string;
  isOutOfStock: boolean;
  sortOrder: number;
  translations: DigitalMenuItemTranslation[];
};

export type DigitalMenuItemTranslation = {
  languageCode: string;
  name: string;
  description: string;
};

export type Entitlement = {
  tier: "none" | "admin" | "free" | "standard" | "plus";
  isActive: boolean;
  grantedManually: boolean;
  endsAt: string | null;
  hasToolAccess: boolean;
};
