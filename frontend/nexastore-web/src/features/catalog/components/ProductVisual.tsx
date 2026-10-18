import {
  Box,
  Cpu,
  Laptop,
  MemoryStick,
  Monitor,
  PackageOpen,
} from "lucide-react";
import { createElement } from "react";
import { cn } from "@/lib/cn";

const iconFor = (category: string) => {
  const c = category.toLowerCase();
  if (c.includes("laptop")) return Laptop;
  if (c.includes("monitor")) return Monitor;
  if (c.includes("memory")) return MemoryStick;
  if (c.includes("gpu") || c.includes("workstation")) return Cpu;
  if (c.includes("storage") || c.includes("mini")) return Box;
  return PackageOpen;
};
export function ProductVisual({
  name,
  category,
  className,
}: {
  name: string;
  category: string;
  className?: string;
}) {
  return (
    <div
      className={cn(
        "relative isolate grid min-h-56 place-items-center overflow-hidden rounded-2xl bg-[radial-gradient(circle_at_30%_15%,rgba(111,231,255,.22),transparent_35%),linear-gradient(145deg,#112944,#07111f_70%)]",
        className,
      )}
    >
      <div className="absolute inset-0 grid-fade opacity-50" />
      <div className="absolute -right-12 -top-12 size-40 rounded-full bg-violet-400/20 blur-3xl" />
      <div className="relative flex flex-col items-center">
        <div className="grid size-28 place-items-center rounded-[2rem] border border-white/15 bg-white/8 shadow-2xl backdrop-blur">
          {createElement(iconFor(category), {
            size: 57,
            strokeWidth: 1.25,
            className:
              "text-electric-300 drop-shadow-[0_0_18px_rgba(111,231,255,.45)]",
          })}
        </div>
        <span className="mt-5 max-w-52 text-center font-display text-sm font-semibold text-white/80">
          {name}
        </span>
      </div>
    </div>
  );
}
