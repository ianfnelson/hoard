import { hoardApi } from "./hoardApi";

export interface InstrumentSummary {
  id: number;
  ticker: string;
  name: string;
  instrumentType: string;
  assetClass: string;
}

export interface GetInstrumentsParams {
  pageNumber?: number;
  pageSize?: number;
}

export async function getInstruments(
  params?: GetInstrumentsParams
): Promise<{ items: InstrumentSummary[]; totalCount: number }> {
  const response = await hoardApi.get("/instruments", { params });
  return response.data;
}

export async function getInstrumentDetail(id: number): Promise<any> {
  const response = await hoardApi.get(`/instruments/${id}`);
  return response.data;
}
