export interface AssetSubclassExposureDto {
  id: number
  name: string
  colour: string
  assetClassId: number
  assetClassName: string
  assetClassColour: string
  value: number
  percentage: number
  targetValue: number
  targetPercentage: number | null
  deviationValue: number | null
  deviationPercentage: number
}
