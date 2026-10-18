import { ArrowUpRight, Sparkles } from "lucide-react";
import { Link } from "react-router-dom";
import { formatPrice } from "@/lib/format";
import { ProductVisual } from "./ProductVisual";
import { StockBadge } from "@/components/ui/StockBadge";
import type { ProductSummary } from "../types";

export function ProductCard({ product }: { product: ProductSummary }) {
  return (
    <article className="group glass flex h-full flex-col overflow-hidden rounded-[1.6rem] transition duration-300 hover:-translate-y-1 hover:border-electric-300/35 hover:shadow-glow">
      <Link
        to={`/products/${product.id}`}
        className="relative m-2 overflow-hidden rounded-[1.2rem]"
      >
        <ProductVisual
          name={product.name}
          category={product.category}
          className="transition duration-500 group-hover:scale-[1.025]"
        />
        {product.isFeatured && (
          <span className="absolute left-3 top-3 inline-flex items-center gap-1 rounded-full bg-ink-950/80 px-3 py-1.5 text-[10px] font-bold uppercase tracking-[.16em] text-electric-300 backdrop-blur">
            <Sparkles size={11} /> Featured
          </span>
        )}
      </Link>
      <div className="flex flex-1 flex-col p-5 pt-3">
        <div className="mb-3 flex items-center justify-between gap-2">
          <span className="text-[10px] font-bold uppercase tracking-[.18em] text-electric-300/80">
            {product.category}
          </span>
          <StockBadge quantity={product.stockQuantity} />
        </div>
        <Link
          to={`/products/${product.id}`}
          className="group/title flex items-start justify-between gap-3"
        >
          <h3 className="font-display text-lg font-bold leading-tight text-white group-hover/title:text-electric-300">
            {product.name}
          </h3>
          <ArrowUpRight
            className="shrink-0 text-slate-500 transition group-hover/title:-translate-y-0.5 group-hover/title:translate-x-0.5 group-hover/title:text-electric-300"
            size={18}
          />
        </Link>
        <p className="mt-2 line-clamp-2 text-sm leading-6 text-slate-400">
          {product.shortDescription}
        </p>
        <div className="mt-auto flex items-end justify-between pt-5">
          <div>
            <span className="text-xs text-slate-500">From</span>
            <p className="font-display text-xl font-bold text-white">
              {formatPrice(product.price, product.currency)}
            </p>
          </div>
          {product.isLocalLlmReady && (
            <span className="rounded-lg bg-violet-400/10 px-2.5 py-1.5 text-[10px] font-semibold text-violet-300">
              Local LLM ready
            </span>
          )}
        </div>
      </div>
    </article>
  );
}
