import axios from "axios";
import { env } from "@/config/env";

export const http = axios.create({
  baseURL: env.apiBaseUrl,
  timeout: 15_000,
  headers: { Accept: "application/json" },
});

export type ApiProblem = {
  title: string;
  detail?: string;
  status: number;
  traceId?: string;
  errors?: Record<string, string[]>;
};

export function getApiErrorMessage(error: unknown) {
  if (axios.isAxiosError<ApiProblem>(error))
    return (
      error.response?.data.detail ??
      error.response?.data.title ??
      "The store could not complete that request."
    );
  return error instanceof Error
    ? error.message
    : "Something unexpected happened.";
}
