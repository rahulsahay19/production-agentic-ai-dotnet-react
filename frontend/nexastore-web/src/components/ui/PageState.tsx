import { AlertTriangle, LoaderCircle } from "lucide-react";
import { Button } from "./Button";

export function LoadingState({
  label = "Loading the good stuff…",
}: {
  label?: string;
}) {
  return (
    <div className="grid min-h-72 place-items-center text-center">
      <div>
        <LoaderCircle
          className="mx-auto mb-4 animate-spin text-electric-300"
          size={30}
        />
        <p className="text-sm text-slate-400">{label}</p>
      </div>
    </div>
  );
}
export function ErrorState({
  message,
  onRetry,
}: {
  message: string;
  onRetry?: () => void;
}) {
  return (
    <div className="glass mx-auto my-10 max-w-xl rounded-3xl p-8 text-center">
      <AlertTriangle className="mx-auto mb-4 text-amber-300" />
      <h2 className="font-display text-xl font-bold">We hit a snag</h2>
      <p className="mt-2 text-sm text-slate-400">{message}</p>
      {onRetry && (
        <Button className="mt-5" onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  );
}
export function EmptyState({ title, copy }: { title: string; copy: string }) {
  return (
    <div className="glass rounded-3xl p-12 text-center">
      <p className="font-display text-2xl font-bold text-white">{title}</p>
      <p className="mt-2 text-slate-400">{copy}</p>
    </div>
  );
}
