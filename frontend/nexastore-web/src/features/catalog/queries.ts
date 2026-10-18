import { useQuery } from "@tanstack/react-query";
import { catalogApi } from "./api";
import type { ProductFilters } from "./types";

export const catalogKeys = {
  all: ["catalog"] as const,
  featured: () => ["catalog", "featured"] as const,
  search: (filters: ProductFilters) => ["catalog", "search", filters] as const,
  product: (id: string) => ["catalog", "product", id] as const,
  categories: () => ["catalog", "categories"] as const,
};
export const useFeaturedProducts = () =>
  useQuery({
    queryKey: catalogKeys.featured(),
    queryFn: catalogApi.featured,
    staleTime: 60_000,
  });
export const useProductSearch = (filters: ProductFilters) =>
  useQuery({
    queryKey: catalogKeys.search(filters),
    queryFn: () => catalogApi.search(filters),
    placeholderData: (previous) => previous,
  });
export const useProduct = (id: string) =>
  useQuery({
    queryKey: catalogKeys.product(id),
    queryFn: () => catalogApi.product(id),
    enabled: Boolean(id),
  });
export const useCategories = () =>
  useQuery({
    queryKey: catalogKeys.categories(),
    queryFn: catalogApi.categories,
    staleTime: 5 * 60_000,
  });
