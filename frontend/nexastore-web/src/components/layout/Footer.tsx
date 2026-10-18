import { Code2, Globe2, ShieldCheck, Truck, Zap } from "lucide-react";
import { Logo } from "@/components/ui/Logo";

export function Footer() {
  return (
    <footer className="mt-24 border-t border-white/8 bg-black/15">
      <div className="page-shell grid gap-8 py-10 md:grid-cols-[1.2fr_2fr]">
        <div>
          <Logo />
          <p className="mt-4 max-w-sm text-sm leading-6 text-slate-500">
            Purpose-built computing for people who turn ambitious ideas into
            real systems.
          </p>
        </div>
        <div className="grid grid-cols-3 gap-3">
          {[
            [Zap, "Performance verified"],
            [ShieldCheck, "Secure checkout"],
            [Truck, "Pan-India delivery"],
          ].map(([Icon, label]) => (
            <div
              key={String(label)}
              className="rounded-2xl border border-white/8 p-4 text-center text-xs font-semibold text-slate-400"
            >
              <Icon className="mx-auto mb-2 text-electric-300" size={20} />
              {label as string}
            </div>
          ))}
        </div>
      </div>
      <div className="page-shell flex flex-col gap-3 border-t border-white/6 py-5 text-xs text-slate-600 sm:flex-row sm:items-center sm:justify-between">
        <span>
          © 2026 NexaStore. Built for the next generation of builders.
        </span>
        <div className="flex gap-3">
          <Code2 size={16} />
          <Globe2 size={16} />
        </div>
      </div>
    </footer>
  );
}
