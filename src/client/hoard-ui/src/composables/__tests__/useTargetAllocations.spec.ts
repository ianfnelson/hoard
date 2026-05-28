import { describe, it, expect } from 'vitest'
import { useTargetAllocations } from '../useTargetAllocations'
import { CASH_ASSET_SUBCLASS_ID } from '@/constants/assetSubclasses'
import type { AssetClassDto } from '@/api/dtos/AssetClasses/AssetClassDto'

const assetClasses: AssetClassDto[] = [
  {
    id: 1,
    name: 'Equity',
    subclasses: [
      { id: 10, name: 'Stocks' },
      { id: 20, name: 'Bonds' },
    ],
  },
  {
    id: 4,
    name: 'Cash',
    subclasses: [{ id: CASH_ASSET_SUBCLASS_ID, name: 'Cash' }],
  },
]

describe('useTargetAllocations', () => {
  it('builds editable rows excluding Cash, seeded from current targets', () => {
    const { rows, initialise } = useTargetAllocations()
    initialise(assetClasses, new Map([[10, 60]]))

    expect(rows.value).toHaveLength(2)
    expect(rows.value.some((r) => r.assetSubclassId === CASH_ASSET_SUBCLASS_ID)).toBe(false)

    const stocks = rows.value.find((r) => r.assetSubclassId === 10)
    expect(stocks?.target).toBe(60)
    expect(stocks?.assetClassName).toBe('Equity')

    const bonds = rows.value.find((r) => r.assetSubclassId === 20)
    expect(bonds?.target).toBe(0)
  })

  it('computes the running total', () => {
    const { rows, total, initialise } = useTargetAllocations()
    initialise(assetClasses, new Map())
    rows.value[0].target = 60
    rows.value[1].target = 30
    expect(total.value).toBe(90)
  })

  it('derives the Cash remainder as 100 minus the total', () => {
    const { rows, cashRemainder, initialise } = useTargetAllocations()
    initialise(assetClasses, new Map())
    rows.value[0].target = 50
    rows.value[1].target = 30
    expect(cashRemainder.value).toBe(20)
  })

  it('is valid at the 0 and 100 boundaries and invalid above 100', () => {
    const { rows, isValid, initialise } = useTargetAllocations()
    initialise(assetClasses, new Map())

    expect(isValid.value).toBe(true) // total 0

    rows.value[0].target = 60
    rows.value[1].target = 40
    expect(isValid.value).toBe(true) // total 100

    rows.value[1].target = 41
    expect(isValid.value).toBe(false) // total 101
  })

  it('builds a payload that excludes Cash and zero-target rows', () => {
    const { rows, buildPayload, initialise } = useTargetAllocations()
    initialise(assetClasses, new Map())
    rows.value[0].target = 70
    rows.value[1].target = 0

    const payload = buildPayload()

    expect(payload).toEqual([{ assetSubclassId: 10, target: 70 }])
    expect(payload.some((a) => a.assetSubclassId === CASH_ASSET_SUBCLASS_ID)).toBe(false)
  })
})
