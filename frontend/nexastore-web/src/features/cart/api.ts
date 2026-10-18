import { env } from "@/config/env";
import { http } from "@/lib/http";
import type { Cart } from "./types";

const root = `/api/cart/${env.customerId}`;
export const cartApi = {
  get: async () => (await http.get<Cart>(root)).data,
  add: async (productId: string, quantity = 1) =>
    (await http.post<Cart>(`${root}/items`, { productId, quantity })).data,
  update: async (productId: string, quantity: number) =>
    (await http.put<Cart>(`${root}/items/${productId}`, { quantity })).data,
  remove: async (productId: string) => {
    await http.delete(`${root}/items/${productId}`);
  },
  clear: async () => {
    await http.delete(root);
  },
};
