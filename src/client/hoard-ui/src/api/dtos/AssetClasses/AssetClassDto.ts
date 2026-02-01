import type { AssetSubclassDto } from './AssetSubclassDto'

export interface AssetClassDto {
  id: number
  name: string
  subclasses: AssetSubclassDto[]
}
