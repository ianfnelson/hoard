import type { PortfolioSnapshotDto } from './PortfolioSnapshotDto'

export interface PortfolioSnapshotsDto {
  portfolioId: number
  snapshots: PortfolioSnapshotDto[]
}
