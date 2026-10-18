import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ordersApi } from "./api";
import { cartKey } from "@/features/cart/queries";
import type { Address } from "./types";

export const orderKeys = {
  all: ["orders"] as const,
  detail: (id: string) => ["orders", id] as const,
};
export const useOrders = () =>
  useQuery({ queryKey: orderKeys.all, queryFn: ordersApi.list });
export const useOrder = (id: string) =>
  useQuery({
    queryKey: orderKeys.detail(id),
    queryFn: () => ordersApi.get(id),
    enabled: Boolean(id),
  });
export function useOrderMutations() {
  const client = useQueryClient();
  const create = useMutation({
    mutationFn: ({
      shipping,
      billing,
    }: {
      shipping: Address;
      billing: Address;
    }) => ordersApi.create(shipping, billing),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: orderKeys.all });
      client.invalidateQueries({ queryKey: cartKey });
    },
  });
  const cancel = useMutation({
    mutationFn: ordersApi.cancel,
    onSuccess: (order) => {
      client.setQueryData(orderKeys.detail(order.id), order);
      client.invalidateQueries({ queryKey: orderKeys.all });
    },
  });
  return { create, cancel };
}
