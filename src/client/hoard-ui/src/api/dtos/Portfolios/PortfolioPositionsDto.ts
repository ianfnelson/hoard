import type { PortfolioPositionDto } from './PortfolioPositionDto'

export interface PortfolioPositionsDto {
  portfolioId: number
  positions: PortfolioPositionDto[]
}
