import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import TargetAllocationsDialog from '../TargetAllocationsDialog.vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { setTargetAllocations } from '@/api/portfoliosApi'
import { CASH_ASSET_SUBCLASS_ID } from '@/constants/assetSubclasses'

vi.mock('@/api/portfoliosApi', () => ({
  setTargetAllocations: vi.fn().mockResolvedValue(undefined),
  getPortfolioDetail: vi.fn().mockResolvedValue({}),
  getPortfolioPositions: vi.fn().mockResolvedValue({}),
  getPortfolioExposure: vi.fn().mockResolvedValue({ assetSubclasses: [] }),
  getPortfolioInstrumentTypes: vi.fn().mockResolvedValue({}),
}))

const vuetify = createVuetify()

const assetClasses = [
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

let pinia: Pinia

async function mountDialog(targetPercentages: Record<number, number>) {
  pinia = createPinia()
  setActivePinia(pinia)

  const refStore = useReferenceDataStore()
  refStore.loadAssetClasses = vi.fn().mockImplementation(async () => {
    refStore.assetClasses = assetClasses as never
  })

  const portfolioStore = usePortfolioStore()
  portfolioStore.portfolioId = 1
  portfolioStore.exposure = {
    assetSubclasses: Object.entries(targetPercentages).map(([id, targetPercentage]) => ({
      id: Number(id),
      targetPercentage,
    })),
  } as never

  const wrapper = mount(TargetAllocationsDialog, {
    props: { modelValue: true, portfolioId: 1 },
    global: {
      plugins: [vuetify, pinia],
      stubs: {
        VDialog: {
          template: '<div><slot /></div>',
          props: ['modelValue'],
        },
      },
    },
  })
  await flushPromises()
  return wrapper
}

function saveButton(wrapper: Awaited<ReturnType<typeof mountDialog>>) {
  return wrapper.findAll('button').find((b) => b.text().includes('Save'))!
}

describe('TargetAllocationsDialog', () => {
  beforeEach(() => {
    vi.mocked(setTargetAllocations).mockClear()
  })

  it('renders an input per non-Cash subclass and excludes Cash', async () => {
    const wrapper = await mountDialog({ 10: 60 })
    const text = wrapper.text()
    expect(text).toContain('Stocks')
    expect(text).toContain('Bonds')
    expect(text).toContain('Cash (remainder)')
    // Cash is derived, not an editable input: only the two non-Cash subclasses
    expect(wrapper.findAll('input[type="number"]')).toHaveLength(2)
  })

  it('disables Save when the total exceeds 100', async () => {
    const wrapper = await mountDialog({ 10: 60, 20: 60 })
    expect(saveButton(wrapper).attributes('disabled')).toBeDefined()
  })

  it('saves a payload excluding Cash and zero-target rows', async () => {
    const wrapper = await mountDialog({ 10: 60 })
    expect(saveButton(wrapper).attributes('disabled')).toBeUndefined()

    await saveButton(wrapper).trigger('click')
    await flushPromises()

    expect(setTargetAllocations).toHaveBeenCalledWith(1, [{ assetSubclassId: 10, target: 60 }])
    expect(wrapper.emitted('saved')).toHaveLength(1)
  })
})
