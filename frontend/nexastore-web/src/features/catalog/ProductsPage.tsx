import { Filter, Search, SlidersHorizontal, X } from "lucide-react";
import { useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/components/ui/PageState";
import { getApiErrorMessage } from "@/lib/http";
import { ProductCard } from "./components/ProductCard";
import { useCategories, useProductSearch } from "./queries";
import type { ProductFilters } from "./types";

const boolFilters = [
  ["aiReady", "AI ready"],
  ["localLlmReady", "Local LLM ready"],
  ["developerReady", "Developer ready"],
  ["creatorReady", "Creator ready"],
  ["gamingReady", "Gaming ready"],
] as const;
export function ProductsPage() {
  const [params, setParams] = useSearchParams();
  const [mobile, setMobile] = useState(false);
  const filters = useMemo<ProductFilters>(
    () => ({
      keyword: params.get("keyword") || undefined,
      category: params.get("category") || undefined,
      ramMin: Number(params.get("ramMin")) || undefined,
      vramMin: Number(params.get("vramMin")) || undefined,
      minPrice: Number(params.get("minPrice")) || undefined,
      maxPrice: Number(params.get("maxPrice")) || undefined,
      aiReady: params.get("aiReady") === "true" || undefined,
      localLlmReady: params.get("localLlmReady") === "true" || undefined,
      developerReady: params.get("developerReady") === "true" || undefined,
      creatorReady: params.get("creatorReady") === "true" || undefined,
      gamingReady: params.get("gamingReady") === "true" || undefined,
      page: Number(params.get("page")) || 1,
      pageSize: 12,
    }),
    [params],
  );
  const products = useProductSearch(filters);
  const categories = useCategories();
  const set = (key: string, value?: string) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    next.delete("page");
    setParams(next);
  };
  const sidebar = (
    <div className="space-y-7">
      <div>
        <label className="text-xs font-bold uppercase tracking-wider text-slate-500">
          Category
        </label>
        <div className="mt-3 grid gap-1">
          {categories.data?.map((c) => (
            <button
              key={c.id}
              onClick={() =>
                set(
                  "category",
                  filters.category === c.slug ? undefined : c.slug,
                )
              }
              className={`rounded-xl px-3 py-2.5 text-left text-sm transition ${filters.category === c.slug ? "bg-electric-300 text-ink-950 font-bold" : "text-slate-400 hover:bg-white/7 hover:text-white"}`}
            >
              {c.name}
            </button>
          ))}
        </div>
      </div>
      <div>
        <label className="text-xs font-bold uppercase tracking-wider text-slate-500">
          Minimum memory
        </label>
        <div className="mt-3 grid grid-cols-3 gap-2">
          {[16, 32, 64].map((v) => (
            <button
              key={v}
              onClick={() =>
                set("ramMin", filters.ramMin === v ? undefined : String(v))
              }
              className={`rounded-xl border py-2 text-xs font-bold ${filters.ramMin === v ? "border-electric-300 bg-electric-300/10 text-electric-300" : "border-white/10 text-slate-400"}`}
            >
              {v} GB
            </button>
          ))}
        </div>
      </div>
      <div>
        <label className="text-xs font-bold uppercase tracking-wider text-slate-500">
          Workload
        </label>
        <div className="mt-3 space-y-2">
          {boolFilters.map(([key, label]) => (
            <label
              key={key}
              className="flex cursor-pointer items-center gap-3 rounded-xl p-2 text-sm text-slate-400 hover:bg-white/5"
            >
              <input
                type="checkbox"
                checked={Boolean(filters[key])}
                onChange={(e) =>
                  set(key, e.target.checked ? "true" : undefined)
                }
                className="size-4 accent-cyan-300"
              />
              {label}
            </label>
          ))}
        </div>
      </div>
      <Button variant="ghost" className="w-full" onClick={() => setParams({})}>
        <X size={15} /> Clear filters
      </Button>
    </div>
  );
  return (
    <div className="page-shell py-10">
      <div className="flex flex-col justify-between gap-6 border-b border-white/8 pb-8 md:flex-row md:items-end">
        <div>
          <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
            The Nexa lineup
          </p>
          <h1 className="mt-2 font-display text-4xl font-bold tracking-tight sm:text-5xl">
            Find your unfair advantage.
          </h1>
          <p className="mt-3 text-slate-400">
            Current specs. Live stock. No marketing fog.
          </p>
        </div>
        <label className="glass flex w-full max-w-md items-center gap-3 rounded-2xl px-4 py-3">
          <Search size={18} className="text-slate-500" />
          <input
            defaultValue={filters.keyword}
            onKeyDown={(e) => {
              if (e.key === "Enter") set("keyword", e.currentTarget.value);
            }}
            placeholder="Search products, CPUs, workloads…"
            className="w-full bg-transparent text-sm outline-none placeholder:text-slate-600"
          />
        </label>
      </div>
      <div className="mt-8 flex gap-8">
        <aside className="hidden w-60 shrink-0 lg:block">{sidebar}</aside>
        <div className="min-w-0 flex-1">
          <div className="mb-5 flex items-center justify-between">
            <p className="text-sm text-slate-500">
              <strong className="text-white">
                {products.data?.totalCount ?? "—"}
              </strong>{" "}
              products
            </p>
            <button
              onClick={() => setMobile(true)}
              className="flex items-center gap-2 rounded-full border border-white/10 px-4 py-2 text-sm lg:hidden"
            >
              <SlidersHorizontal size={16} /> Filters
            </button>
          </div>
          {products.isLoading ? (
            <LoadingState />
          ) : products.isError ? (
            <ErrorState
              message={getApiErrorMessage(products.error)}
              onRetry={() => products.refetch()}
            />
          ) : products.data?.items.length === 0 ? (
            <EmptyState
              title="No machines found"
              copy="Try removing a filter or searching for a different workload."
            />
          ) : (
            <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
              {products.data?.items.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}
        </div>
      </div>
      {mobile && (
        <div className="fixed inset-0 z-50 bg-ink-950/90 p-4 backdrop-blur lg:hidden">
          <div className="ml-auto h-full max-w-sm overflow-y-auto rounded-3xl border border-white/10 bg-ink-900 p-6">
            <div className="mb-7 flex items-center justify-between">
              <span className="flex items-center gap-2 font-display text-xl font-bold">
                <Filter /> Filters
              </span>
              <button onClick={() => setMobile(false)}>
                <X />
              </button>
            </div>
            {sidebar}
          </div>
        </div>
      )}
    </div>
  );
}
