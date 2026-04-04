import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import SummaryCard from '../SummaryCard.vue'

const vuetify = createVuetify()

function mountCard(props: { title: string; value?: number; percentage?: number; trend?: number }) {
  return mount(SummaryCard, {
    props,
    global: {
      plugins: [vuetify],
    },
  })
}

describe('SummaryCard', () => {
  it('should render title', () => {
    const wrapper = mountCard({ title: 'Total Value' })
    expect(wrapper.text()).toContain('Total Value')
  })

  it('should render formatted currency value', () => {
    const wrapper = mountCard({ title: 'Value', value: 1234.56 })
    expect(wrapper.text()).toContain('£1,234.56')
  })

  it('should render formatted percentage', () => {
    const wrapper = mountCard({ title: 'Return', percentage: 12.34 })
    expect(wrapper.text()).toContain('12.34%')
  })

  it('should apply text-positive class for positive trend', () => {
    const wrapper = mountCard({ title: 'Test', trend: 5 })
    expect(wrapper.html()).toContain('text-positive')
  })

  it('should apply text-negative class for negative trend', () => {
    const wrapper = mountCard({ title: 'Test', trend: -3 })
    expect(wrapper.html()).toContain('text-negative')
  })

  it('should not apply trend class when trend is zero', () => {
    const wrapper = mountCard({ title: 'Test', trend: 0 })
    expect(wrapper.html()).not.toContain('text-positive')
    expect(wrapper.html()).not.toContain('text-negative')
  })

  it('should handle missing optional props', () => {
    const wrapper = mountCard({ title: 'Empty' })
    expect(wrapper.text()).toContain('Empty')
    // No value/percentage should render empty strings from formatters
  })
})
