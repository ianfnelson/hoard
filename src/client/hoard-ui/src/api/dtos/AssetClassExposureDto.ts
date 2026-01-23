export interface AssetClassExposureDto {
  assetClassName: string;
  targetPercentage: number | null;
  actualPercentage: number;
  actualValue: number;
  deviationValue: number | null;
}
