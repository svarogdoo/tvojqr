export type ProjectListItem = {
  id: string;
  name: string;
  slug: string;
  status: "active" | "disabled";
  updatedAt: string;
  viewCount: number;
  lastViewedAt: string | null;
};

export type ProjectDetail = {
  id: string;
  name: string;
  slug: string;
  status: "active" | "disabled";
  backgroundColor: string;
  createdAt: string;
  updatedAt: string;
  viewCount: number;
  lastViewedAt: string | null;
  languages: ProjectLanguageVariant[];
  assets: Asset[];
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

export type Entitlement = {
  tier: "none" | "admin" | "free" | "standard" | "plus";
  isActive: boolean;
  grantedManually: boolean;
  endsAt: string | null;
  hasToolAccess: boolean;
};
