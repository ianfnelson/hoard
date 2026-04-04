import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import InstrumentSelect from '../InstrumentSelect.vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'

const vuetify = createVuetify()

let pinia: Pinia

function mountSelect(props: Record<string, unknown> = {}) {
  return mount(InstrumentSelect, {
    props: { modelValue: null, ...props },
    global: {
      plugins: [vuetify, pinia],
    },
  })
}

describe('InstrumentSelect', () => {
  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    vi.clearAllMocks()
  })

  it('should render with default label', () => {
    const wrapper = mountSelect()
    expect(wrapper.html()).toContain('Instrument')
  })

  it('should render with custom label', () => {
    const wrapper = mountSelect({ label: 'Choose Fund' })
    expect(wrapper.html()).toContain('Choose Fund')
  })

  it('should transform instruments into autocomplete items', () => {
    const store = useReferenceDataStore()
    store.instruments = [
      { id: 1, name: 'Fundsmith Equity', context: 'Fund' },
      { id: 2, name: 'Apple Inc', context: null },
    ] as never

    const wrapper = mountSelect()
    const autocomplete = wrapper.findComponent({ name: 'v-autocomplete' })

    const items = autocomplete.props('items')
    expect(items).toEqual([
      { title: 'Fundsmith Equity (Fund)', value: 1, searchText: 'fundsmith equity fund' },
      { title: 'Apple Inc', value: 2, searchText: 'apple inc ' },
    ])
  })

  it('should emit update:modelValue when selection changes', async () => {
    const wrapper = mountSelect({ modelValue: null })

    const autocomplete = wrapper.findComponent({ name: 'v-autocomplete' })
    await autocomplete.vm.$emit('update:modelValue', 5)

    expect(wrapper.emitted('update:modelValue')).toEqual([[5]])
  })

  it('should pass disabled prop through', () => {
    const wrapper = mountSelect({ disabled: true })
    const autocomplete = wrapper.findComponent({ name: 'v-autocomplete' })
    expect(autocomplete.props('disabled')).toBe(true)
  })
})
