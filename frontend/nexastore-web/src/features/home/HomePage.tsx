import {
  ArrowRight,
  Bot,
  Box,
  Braces,
  Cpu,
  Gauge,
  Laptop,
  ShieldCheck,
  Sparkles,
} from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { ErrorState, LoadingState } from "@/components/ui/PageState";
import { ProductCard } from "@/features/catalog/components/ProductCard";
import { ProductVisual } from "@/features/catalog/components/ProductVisual";
import { useFeaturedProducts } from "@/features/catalog/queries";
import { getApiErrorMessage } from "@/lib/http";

const categories = [
  {
    title: "AI laptops",
    copy: "Portable compute for local inference.",
    icon: Laptop,
    to: "/products?category=ai-laptops",
    tone: "from-cyan-400/20",
  },
  {
    title: "AI workstations",
    copy: "Maximum VRAM. Zero compromise.",
    icon: Cpu,
    to: "/products?category=ai-workstations",
    tone: "from-violet-400/20",
  },
  {
    title: "Developer machines",
    copy: "Made for .NET, containers and cloud.",
    icon: Braces,
    to: "/products?category=developer-laptops",
    tone: "from-lime-300/15",
  },
  {
    title: "GPUs",
    copy: "Scale every visual and AI workload.",
    icon: Gauge,
    to: "/products?category=gpus",
    tone: "from-fuchsia-400/15",
  },
];
export function HomePage() {
  const q = useFeaturedProducts();
  return (
    <>
      <section className="relative overflow-hidden pb-20 pt-14 lg:pb-28 lg:pt-24">
        <div className="absolute inset-0 grid-fade" />
        <div className="absolute left-[58%] top-5 size-[28rem] rounded-full bg-electric-400/10 blur-[110px] animate-pulse-soft" />
        <div className="page-shell relative grid items-center gap-14 lg:grid-cols-[1.05fr_.95fr]">
          <div>
            <div className="inline-flex items-center gap-2 rounded-full border border-electric-300/20 bg-electric-300/8 px-4 py-2 text-xs font-bold text-electric-300">
              <Sparkles size={14} /> Engineering-grade computing
            </div>
            <h1 className="text-balance mt-7 font-display text-5xl font-bold leading-[.98] tracking-[-.055em] sm:text-6xl lg:text-8xl">
              <span className="gradient-text">Build bigger.</span>
              <br />
              Wait less.
            </h1>
            <p className="mt-7 max-w-xl text-balance text-lg leading-8 text-slate-400">
              Premium systems engineered for developers, creators and AI
              builders—from the first line of code to the final model.
            </p>
            <div className="mt-9 flex flex-wrap gap-3">
              <Link to="/products">
                <Button size="lg">
                  Explore machines <ArrowRight size={18} />
                </Button>
              </Link>
              <Link to="/products?localLlmReady=true">
                <Button size="lg" variant="secondary">
                  Shop local AI ready
                </Button>
              </Link>
            </div>
            <div className="mt-10 flex flex-wrap gap-x-8 gap-y-3 text-xs font-semibold text-slate-500">
              <span className="flex items-center gap-2">
                <ShieldCheck size={15} className="text-lime-300" />{" "}
                Configuration verified
              </span>
              <span className="flex items-center gap-2">
                <Box size={15} className="text-lime-300" /> Live inventory
              </span>
              <span className="flex items-center gap-2">
                <Bot size={15} className="text-violet-300" /> AI-ready journey
              </span>
            </div>
          </div>
          <div className="animate-float relative mx-auto w-full max-w-xl">
            <div className="absolute -inset-5 rounded-[3rem] bg-gradient-to-br from-electric-300/20 to-violet-400/20 blur-2xl" />
            <div className="glass relative rounded-[2.5rem] p-3 shadow-2xl">
              <ProductVisual
                name="NexaBook AI Studio 16"
                category="AI Laptops"
                className="min-h-[430px] rounded-[2rem]"
              />
              <div className="absolute bottom-7 left-7 right-7 flex items-center justify-between rounded-2xl border border-white/10 bg-ink-950/75 p-4 backdrop-blur-xl">
                <div>
                  <p className="text-[10px] font-bold uppercase tracking-[.18em] text-electric-300">
                    Flagship mobile AI
                  </p>
                  <p className="mt-1 font-display text-lg font-bold">
                    16 GB VRAM · 128 GB ready
                  </p>
                </div>
                <ArrowRight className="text-electric-300" />
              </div>
            </div>
          </div>
        </div>
      </section>
      <section className="page-shell py-12">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[.2em] text-electric-300">
              Find your lane
            </p>
            <h2 className="mt-2 font-display text-3xl font-bold tracking-tight sm:text-4xl">
              Built around how you work.
            </h2>
          </div>
          <Link
            to="/products"
            className="hidden items-center gap-2 text-sm font-bold text-slate-300 hover:text-electric-300 sm:flex"
          >
            View all <ArrowRight size={16} />
          </Link>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {categories.map(({ title, copy, icon: Icon, to, tone }) => (
            <Link
              key={title}
              to={to}
              className={`group relative overflow-hidden rounded-3xl border border-white/10 bg-gradient-to-br ${tone} to-white/[.035] p-6 transition hover:-translate-y-1 hover:border-white/20`}
            >
              <Icon className="text-electric-300" size={28} />
              <h3 className="mt-12 font-display text-xl font-bold">{title}</h3>
              <p className="mt-2 text-sm leading-6 text-slate-400">{copy}</p>
              <ArrowRight
                className="absolute bottom-6 right-6 text-slate-600 transition group-hover:translate-x-1 group-hover:text-white"
                size={18}
              />
            </Link>
          ))}
        </div>
      </section>
      <section className="page-shell py-20">
        <div className="mb-8">
          <p className="text-xs font-bold uppercase tracking-[.2em] text-violet-300">
            Curated performance
          </p>
          <h2 className="mt-2 font-display text-3xl font-bold tracking-tight sm:text-4xl">
            Machines worth obsessing over.
          </h2>
        </div>
        {q.isLoading ? (
          <LoadingState />
        ) : q.isError ? (
          <ErrorState
            message={getApiErrorMessage(q.error)}
            onRetry={() => q.refetch()}
          />
        ) : (
          <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4">
            {q.data?.items.slice(0, 4).map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        )}
      </section>
      <section className="page-shell pb-12">
        <div className="relative overflow-hidden rounded-[2.5rem] border border-violet-400/20 bg-[linear-gradient(115deg,rgba(157,123,255,.18),rgba(35,199,245,.08))] p-8 sm:p-12">
          <div className="absolute right-0 top-0 size-64 rounded-full bg-violet-400/15 blur-3xl" />
          <div className="relative max-w-2xl">
            <span className="inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[.2em] text-violet-300">
              <Bot size={15} /> Coming next
            </span>
            <h2 className="text-balance mt-4 font-display text-3xl font-bold sm:text-5xl">
              One assistant. Every buying decision, grounded.
            </h2>
            <p className="mt-5 leading-7 text-slate-300">
              Nexa AI will connect product truth, buying guides, support
              knowledge and controlled store actions—right inside this
              experience.
            </p>
          </div>
        </div>
      </section>
    </>
  );
}
