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
    vi.mocked(streamAssistantResponse).mockResolvedValue(undefined);
    vi.mocked(getAssistantStatus).mockResolvedValue({
      enabled: true,
      provider: "Ollama",
      model: "qwen3.5:4b",
      capabilities: ["chat", "prompt-styles", "streaming", "cancellation"],
      responseStyles: ["concise", "balanced", "detailed"],
    });
  });

  it("shows the local provider and transparent capability boundary", async () => {
    render(<AssistantPanel open onClose={() => undefined} />);

    expect(screen.getByRole("dialog", { name: "Nexa AI assistant" })).toBeInTheDocument();
    expect(await screen.findByText("Ollama · qwen3.5:4b")).toBeInTheDocument();
    expect(screen.getByText(/Live product, stock, and order access arrives/i)).toBeInTheDocument();
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
});
