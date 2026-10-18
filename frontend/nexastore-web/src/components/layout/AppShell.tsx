import { Outlet, ScrollRestoration } from "react-router-dom";
import { Header } from "./Header";
import { Footer } from "./Footer";

export function AppShell() {
  return (
    <div className="min-h-screen overflow-hidden bg-[radial-gradient(circle_at_75%_-10%,rgba(80,70,180,.16),transparent_30%),#06101f] text-slate-100">
      <Header />
      <main>
        <Outlet />
      </main>
      <Footer />
      <ScrollRestoration />
    </div>
  );
}
