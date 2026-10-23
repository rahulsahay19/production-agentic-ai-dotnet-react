import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AssistantPanel } from "./AssistantPanel";
import { getAssistantStatus, streamAssistantResponse } from "./api";

vi.mock("./api", () => ({
  getAssistantStatus: vi.fn(),
  streamAssistantResponse: vi.fn(),
}));

describe("AssistantPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(streamAssistantResponse).mockResolvedValue(undefined);
    vi.mocked(getAssistantStatus).mockResolvedValue({
      enabled: true,
      provider: "Ollama",
      model: "qwen3.5:4b",
      capabilities: [
        "chat",
        "prompt-styles",
        "streaming",
        "cancellation",
        "product-tools",
        "inventory-tools",
      ],
      responseStyles: ["concise", "balanced", "detailed"],
    });
  });

  it("shows the local provider and transparent capability boundary", async () => {
    render(<AssistantPanel open onClose={() => undefined} />);

    expect(screen.getByRole("dialog", { name: "Nexa AI assistant" })).toBeInTheDocument();
    expect(await screen.findByText("Ollama · qwen3.5:4b")).toBeInTheDocument();
    expect(screen.getByText(/search the live NexaStore catalog/i)).toBeInTheDocument();
  });

  it("does not render while closed", () => {
    render(<AssistantPanel open={false} onClose={() => undefined} />);

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("allows the response style to be selected", async () => {
    const user = userEvent.setup();
    render(<AssistantPanel open onClose={() => undefined} />);

    const detailed = screen.getByRole("button", { name: "Detailed" });
    await user.click(detailed);

    expect(detailed).toHaveAttribute("aria-pressed", "true");
    expect(screen.getByRole("button", { name: "Balanced" })).toHaveAttribute(
      "aria-pressed",
      "false",
    );
  });

  it("shows read-only tool activity alongside the streamed answer", async () => {
    vi.mocked(streamAssistantResponse).mockImplementation(
      async (_messages, _responseStyle, onEvent) => {
        onEvent({
          type: "tool",
          name: "search_products",
          label: "Searched the live product catalog",
        });
        onEvent({ type: "delta", sequence: 0, content: "I found two products." });
        onEvent({ type: "done", requestId: "test", chunks: 1, elapsedMs: 50 });
      },
    );
    const user = userEvent.setup();
    render(<AssistantPanel open onClose={() => undefined} />);

    await user.click(
      screen.getByRole("button", {
        name: "Show me AI-ready laptops with at least 16 GB VRAM.",
      }),
    );

    expect(await screen.findByText("Searched the live product catalog")).toBeInTheDocument();
    expect(screen.getByText("I found two products.")).toBeInTheDocument();
  });
});
