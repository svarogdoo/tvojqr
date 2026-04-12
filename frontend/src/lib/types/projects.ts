export type ProjectListItem = {
  id: string;
  name: string;
  slug: string;
  updatedAt: string;
};

export type ProjectDetail = {
  id: string;
  name: string;
  slug: string;
  createdAt: string;
  updatedAt: string;
};

export type UpdateProjectRequest = {
  name: string;
  slug: string;
};

export type SlugAvailabilityResponse = {
  slug: string;
  isAvailable: boolean;
};

export type GeneratedSlugResponse = {
  slug: string;
};
