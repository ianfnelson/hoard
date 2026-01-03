import type { PortfolioPerformanceDto } from "./PortfolioPerformanceDto";

export interface PortfolioDetailDto {
  id: number;
  name: string;
  isActive: boolean;
  createdUtc: string; // ISO string
  performance: PortfolioPerformanceDto | null;
}
