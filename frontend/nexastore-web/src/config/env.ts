export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "",
  customerId:
    import.meta.env.VITE_CUSTOMER_ID ?? "11111111-1111-1111-1111-111111111111",
} as const;
