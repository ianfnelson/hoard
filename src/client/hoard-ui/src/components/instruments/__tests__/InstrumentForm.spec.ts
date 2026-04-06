import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import InstrumentForm from '../InstrumentForm.vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'

const vuetify = createVuetify()

let pinia: Pinia

function mountForm(props: Record<string, unknown> = {}) {
  return mount(InstrumentForm, {
    props: {
      instrument: null,
      isSaving: false,
      ...props,
    },
    global: {
      plugins: [vuetify, pinia],
    },
  })
}

describe('InstrumentForm', () => {
  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)

    const refStore = useReferenceDataStore()
    refStore.instrumentTypes = [{ id: 1, name: 'Equity' }] as never
    refStore.assetClasses = [
      { id: 1, name: 'Equities', subclasses: [{ id: 10, name: 'UK Large Cap' }] },
    ] as never
    refStore.currencies = [{ id: 'GBP', name: 'British Pound' }] as never
  })

  it('should render create title when no instrument provided', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('New Instrument')
  })

  it('should render edit title when instrument provided', () => {
    const wrapper = mountForm({
      instrument: {
        id: 1,
        name: 'Test Fund',
        ticker: 'TF.',
        isin: null,
        instrumentTypeId: 1,
        assetClassId: 1,
        assetSubclassId: 10,
        currencyId: 'GBP',
        dataFeeds: [],
      },
    })
    expect(wrapper.text()).toContain('Edit Instrument')
  })

  it('should emit cancel when cancel button is clicked', async () => {
    const wrapper = mountForm()
    const cancelBtn = wrapper.findAll('button').find((b) => b.text().includes('Cancel'))
    await cancelBtn!.trigger('click')
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('should render data feeds configuration section', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('Data Feeds')
    expect(wrapper.text()).toContain('Enable News Updates')
    expect(wrapper.text()).toContain('Enable Price Updates')
    expect(wrapper.text()).toContain('Enable Quote Updates')
  })
})
