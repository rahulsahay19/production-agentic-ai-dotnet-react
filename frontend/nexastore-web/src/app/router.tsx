import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "@/components/layout/AppShell";
import { CartPage } from "@/features/cart/CartPage";
import { ProductDetailsPage } from "@/features/catalog/ProductDetailsPage";
import { ProductsPage } from "@/features/catalog/ProductsPage";
import { HomePage } from "@/features/home/HomePage";
import { CheckoutPage } from "@/features/orders/CheckoutPage";
import { OrderConfirmationPage } from "@/features/orders/OrderConfirmationPage";
import { OrderDetailsPage } from "@/features/orders/OrderDetailsPage";
import { OrdersPage } from "@/features/orders/OrdersPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

export const router = createBrowserRouter([
  {
    element: <AppShell />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "products", element: <ProductsPage /> },
      { path: "products/:id", element: <ProductDetailsPage /> },
      { path: "cart", element: <CartPage /> },
      { path: "checkout", element: <CheckoutPage /> },
      { path: "orders", element: <OrdersPage /> },
      { path: "orders/:id", element: <OrderDetailsPage /> },
      { path: "orders/:id/confirmed", element: <OrderConfirmationPage /> },
      { path: "*", element: <NotFoundPage /> },
    ],
  },
]);
