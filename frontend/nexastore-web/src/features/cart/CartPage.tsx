import {
  ArrowRight,
  Minus,
  Plus,
  ShieldCheck,
  ShoppingBag,
  Trash2,
} from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/components/ui/PageState";
import { formatPrice } from "@/lib/format";
import { getApiErrorMessage } from "@/lib/http";
import { useCart, useCartMutations } from "./queries";

export function CartPage() {
  const q = useCart();
  const m = useCartMutations();
  if (q.isLoading) return <LoadingState label="Loading your build…" />;
  if (q.isError)
    return (
      <ErrorState
        message={getApiErrorMessage(q.error)}
        onRetry={() => q.refetch()}
      />
    );
  const cart = q.data;
  if (!cart || cart.items.length === 0)
    return (
      <div className="page-shell py-20">
        <EmptyState
          title="Your build starts here"
          copy="Explore machines engineered for your next ambitious project."
        />
        <div className="mt-6 text-center">
          <Link to="/products">
            <Button>
              Explore products <ArrowRight size={16} />
            </Button>
          </Link>
        </div>
      </div>
    );
  return (
    <div className="page-shell py-10">
      <div className="mb-9">
        <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
          Your selection
        </p>
        <h1 className="mt-2 font-display text-4xl font-bold">
          Cart <span className="text-slate-600">({cart.items.length})</span>
        </h1>
      </div>
      <div className="grid gap-8 lg:grid-cols-[1fr_390px]">
        <section className="space-y-3">
          {cart.items.map((item) => (
            <article
              key={item.productId}
              className="glass flex flex-col gap-5 rounded-3xl p-5 sm:flex-row sm:items-center"
            >
              <div className="grid size-24 shrink-0 place-items-center rounded-2xl bg-gradient-to-br from-electric-300/20 to-violet-400/10">
                <ShoppingBag className="text-electric-300" />
              </div>
              <div className="min-w-0 flex-1">
                <Link
                  to={`/products/${item.productId}`}
                  className="font-display text-lg font-bold hover:text-electric-300"
                >
                  {item.productName}
                </Link>
                <p className="mt-1 text-xs text-slate-500">SKU {item.sku}</p>
                <p className="mt-3 font-bold">
                  {formatPrice(item.unitPrice, item.currency)}
                </p>
              </div>
              <div className="flex items-center justify-between gap-4 sm:flex-col sm:items-end">
                <div className="flex items-center rounded-full border border-white/10">
                  <button
                    disabled={item.quantity <= 1 || m.update.isPending}
                    onClick={() =>
                      m.update.mutate({
                        productId: item.productId,
                        quantity: item.quantity - 1,
                      })
                    }
                    className="p-2.5 text-slate-400 hover:text-white"
                  >
                    <Minus size={14} />
                  </button>
                  <span className="min-w-8 text-center text-sm font-bold">
                    {item.quantity}
                  </span>
                  <button
                    disabled={m.update.isPending}
                    onClick={() =>
                      m.update.mutate({
                        productId: item.productId,
                        quantity: item.quantity + 1,
                      })
                    }
                    className="p-2.5 text-slate-400 hover:text-white"
                  >
                    <Plus size={14} />
                  </button>
                </div>
                <button
                  onClick={() => m.remove.mutate(item.productId)}
                  className="flex items-center gap-1.5 text-xs font-semibold text-slate-500 hover:text-rose-300"
                >
                  <Trash2 size={14} /> Remove
                </button>
              </div>
            </article>
          ))}
        </section>
        <aside className="glass h-fit rounded-3xl p-6 lg:sticky lg:top-24">
          <h2 className="font-display text-xl font-bold">Order summary</h2>
          <div className="mt-6 space-y-3 text-sm">
            <div className="flex justify-between text-slate-400">
              <span>Subtotal</span>
              <span className="text-white">
                {formatPrice(cart.subtotal, cart.currency)}
              </span>
            </div>
            <div className="flex justify-between text-slate-400">
              <span>Shipping</span>
              <span className="text-lime-300">Calculated at checkout</span>
            </div>
            <div className="flex justify-between text-slate-400">
              <span>Taxes</span>
              <span>Calculated at checkout</span>
            </div>
          </div>
          <div className="my-5 border-t border-white/10" />
          <div className="flex items-end justify-between">
            <span className="font-bold">Estimated total</span>
            <span className="font-display text-2xl font-bold">
              {formatPrice(cart.subtotal, cart.currency)}
            </span>
          </div>
          <Link to="/checkout" className="mt-6 block">
            <Button size="lg" className="w-full">
              Continue to checkout <ArrowRight size={17} />
            </Button>
          </Link>
          <p className="mt-5 flex items-center justify-center gap-2 text-xs text-slate-500">
            <ShieldCheck size={14} className="text-lime-300" /> Secure checkout
            · Inventory verified
          </p>
        </aside>
      </div>
    </div>
  );
}
