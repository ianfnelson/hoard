import { hoardApi } from "./hoardApi";
import type { InstrumentTypeDto } from "./dtos/InstrumentTypeDto";
import type { AssetClassDto } from "./dtos/AssetClassDto";

export async function getInstrumentTypes(): Promise<InstrumentTypeDto[]> {
  const response = await hoardApi.get("/reference/instrument-types/");
  return response.data;
}

export async function getAssetClasses(): Promise<AssetClassDto[]> {
  const response = await hoardApi.get("/reference/asset-classes/");
  return response.data;
}
