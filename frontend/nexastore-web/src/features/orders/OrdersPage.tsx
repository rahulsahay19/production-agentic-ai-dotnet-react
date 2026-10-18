import { ArrowRight, Box, CalendarDays } from "lucide-react";
import { Link } from "react-router-dom";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/components/ui/PageState";
import { formatDate, formatPrice } from "@/lib/format";
import { getApiErrorMessage } from "@/lib/http";
import { useOrders } from "./queries";

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
export function OrdersPage() {
  const q = useOrders();
  if (q.isLoading) return <LoadingState />;
  if (q.isError)
    return (
      <ErrorState
        message={getApiErrorMessage(q.error)}
        onRetry={() => q.refetch()}
      />
    );
  return (
    <div className="page-shell py-10">
      <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
        Your account
      </p>
      <h1 className="mt-2 font-display text-4xl font-bold">My orders</h1>
      <p className="mt-3 text-slate-400">
        Every build, from checkout to your desk.
      </p>
      <div className="mt-9 space-y-4">
        {q.data?.length ? (
          q.data.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="glass group flex flex-col gap-5 rounded-3xl p-5 transition hover:border-electric-300/30 sm:flex-row sm:items-center"
            >
              <div className="grid size-14 place-items-center rounded-2xl bg-electric-300/10 text-electric-300">
                <Box />
              </div>
              <div className="flex-1">
                <div className="flex flex-wrap items-center gap-3">
                  <h2 className="font-display text-lg font-bold">
                    {order.orderNumber}
                  </h2>
                  <span
                    className={`rounded-full px-2.5 py-1 text-[10px] font-bold ${order.orderStatus === 6 ? "bg-rose-400/10 text-rose-300" : "bg-lime-300/10 text-lime-300"}`}
                  >
                    {status[order.orderStatus]}
                  </span>
                </div>
                <p className="mt-2 flex items-center gap-2 text-xs text-slate-500">
                  <CalendarDays size={13} />
                  {formatDate(order.orderDate)} · {order.items.length} items
                </p>
              </div>
              <div className="flex items-center justify-between gap-6 sm:block sm:text-right">
                <p className="font-display text-lg font-bold">
                  {formatPrice(order.totalAmount, order.currency)}
                </p>
                <span className="mt-1 inline-flex items-center gap-1 text-xs font-bold text-electric-300">
                  View order <ArrowRight size={13} />
                </span>
              </div>
            </Link>
          ))
        ) : (
          <EmptyState
            title="No orders yet"
            copy="When you place your first build, its journey will appear here."
          />
        )}
      </div>
    </div>
  );
}
