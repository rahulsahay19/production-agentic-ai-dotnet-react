import { describe, expect, it } from "vitest";
import { formatDate, formatPrice } from "./format";

describe("format helpers", () => {
  it("formats Indian Rupee prices without decimals", () => {
    expect(formatPrice(239990)).toContain("2,39,990");
  });

  it("formats API dates for the Indian locale", () => {
    expect(formatDate("2026-10-07T00:00:00Z")).toContain("2026");
  });
});
