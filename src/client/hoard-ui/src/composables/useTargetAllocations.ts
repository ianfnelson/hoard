import { ref, computed } from 'vue'
import type { AssetClassDto } from '@/api/dtos/AssetClasses/AssetClassDto'
import type { TargetAllocationItemDto } from '@/api/dtos/Portfolios/TargetAllocationItemDto'
import { CASH_ASSET_SUBCLASS_ID } from '@/constants/assetSubclasses'

export interface EditableAllocation {
  assetSubclassId: number
  name: string
  assetClassName: string
  target: number
}

function round2(value: number): number {
  return Math.round(value * 100) / 100
}

export function useTargetAllocations() {
  const rows = ref<EditableAllocation[]>([])

  function initialise(assetClasses: AssetClassDto[], currentTargets: Map<number, number>) {
    rows.value = assetClasses.flatMap((assetClass) =>
      assetClass.subclasses
        .filter((subclass) => subclass.id !== CASH_ASSET_SUBCLASS_ID)
        .map((subclass) => ({
          assetSubclassId: subclass.id,
          name: subclass.name,
          assetClassName: assetClass.name,
          target: currentTargets.get(subclass.id) ?? 0,
        }))
    )
  }

  const total = computed(() =>
    round2(rows.value.reduce((sum, row) => sum + (Number(row.target) || 0), 0))
  )

  const cashRemainder = computed(() => round2(100 - total.value))

  const isValid = computed(() => total.value >= 0 && total.value <= 100)

  function buildPayload(): TargetAllocationItemDto[] {
    return rows.value
      .filter((row) => (Number(row.target) || 0) > 0)
      .map((row) => ({ assetSubclassId: row.assetSubclassId, target: Number(row.target) }))
  }

  return { rows, total, cashRemainder, isValid, initialise, buildPayload }
}
