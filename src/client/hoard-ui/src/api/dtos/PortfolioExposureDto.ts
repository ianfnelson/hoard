import type { AssetClassExposureDto } from "./AssetClassExposureDto";
import type { AssetSubclassExposureDto } from "./AssetSubclassExposureDto";
import type { RebalanceActionDto } from "./RebalanceActionDto";

export interface PortfolioExposureDto {
  assetClasses: AssetClassExposureDto[];
  assetSubclasses: AssetSubclassExposureDto[];
  rebalanceActions: RebalanceActionDto[];
}
