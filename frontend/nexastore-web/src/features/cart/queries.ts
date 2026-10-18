import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cartApi } from "./api";

export const cartKey = ["cart"] as const;
export const useCart = () =>
  useQuery({ queryKey: cartKey, queryFn: cartApi.get });
export function useCartMutations() {
  const client = useQueryClient();
  const sync = (data: unknown) => {
    client.setQueryData(cartKey, data);
  };
  const add = useMutation({
    mutationFn: ({
      productId,
      quantity = 1,
    }: {
      productId: string;
      quantity?: number;
    }) => cartApi.add(productId, quantity),
    onSuccess: sync,
  });
  const update = useMutation({
    mutationFn: ({
      productId,
      quantity,
    }: {
      productId: string;
      quantity: number;
    }) => cartApi.update(productId, quantity),
    onSuccess: sync,
  });
  const remove = useMutation({
    mutationFn: cartApi.remove,
    onSuccess: () => client.invalidateQueries({ queryKey: cartKey }),
  });
  const clear = useMutation({
    mutationFn: cartApi.clear,
    onSuccess: () => client.invalidateQueries({ queryKey: cartKey }),
  });
  return { add, update, remove, clear };
}
