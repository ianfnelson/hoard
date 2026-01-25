import { hoardApi } from "./hoardApi";
import type { InstrumentSummaryDto } from "./dtos/InstrumentSummaryDto";

export interface GetInstrumentsParams {
  pageNumber?: number;
  pageSize?: number;
}

export async function getInstruments(
  params?: GetInstrumentsParams
): Promise<{ items: InstrumentSummaryDto[]; totalCount: number }> {
  const response = await hoardApi.get("/instruments", { params });
  return response.data;
}

export async function getInstrumentDetail(id: number): Promise<any> {
  const response = await hoardApi.get(`/instruments/${id}`);
  return response.data;
}
