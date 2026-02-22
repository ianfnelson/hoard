import type { PortfolioValuationItemDto } from './PortfolioValuationItemDto'

export interface PortfolioValuationsDto {
  portfolioId: number
  toDate: string
  fromDate: string | null
  items: PortfolioValuationItemDto[]
}
