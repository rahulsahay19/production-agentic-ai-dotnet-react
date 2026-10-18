export type ProductCapabilities = {
  aiReady: boolean;
  localLlmReady: boolean;
  developerReady: boolean;
  creatorReady: boolean;
  gamingReady: boolean;
  dockerReady: boolean;
};
export type TechnicalSpecifications = {
  cpu?: string;
  cpuCores?: number;
  ramGb?: number;
  maxRamGb?: number;
  gpu?: string;
  vramGb?: number;
  npu?: string;
  npuTops?: number;
  storageGb?: number;
  storageType?: string;
  display?: string;
  batteryWh?: number;
  weightKg?: number;
};
export type ProductImage = {
  url: string;
  alt: string;
  isPrimary: boolean;
  displayOrder: number;
};
export type ProductSpecification = {
  group: string;
  name: string;
  value: string;
  displayOrder: number;
};

export type ProductSummary = {
  id: string;
  sku: string;
  name: string;
  slug: string;
  brand: string;
  category: string;
  shortDescription: string;
  price: number;
  currency: string;
  stockQuantity: number;
  isFeatured: boolean;
  isAiReady: boolean;
  isLocalLlmReady: boolean;
  isDeveloperReady: boolean;
  isCreatorReady: boolean;
  isGamingReady: boolean;
  primaryImageUrl?: string;
};

export type ProductDetails = ProductSummary & {
  categoryId: string;
  description: string;
  isActive: boolean;
  technicalSpecifications: TechnicalSpecifications;
  capabilities: ProductCapabilities;
  tags: string[];
  images: ProductImage[];
  specifications: ProductSpecification[];
};

export type Category = {
  id: string;
  name: string;
  slug: string;
  description?: string;
};
export type ProductSearchResult = {
  items: ProductSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
};
export type ProductFilters = {
  keyword?: string;
  category?: string;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  ramMin?: number;
  vramMin?: number;
  aiReady?: boolean;
  localLlmReady?: boolean;
  developerReady?: boolean;
  creatorReady?: boolean;
  gamingReady?: boolean;
  page?: number;
  pageSize?: number;
};
