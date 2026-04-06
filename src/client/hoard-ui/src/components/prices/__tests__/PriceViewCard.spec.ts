import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import PriceViewCard from '../PriceViewCard.vue'

const vuetify = createVuetify()

const samplePrice = {
  id: 1,
  instrumentId: 1,
  instrumentName: 'Test Fund',
  instrumentTicker: 'TF.',
  asOfDate: '2026-01-15',
  retrievedUtc: '2026-01-15T10:00:00Z',
  source: 'Yahoo',
  isLocked: false,
  open: 99,
  high: 101,
  low: 98,
  close: 100,
  volume: 50000,
  adjustedClose: 100,
}

function mountCard(props: Record<string, unknown> = {}) {
  return mount(PriceViewCard, {
    props: { price: samplePrice, ...props },
    global: {
      plugins: [vuetify],
      stubs: { 'router-link': { template: '<a><slot /></a>' } },
    },
  })
}

describe('PriceViewCard', () => {
  it('should render price details', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('Yahoo')
    expect(wrapper.text()).toContain('100')
    expect(wrapper.text()).toContain('50000')
  })

  it('should render instrument link', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('Test Fund')
    expect(wrapper.text()).toContain('TF.')
  })

  it('should emit edit when edit button is clicked', async () => {
    const wrapper = mountCard()
    const editBtn = wrapper.findAll('button').find((b) => b.text().includes('Edit'))
    await editBtn!.trigger('click')
    expect(wrapper.emitted('edit')).toHaveLength(1)
  })

  it('should emit delete when delete button is clicked', async () => {
    const wrapper = mountCard()
    const deleteBtn = wrapper.findAll('button').find((b) => b.text().includes('Delete'))
    await deleteBtn!.trigger('click')
    expect(wrapper.emitted('delete')).toHaveLength(1)
  })

  it('should show dash for null OHLV values', () => {
    const wrapper = mountCard({
      price: { ...samplePrice, open: null, high: null, low: null, volume: null },
    })
    const text = wrapper.text()
    expect(text).toContain('—')
  })
})
