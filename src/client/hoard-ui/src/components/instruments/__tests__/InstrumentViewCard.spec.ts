import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import InstrumentViewCard from '../InstrumentViewCard.vue'

const vuetify = createVuetify()

const sampleInstrument = {
  id: 1,
  name: 'Fundsmith Equity',
  ticker: 'FEET',
  isin: 'GB00B4Q5X527',
  instrumentTypeName: 'Fund',
  assetClassName: 'Equities',
  assetSubclassName: 'Global',
  currencyName: 'British Pound',
  quote: null,
  dataFeeds: [],
}

function mountCard(props: Record<string, unknown> = {}) {
  return mount(InstrumentViewCard, {
    props: { instrument: sampleInstrument, ...props },
    global: {
      plugins: [vuetify],
      stubs: {
        VChipGroup: { template: '<div><slot /></div>' },
      },
    },
  })
}

describe('InstrumentViewCard', () => {
  it('should render instrument name', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('Fundsmith Equity')
  })

  it('should render ticker and ISIN', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('FEET')
    expect(wrapper.text()).toContain('GB00B4Q5X527')
  })

  it('should render classification fields', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('Fund')
    expect(wrapper.text()).toContain('Equities')
    expect(wrapper.text()).toContain('Global')
    expect(wrapper.text()).toContain('British Pound')
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

  it('should show quote data when quote is present', () => {
    const wrapper = mountCard({
      instrument: {
        ...sampleInstrument,
        quote: {
          regularMarketPrice: 5.42,
          regularMarketChange: 0.12,
          regularMarketChangePercent: 2.26,
          bid: 5.41,
          ask: 5.43,
          fiftyTwoWeekLow: 4.1,
          fiftyTwoWeekHigh: 6.0,
          retrievedUtc: '2026-04-06T10:00:00Z',
        },
      },
    })
    expect(wrapper.text()).toContain('5.42')
    expect(wrapper.text()).toContain('0.12')
  })

  it('should render data feeds section when feeds are present', () => {
    const wrapper = mountCard({
      instrument: {
        ...sampleInstrument,
        dataFeeds: [{ dataFeedId: 1, dataFeedName: 'News', ticker: 'FEET' }],
      },
    })
    expect(wrapper.text()).toContain('Active Data Feeds')
  })
})
