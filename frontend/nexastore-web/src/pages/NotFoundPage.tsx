import { ArrowLeft, Compass } from "lucide-react";
import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="page-shell grid min-h-[68vh] place-items-center py-20 text-center">
      <div className="max-w-lg">
        <div className="mx-auto grid size-20 place-items-center rounded-3xl border border-electric-300/20 bg-electric-400/10 text-electric-300 shadow-glow">
          <Compass size={36} />
        </div>
        <p className="mt-8 text-xs font-bold uppercase tracking-[.3em] text-electric-300">
          Error 404
        </p>
        <h1 className="mt-3 font-display text-4xl font-black text-white md:text-5xl">
          This aisle doesn’t exist.
        </h1>
        <p className="mt-4 text-slate-400">
          The page may have moved, but your next great piece of technology is
          still close by.
        </p>
        <Link
          className="mt-8 inline-flex items-center justify-center gap-2 rounded-full bg-electric-300 px-5 py-3 text-sm font-bold text-ink-950 shadow-[0_0_28px_rgba(111,231,255,.22)] transition hover:-translate-y-0.5 hover:bg-white"
          to="/products"
        >
          <ArrowLeft size={17} /> Explore products
        </Link>
      </div>
    </section>
  );
}
