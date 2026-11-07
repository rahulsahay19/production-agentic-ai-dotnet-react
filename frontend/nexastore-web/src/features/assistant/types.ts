export type AssistantRole = "user" | "assistant";
export type AssistantResponseStyle = "concise" | "balanced" | "detailed";

export type AssistantMessage = {
  id: string;
  role: AssistantRole;
  content: string;
  tools?: AssistantToolActivity[];
  governance?: JevDecision;
  verification?: JevVerification;
};

export type AssistantToolActivity = { name: string; label: string };

export type JevDecision = {
  route: string;
  risk: string;
  evidenceRequirement: string;
  allowed: boolean;
  reasonCode: string;
  requiredTools: string[];
};

export type JevVerification = {
  passed: boolean;
  status: string;
  evidenceUsed: string[];
  missingEvidence: string[];
};

export type AssistantStatus = {
  enabled: boolean;
  provider: string;
  model: string;
  capabilities: string[];
  responseStyles: AssistantResponseStyle[];
};

export type AssistantStreamEvent =
  | {
      type: "started";
      requestId: string;
      model: string;
      responseStyle: AssistantResponseStyle;
    }
  | { type: "delta"; sequence: number; content: string }
  | { type: "tool"; name: string; label: string }
  | ({ type: "governance" } & JevDecision)
  | ({ type: "verification" } & JevVerification)
  | { type: "done"; requestId: string; chunks: number; elapsedMs: number }
  | { type: "error"; message: string };
