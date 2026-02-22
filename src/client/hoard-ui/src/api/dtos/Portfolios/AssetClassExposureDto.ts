export interface AssetClassExposureDto {
  id: number
  name: string
  colour: string
  value: number
  percentage: number
  targetValue: number
  targetPercentage: number | null
  deviationValue: number | null
  deviationPercentage: number
}
