import { hoardApi } from "./hoardApi";
import type { PortfolioDetailDto } from "./dtos/PortfolioDetailDto";
import type { PortfolioPositionsDto } from "@/api/dtos/PortfolioPositionsDto.ts";
import type { PortfolioExposureDto } from "@/api/dtos/PortfolioExposureDto";
import type { PortfolioInstrumentTypesDto } from "@/api/dtos/PortfolioInstrumentTypesDto";
import type { PortfolioSummaryDto } from "@/api/dtos/PortfolioSummaryDto";

export interface GetPortfolioPositionsParams {
  isOpen?: boolean;
}

export async function getPortfolioList(): Promise<PortfolioSummaryDto[]> {
  const response = await hoardApi.get("/portfolios");
  return response.data;
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
  )
  return response.data;
}

export async function getPortfolioExposure(
  portfolioId: number
): Promise<PortfolioExposureDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}/exposure`
  );
  return response.data;
}

export async function getPortfolioInstrumentTypes(
  portfolioId: number
): Promise<PortfolioInstrumentTypesDto> {
  const response = await hoardApi.get(
    `/portfolios/${portfolioId}/instrument-types`
  );
  return response.data;
}
