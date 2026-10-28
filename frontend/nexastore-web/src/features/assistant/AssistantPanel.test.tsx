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
      provider: "OpenAI",
      model: "gpt-5.6-terra",
      capabilities: [
        "chat",
        "prompt-styles",
        "streaming",
        "cancellation",
        "product-tools",
        "inventory-tools",
        "rag",
        "citations",
      ],
      responseStyles: ["concise", "balanced", "detailed"],
    });
  });

  it("shows the cloud provider and transparent capability boundary", async () => {
    render(<AssistantPanel open onClose={() => undefined} />);

    expect(screen.getByRole("dialog", { name: "Nexa AI assistant" })).toBeInTheDocument();
    expect(await screen.findByText("OpenAI · gpt-5.6-terra")).toBeInTheDocument();
    expect(screen.getByText("Cloud")).toBeInTheDocument();
    expect(screen.getByText(/answer from NexaStore’s indexed/i)).toBeInTheDocument();
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

  it("renders assistant Markdown as semantic HTML", async () => {
    vi.mocked(streamAssistantResponse).mockImplementation(
      async (_messages, _responseStyle, onEvent) => {
        onEvent({ type: "delta", sequence: 0, content: "### Policy summary\n\n- **14 days** for laptops." });
        onEvent({ type: "done", requestId: "test", chunks: 1, elapsedMs: 50 });
      },
    );
    const user = userEvent.setup();
    render(<AssistantPanel open onClose={() => undefined} />);

    await user.click(screen.getByRole("button", { name: "Summarize the return policy and cite the source." }));

    expect(await screen.findByRole("heading", { name: "Policy summary", level: 3 })).toBeInTheDocument();
    expect(screen.getByText("14 days").tagName).toBe("STRONG");
  });

  it("removes a failed empty response before retrying", async () => {
    vi.mocked(streamAssistantResponse)
      .mockRejectedValueOnce(new Error("Provider failed"))
      .mockImplementationOnce(async (_messages, _responseStyle, onEvent) => {
        onEvent({ type: "delta", sequence: 0, content: "Recovered response" });
        onEvent({ type: "done", requestId: "retry", chunks: 1, elapsedMs: 25 });
      });
    const user = userEvent.setup();
    render(<AssistantPanel open onClose={() => undefined} />);

    await user.click(
      screen.getByRole("button", {
        name: "Show me AI-ready laptops with at least 16 GB VRAM.",
      }),
    );
    expect(await screen.findByText("Provider failed")).toBeInTheDocument();

    const input = screen.getByPlaceholderText("Ask about developer and AI hardware…");
    await user.type(input, "Try the search again");
    await user.click(screen.getByRole("button", { name: "Send message" }));

    expect(await screen.findByText("Recovered response")).toBeInTheDocument();
    const retryMessages = vi.mocked(streamAssistantResponse).mock.calls[1][0];
    expect(retryMessages.every((message) => message.content.trim().length > 0)).toBe(true);
    expect(retryMessages.map((message) => message.content)).toEqual([
      "Show me AI-ready laptops with at least 16 GB VRAM.",
      "Try the search again",
    ]);
  });
});
