import { ArrowLeft, ArrowRight, LockKeyhole } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { ErrorState, LoadingState } from "@/components/ui/PageState";
import { useCart } from "@/features/cart/queries";
import { formatPrice } from "@/lib/format";
import { getApiErrorMessage } from "@/lib/http";
import { useOrderMutations } from "./queries";
import type { Address } from "./types";

const initial: Address = {
  recipientName: "Aarav Sharma",
  line1: "101 Developer Avenue",
  line2: "Tech Park",
  city: "Bengaluru",
  stateOrProvince: "Karnataka",
  postalCode: "560001",
  countryCode: "IN",
  phone: "+91-9000000000",
};
export function CheckoutPage() {
  const cart = useCart();
  const { create } = useOrderMutations();
  const nav = useNavigate();
  const [address, setAddress] = useState(initial);
  if (cart.isLoading) return <LoadingState />;
  if (cart.isError)
    return <ErrorState message={getApiErrorMessage(cart.error)} />;
  if (!cart.data?.items.length)
    return (
      <div className="page-shell py-20">
        <ErrorState message="Your cart is empty. Add a machine before checkout." />
      </div>
    );
  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    create.mutate(
      { shipping: address, billing: { ...address } },
      {
        onSuccess: (o) =>
          nav(`/orders/${o.id}/confirmed`, { state: { order: o } }),
      },
    );
  };
  return (
    <div className="page-shell py-10">
      <Link
        to="/cart"
        className="inline-flex items-center gap-2 text-sm text-slate-400 hover:text-white"
      >
        <ArrowLeft size={16} /> Back to cart
      </Link>
      <div className="mt-7 grid gap-8 lg:grid-cols-[1fr_390px]">
        <form onSubmit={submit} className="glass rounded-[2rem] p-6 sm:p-8">
          <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
            Checkout
          </p>
          <h1 className="mt-2 font-display text-3xl font-bold">
            Where should we send it?
          </h1>
          <div className="mt-8 grid gap-5 sm:grid-cols-2">
            {(
              [
                ["recipientName", "Full name"],
                ["phone", "Phone"],
                ["line1", "Address line 1"],
                ["line2", "Address line 2"],
                ["city", "City"],
                ["stateOrProvince", "State"],
                ["postalCode", "Postal code"],
                ["countryCode", "Country code"],
              ] as const
            ).map(([key, label]) => (
              <label
                key={key}
                className={
                  key === "line1" || key === "line2" ? "sm:col-span-2" : ""
                }
              >
                <span className="mb-2 block text-xs font-bold text-slate-400">
                  {label}
                </span>
                <input
                  required={key !== "line2"}
                  value={address[key] ?? ""}
                  onChange={(e) =>
                    setAddress({ ...address, [key]: e.target.value })
                  }
                  className="w-full rounded-xl border border-white/10 bg-white/5 px-4 py-3 text-sm outline-none transition focus:border-electric-300/60"
                />
              </label>
            ))}
          </div>
          {create.isError && (
            <p className="mt-5 rounded-xl bg-rose-400/10 p-3 text-sm text-rose-200">
              {getApiErrorMessage(create.error)}
            </p>
          )}
          <Button
            type="submit"
            size="lg"
            className="mt-8 w-full sm:w-auto"
            disabled={create.isPending}
          >
            {create.isPending ? "Placing order…" : "Place order"}{" "}
            <ArrowRight size={17} />
          </Button>
        </form>
        <aside className="glass h-fit rounded-3xl p-6">
          <h2 className="font-display text-xl font-bold">Your build</h2>
          <div className="mt-5 space-y-4">
            {cart.data.items.map((i) => (
              <div
                key={i.productId}
                className="flex justify-between gap-4 text-sm"
              >
                <div>
                  <p className="font-semibold">{i.productName}</p>
                  <p className="text-xs text-slate-500">Qty {i.quantity}</p>
                </div>
                <span>{formatPrice(i.lineTotal, i.currency)}</span>
              </div>
            ))}
          </div>
          <div className="my-5 border-t border-white/10" />
          <div className="flex justify-between font-bold">
            <span>Subtotal</span>
            <span>{formatPrice(cart.data.subtotal, cart.data.currency)}</span>
          </div>
          <p className="mt-5 flex items-center gap-2 text-xs text-slate-500">
            <LockKeyhole size={14} className="text-lime-300" /> Business rules
            and stock rechecked before placement.
          </p>
        </aside>
      </div>
    </div>
  );
}
