import { ArrowRight, Check, PackageCheck } from "lucide-react";
import { Link, useLocation, useParams } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { formatPrice } from "@/lib/format";
import type { Order } from "./types";

export function OrderConfirmationPage() {
  const { id } = useParams();
  const order = (useLocation().state as { order?: Order } | null)?.order;
  return (
    <div className="page-shell grid min-h-[65vh] place-items-center py-16">
      <div className="glass relative max-w-2xl overflow-hidden rounded-[2.5rem] p-8 text-center sm:p-12">
        <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-electric-300 via-lime-300 to-violet-400" />
        <div className="mx-auto grid size-20 place-items-center rounded-full bg-lime-300/12 text-lime-300">
          <Check size={38} />
        </div>
        <p className="mt-7 text-xs font-bold uppercase tracking-[.2em] text-lime-300">
          Order confirmed
        </p>
        <h1 className="text-balance mt-3 font-display text-4xl font-bold">
          Your next big build is officially in motion.
        </h1>
        <p className="mt-4 leading-7 text-slate-400">
          We’ve reserved your hardware and created order{" "}
          <strong className="text-white">{order?.orderNumber ?? id}</strong>.
        </p>
        {order && (
          <div className="mx-auto mt-7 flex max-w-sm items-center justify-between rounded-2xl bg-white/5 p-4 text-left">
            <span className="flex items-center gap-3">
              <PackageCheck className="text-electric-300" />
              <span>
                <small className="block text-slate-500">Order total</small>
                <strong>
                  {formatPrice(order.totalAmount, order.currency)}
                </strong>
              </span>
            </span>
            <span className="text-xs text-lime-300">Confirmed</span>
          </div>
        )}
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          <Link to={`/orders/${id}`}>
            <Button>
              View order <ArrowRight size={16} />
            </Button>
          </Link>
          <Link to="/products">
            <Button variant="secondary">Continue shopping</Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
