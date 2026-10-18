import { http } from "@/lib/http";
import type {
  Category,
  ProductDetails,
  ProductFilters,
  ProductSearchResult,
} from "./types";

export const catalogApi = {
  search: async (filters: ProductFilters = {}) =>
    (
      await http.get<ProductSearchResult>("/api/products/search", {
        params: filters,
      })
    ).data,
  featured: async () =>
    (
      await http.get<ProductSearchResult>("/api/products", {
        params: { pageSize: 8 },
      })
    ).data,
  product: async (id: string) =>
    (await http.get<ProductDetails>(`/api/products/${id}`)).data,
  categories: async () => (await http.get<Category[]>("/api/categories")).data,
};
