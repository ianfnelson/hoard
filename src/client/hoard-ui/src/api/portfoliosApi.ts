import { hoardApi } from "./hoardApi";
import type { PortfolioDetailDto } from "./dtos/PortfolioDetailDto";
import type { PortfolioPositionsDto } from "@/api/dtos/PortfolioPositionsDto.ts";

export interface GetPortfolioPositionsParams {
  isOpen?: boolean;
}

export async function getPortfolioDetail(
  portfolioId: number
): Promise<PortfolioDetailDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}`
  );
  return response.data;
}

export async function getPortfolioPositions(
  portfolioId: number,
  params?: GetPortfolioPositionsParams
): Promise<PortfolioPositionsDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}/positions`,
    {
      params
    }
  );
  return response.data;
}
