export interface AssetSubclassExposureDto {
  assetSubclassName: string
  targetPercentage: number | null
  actualPercentage: number
  actualValue: number
  deviationValue: number | null
}
