import { hoardApi } from "./hoardApi";
import type { PortfolioDetailDto } from "./dtos/PortfolioDetailDto";
import type { PortfolioPositionsDto } from "@/api/dtos/PortfolioPositionsDto.ts";

export async function getPortfolioDetail(
  portfolioId: number
): Promise<PortfolioDetailDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}`
  );
  return response.data;
}

export async function getPortfolioPositions(
  portfolioId: number
): Promise<PortfolioPositionsDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}/positions`
  );
  return response.data;
}
