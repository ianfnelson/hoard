export interface AccountSummaryDto {
  id: number;
  name: string;
  isActive: boolean;
  createdUtc: string;
  accountTypeId: number;
  accountTypeName: string;
}
