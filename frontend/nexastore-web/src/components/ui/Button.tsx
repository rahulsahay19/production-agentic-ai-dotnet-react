import type { ButtonHTMLAttributes, PropsWithChildren } from "react";
import { cn } from "@/lib/cn";

type Props = PropsWithChildren<
  ButtonHTMLAttributes<HTMLButtonElement> & {
    variant?: "primary" | "secondary" | "ghost";
    size?: "sm" | "md" | "lg";
  }
>;
export function Button({
  variant = "primary",
  size = "md",
  className,
  children,
  ...props
}: Props) {
  return (
    <button
      className={cn(
        "inline-flex items-center justify-center gap-2 rounded-full font-bold transition duration-200 disabled:cursor-not-allowed disabled:opacity-45",
        variant === "primary" &&
          "bg-electric-300 text-ink-950 shadow-[0_0_28px_rgba(111,231,255,.22)] hover:-translate-y-0.5 hover:bg-white",
        variant === "secondary" &&
          "border border-white/15 bg-white/7 text-white hover:border-electric-300/60 hover:bg-white/12",
        variant === "ghost" &&
          "text-slate-300 hover:bg-white/8 hover:text-white",
        size === "sm" && "px-4 py-2 text-xs",
        size === "md" && "px-5 py-3 text-sm",
        size === "lg" && "px-7 py-4 text-base",
        className,
      )}
      {...props}
    >
      {children}
    </button>
  );
}
