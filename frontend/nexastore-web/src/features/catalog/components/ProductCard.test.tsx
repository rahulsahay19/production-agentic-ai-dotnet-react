import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { ProductCard } from "./ProductCard";
import type { ProductSummary } from "../types";

const product: ProductSummary = {
  id: "product-1",
  sku: "NB-AIS16-2026",
  name: "NexaBook AI Studio 16",
  slug: "nexabook-ai-studio-16",
  brand: "Nexa",
  category: "AI Laptops",
  shortDescription: "A mobile AI studio for local model development.",
  price: 239990,
  currency: "INR",
  stockQuantity: 12,
  isFeatured: true,
  isAiReady: true,
  isLocalLlmReady: true,
  isDeveloperReady: true,
  isCreatorReady: true,
  isGamingReady: true,
};

describe("ProductCard", () => {
  it("renders product data and links to its details", () => {
    render(
      <MemoryRouter>
        <ProductCard product={product} />
      </MemoryRouter>,
    );

    expect(screen.getAllByText(product.name).length).toBeGreaterThan(0);
    expect(screen.getByText("Local LLM ready")).toBeInTheDocument();
    expect(screen.getAllByRole("link")[0]).toHaveAttribute(
      "href",
      `/products/${product.id}`,
    );
  });
});
