import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import PriceForm from '../PriceForm.vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'

const vuetify = createVuetify()

let pinia: Pinia

function mountForm(props: Record<string, unknown> = {}) {
  return mount(PriceForm, {
    props: {
      price: null,
      instrumentId: 1,
      isSaving: false,
      ...props,
    },
    global: {
      plugins: [vuetify, pinia],
    },
  })
}

describe('PriceForm', () => {
  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)

    const refStore = useReferenceDataStore()
    refStore.instruments = [{ id: 1, name: 'Test Fund', context: null }] as never
  })

  it('should render create title when no price provided', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('New Price')
  })

  it('should render edit title when price provided', () => {
    const wrapper = mountForm({
      price: {
        id: 1,
        instrumentId: 1,
        instrumentName: 'Test Fund',
        instrumentTicker: 'TF.',
        asOfDate: '2026-01-15',
        retrievedUtc: '2026-01-15T10:00:00Z',
        source: 'Manual',
        isLocked: false,
        open: null,
        high: null,
        low: null,
        close: 100,
        volume: null,
        adjustedClose: 100,
      },
    })
    expect(wrapper.text()).toContain('Edit Price')
  })

  it('should emit cancel when cancel button is clicked', async () => {
    const wrapper = mountForm()
    const cancelBtn = wrapper.findAll('button').find((b) => b.text().includes('Cancel'))
    await cancelBtn!.trigger('click')
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('should render price fields', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('As Of Date')
    expect(wrapper.text()).toContain('Source')
    expect(wrapper.text()).toContain('Close')
    expect(wrapper.text()).toContain('Adjusted Close')
  })
})
