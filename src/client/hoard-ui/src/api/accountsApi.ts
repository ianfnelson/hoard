import { hoardApi } from './hoardApi'
import type { AccountSummaryDto } from './dtos/Accounts/AccountSummaryDto'

export interface GetAccountsParams {
  portfolioId?: number
  isActive?: boolean
}

export async function getAccounts(
  params?: GetAccountsParams,
  signal?: AbortSignal
): Promise<AccountSummaryDto[]> {
  const response = await hoardApi.get('/accounts', { params, signal })
  return response.data
}
