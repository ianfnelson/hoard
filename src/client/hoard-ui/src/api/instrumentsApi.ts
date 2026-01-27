import { hoardApi } from "./hoardApi";
import type { InstrumentSummaryDto } from "./dtos/InstrumentSummaryDto";
import type { PagedResult } from "@/api/dtos/PagedResult.ts";

export interface GetInstrumentsParams {
  instrumentTypeId?: number;
  assetClassId?: number;
  assetSubclassId?: number;
  enablePriceUpdates?: boolean;
  enableNewsUpdates?: boolean;
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}

export async function getInstruments(
  params?: GetInstrumentsParams
): Promise<PagedResult<InstrumentSummaryDto>> {
  const response = await hoardApi.get("/instruments", { params });
  return response.data;
}
