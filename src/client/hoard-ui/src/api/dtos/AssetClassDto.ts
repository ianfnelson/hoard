export interface AssetSubclassDto {
  id: number;
  name: string;
}

export interface AssetClassDto {
  id: number;
  name: string;
  subclasses: AssetSubclassDto[];
}
