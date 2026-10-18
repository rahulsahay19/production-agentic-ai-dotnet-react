import {
  Check,
  ChevronRight,
  Cpu,
  Database,
  Gauge,
  MemoryStick,
  ShoppingBag,
  Sparkles,
  Zap,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { createElement } from "react";
import { Button } from "@/components/ui/Button";
import { ErrorState, LoadingState } from "@/components/ui/PageState";
import { StockBadge } from "@/components/ui/StockBadge";
import { useCartMutations } from "@/features/cart/queries";
import { formatPrice } from "@/lib/format";
import { getApiErrorMessage } from "@/lib/http";
import { ProductVisual } from "./components/ProductVisual";
import { useProduct } from "./queries";

export function ProductDetailsPage() {
  const { id = "" } = useParams();
  const q = useProduct(id);
  const { add } = useCartMutations();
  if (q.isLoading) return <LoadingState label="Loading the machine…" />;
  if (q.isError || !q.data)
    return (
      <ErrorState
        message={getApiErrorMessage(q.error)}
        onRetry={() => q.refetch()}
      />
    );
  const p = q.data;
  const specs: Array<[string, string | undefined, LucideIcon]> = [
    ["Processor", p.technicalSpecifications.cpu, Cpu],
    [
      "Memory",
      p.technicalSpecifications.ramGb
        ? `${p.technicalSpecifications.ramGb} GB RAM`
        : undefined,
      MemoryStick,
    ],
    ["Graphics", p.technicalSpecifications.gpu, Gauge],
    [
      "Storage",
      p.technicalSpecifications.storageGb
        ? `${p.technicalSpecifications.storageGb / 1024} TB ${p.technicalSpecifications.storageType ?? ""}`
        : undefined,
      Database,
    ],
  ];
  return (
    <div className="page-shell py-8">
      <nav className="mb-8 flex items-center gap-2 text-xs text-slate-500">
        <Link to="/products" className="hover:text-white">
          Products
        </Link>
        <ChevronRight size={13} />
        <span>{p.category}</span>
        <ChevronRight size={13} />
        <span className="text-slate-300">{p.name}</span>
      </nav>
      <div className="grid gap-10 lg:grid-cols-[1.05fr_.95fr]">
        <div>
          <ProductVisual
            name={p.name}
            category={p.category}
            className="min-h-[480px] rounded-[2rem]"
          />
          <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {specs.filter(([, value]) => value).map(([label, value, Icon]) => (
              <div key={String(label)} className="glass rounded-2xl p-4">
                {createElement(Icon, {
                  size: 18,
                  className: "text-electric-300",
                })}
                <p className="mt-5 text-[10px] font-bold uppercase tracking-wider text-slate-600">
                  {String(label)}
                </p>
                <p className="mt-1 text-sm font-semibold text-slate-200">
                  {String(value)}
                </p>
              </div>
            ))}
          </div>
        </div>
        <section className="lg:pt-4">
          <div className="flex items-center gap-3">
            <span className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
              {p.category}
            </span>
            {p.isFeatured && (
              <span className="flex items-center gap-1 rounded-full bg-violet-400/10 px-2.5 py-1 text-[10px] font-bold text-violet-300">
                <Sparkles size={11} /> Featured
              </span>
            )}
          </div>
          <h1 className="text-balance mt-4 font-display text-4xl font-bold leading-tight tracking-[-.04em] sm:text-5xl">
            {p.name}
          </h1>
          <p className="mt-5 text-lg leading-8 text-slate-400">
            {p.description}
          </p>
          <div className="mt-8 flex items-end justify-between border-y border-white/8 py-6">
            <div>
              <p className="text-xs text-slate-500">Inclusive of taxes</p>
              <p className="mt-1 font-display text-3xl font-bold">
                {formatPrice(p.price, p.currency)}
              </p>
            </div>
            <StockBadge quantity={p.stockQuantity} />
          </div>
          <div className="mt-6 grid grid-cols-2 gap-2">
            {p.tags.slice(0, 6).map((tag) => (
              <span
                key={tag}
                className="flex items-center gap-2 rounded-xl bg-white/5 px-3 py-2.5 text-xs font-semibold text-slate-300"
              >
                <Check size={13} className="text-lime-300" />
                {tag}
              </span>
            ))}
          </div>
          <Button
            size="lg"
            className="mt-8 w-full"
            disabled={!p.stockQuantity || add.isPending}
            onClick={() => add.mutate({ productId: p.id })}
          >
            <ShoppingBag size={19} />
            {add.isPending ? "Adding…" : "Add to cart"}
          </Button>
          {add.isSuccess && (
            <p className="mt-3 text-center text-sm font-semibold text-lime-300">
              Added to your build.
            </p>
          )}
          <div className="mt-8 rounded-2xl border border-electric-300/15 bg-electric-300/6 p-5">
            <div className="flex gap-3">
              <Zap className="shrink-0 text-electric-300" />
              <div>
                <p className="font-bold">Nexa performance promise</p>
                <p className="mt-1 text-sm leading-6 text-slate-400">
                  Every configuration is checked for thermal headroom, upgrade
                  path and workload fit.
                </p>
              </div>
            </div>
          </div>
        </section>
      </div>
      <section className="mt-20 border-t border-white/8 pt-12">
        <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
          Under the hood
        </p>
        <h2 className="mt-2 font-display text-3xl font-bold">
          Technical specifications
        </h2>
        <div className="mt-7 grid gap-3 md:grid-cols-2">
          {Object.entries(p.technicalSpecifications)
            .filter(([, v]) => v !== null && v !== undefined)
            .map(([key, value]) => (
              <div
                key={key}
                className="flex items-center justify-between rounded-xl border border-white/7 px-4 py-3"
              >
                <span className="text-sm capitalize text-slate-500">
                  {key.replace(/([A-Z])/g, " $1")}
                </span>
                <span className="text-sm font-semibold text-slate-200">
                  {String(value)}
                </span>
              </div>
            ))}
        </div>
      </section>
    </div>
  );
}
