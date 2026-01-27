import { hoardApi } from "./hoardApi";
import type { AccountSummaryDto } from "./dtos/AccountSummaryDto";

export interface GetAccountsParams {
  portfolioId?: number;
  isActive?: boolean;
}

export async function getAccounts(
  params?: GetAccountsParams
): Promise<AccountSummaryDto[]> {
  const response = await hoardApi.get("/accounts", { params });
  return response.data;
}
