import type { PortfolioStatementDto } from './PortfolioStatementDto'

export interface PortfolioStatementsDto {
  portfolioId: number
  snapshots: PortfolioStatementDto[]
}
