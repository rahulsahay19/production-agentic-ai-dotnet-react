import { env } from "@/config/env";
import { http } from "@/lib/http";
import type { Address, Order } from "./types";

export const ordersApi = {
  create: async (shippingAddress: Address, billingAddress: Address) =>
    (
      await http.post<Order>("/api/orders", {
        customerId: env.customerId,
        shippingAddress,
        billingAddress,
      })
    ).data,
  list: async () =>
    (await http.get<Order[]>(`/api/customers/${env.customerId}/orders`)).data,
  get: async (id: string) => (await http.get<Order>(`/api/orders/${id}`)).data,
  cancel: async (id: string) =>
    (await http.post<Order>(`/api/orders/${id}/cancel`)).data,
};
