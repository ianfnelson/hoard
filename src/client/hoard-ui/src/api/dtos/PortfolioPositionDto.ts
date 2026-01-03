import type { PositionPerformanceDto } from "./PositionPerformanceDto";

export interface PortfolioPositionDto {
  instrumentId: number;
  instrumentName: string;
  instrumentTicker: string;

  openDate: string;      // ISO date: YYYY-MM-DD
  closeDate: string | null;

  performance: PositionPerformanceDto | null;

  portfolioPercentage: number;
}
