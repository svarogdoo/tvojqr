import type { MenuType } from "$lib/types/projects";

export type AdminClient = {
  userId: string;
  email: string;
  displayName: string;
  joinedAt: string;
  menuCount: number;
  menuTypes: Partial<Record<MenuType, number>>;
  billingCycle: "monthly" | "annual" | null;
  billingActive: boolean | null;
  lastInvoiceDate: string | null;
  nextInvoiceDate: string | null;
};

export type AdminMenu = {
  projectId: string;
  name: string;
  slug: string;
  status: "active" | "disabled";
  menuType: MenuType;
  updatedAt: string;
  clientUserId: string;
  clientEmail: string;
  clientDisplayName: string;
};

export type Invoice = {
  id: string;
  clientUserId: string;
  invoiceDate: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  createdAt: string;
};

export type AccountSummary = {
  joinedAt: string;
  menuCount: number;
  menuTypes: Partial<Record<MenuType, number>>;
  billingCycle: "monthly" | "annual" | null;
  billingActive: boolean | null;
  lastInvoiceDate: string | null;
  nextInvoiceDate: string | null;
};
