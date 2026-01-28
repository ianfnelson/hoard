import type {AssetSubclassDto} from "@/api/dtos/AssetSubclassDto.ts";

export interface AssetClassDto {
  id: number;
  name: string;
  subclasses: AssetSubclassDto[];
}
