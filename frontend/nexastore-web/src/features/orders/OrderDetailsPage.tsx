import {
  ArrowLeft,
  CheckCircle2,
  MapPin,
  PackageCheck,
  RotateCcw,
} from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { ErrorState, LoadingState } from "@/components/ui/PageState";
import { formatDate, formatPrice } from "@/lib/format";
import { getApiErrorMessage } from "@/lib/http";
import { useOrder, useOrderMutations } from "./queries";

const status = [
  "",
  "Pending",
  "Confirmed",
  "Processing",
  "Shipped",
  "Delivered",
  "Cancelled",
  "Returned",
];
export function OrderDetailsPage() {
  const { id = "" } = useParams();
  const q = useOrder(id);
  const { cancel } = useOrderMutations();
  if (q.isLoading) return <LoadingState />;
  if (q.isError || !q.data)
    return <ErrorState message={getApiErrorMessage(q.error)} />;
  const o = q.data;
  return (
    <div className="page-shell py-10">
      <Link
        to="/orders"
        className="inline-flex items-center gap-2 text-sm text-slate-400 hover:text-white"
      >
        <ArrowLeft size={16} /> My orders
      </Link>
      <div className="mt-7 flex flex-col justify-between gap-5 border-b border-white/8 pb-8 sm:flex-row sm:items-end">
        <div>
          <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
            Order details
          </p>
          <h1 className="mt-2 font-display text-3xl font-bold">
            {o.orderNumber}
          </h1>
          <p className="mt-2 text-sm text-slate-500">
            Placed {formatDate(o.orderDate)}
          </p>
        </div>
        <span className="inline-flex w-fit items-center gap-2 rounded-full bg-lime-300/10 px-4 py-2 text-xs font-bold text-lime-300">
          <PackageCheck size={15} />
          {status[o.orderStatus]}
        </span>
      </div>
      <div className="mt-8 grid gap-8 lg:grid-cols-[1fr_360px]">
        <section className="space-y-3">
          {o.items.map((i) => (
            <div
              key={i.productId}
              className="glass flex items-center justify-between rounded-2xl p-5"
            >
              <div>
                <Link
                  to={`/products/${i.productId}`}
                  className="font-display font-bold hover:text-electric-300"
                >
                  {i.productName}
                </Link>
                <p className="mt-1 text-xs text-slate-500">
                  {i.sku} · Qty {i.quantity}
                </p>
              </div>
              <span className="font-bold">
                {formatPrice(i.lineTotal, o.currency)}
              </span>
            </div>
          ))}
          <div className="glass mt-5 rounded-2xl p-5">
            <div className="flex gap-3">
              <MapPin className="text-electric-300" />
              <div>
                <p className="font-bold">Delivery address</p>
                <p className="mt-2 text-sm leading-6 text-slate-400">
                  {o.shippingAddress.recipientName}
                  <br />
                  {o.shippingAddress.line1}, {o.shippingAddress.line2}
                  <br />
                  {o.shippingAddress.city}, {o.shippingAddress.stateOrProvince}{" "}
                  {o.shippingAddress.postalCode}
                </p>
              </div>
            </div>
          </div>
        </section>
        <aside className="glass h-fit rounded-3xl p-6">
          <h2 className="font-display text-xl font-bold">Payment summary</h2>
          <div className="mt-5 space-y-3 text-sm text-slate-400">
            <div className="flex justify-between">
              <span>Subtotal</span>
              <span className="text-white">
                {formatPrice(o.subtotal, o.currency)}
              </span>
            </div>
            <div className="flex justify-between">
              <span>Shipping</span>
              <span>{formatPrice(o.shippingAmount, o.currency)}</span>
            </div>
            <div className="flex justify-between">
              <span>Tax</span>
              <span>{formatPrice(o.taxAmount, o.currency)}</span>
            </div>
          </div>
          <div className="my-5 border-t border-white/10" />
          <div className="flex justify-between font-bold">
            <span>Total</span>
            <span className="font-display text-xl">
              {formatPrice(o.totalAmount, o.currency)}
            </span>
          </div>
          {[1, 2].includes(o.orderStatus) && (
            <Button
              variant="secondary"
              className="mt-6 w-full"
              disabled={cancel.isPending}
              onClick={() => cancel.mutate(o.id)}
            >
              <RotateCcw size={16} />
              {cancel.isPending ? "Cancelling…" : "Cancel order"}
            </Button>
          )}
          {o.orderStatus === 6 && (
            <p className="mt-5 flex items-center gap-2 text-xs text-slate-400">
              <CheckCircle2 size={14} className="text-lime-300" /> Inventory has
              been restored.
            </p>
          )}
        </aside>
      </div>
    </div>
  );
}
