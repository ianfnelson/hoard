import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import TransactionViewCard from '../TransactionViewCard.vue'

const vuetify = createVuetify()

const sampleTransaction = {
  id: 1,
  accountId: 1,
  accountName: 'ISA',
  transactionTypeId: 100,
  transactionTypeName: 'Buy',
  date: '2026-01-15',
  value: 1000,
  units: 100,
  price: 10,
}

function mountCard(props: Record<string, unknown> = {}) {
  return mount(TransactionViewCard, {
    props: { transaction: sampleTransaction, ...props },
    global: {
      plugins: [vuetify],
      stubs: { 'router-link': { template: '<a><slot /></a>' } },
    },
  })
}

describe('TransactionViewCard', () => {
  it('should render transaction details', () => {
    const wrapper = mountCard()
    expect(wrapper.text()).toContain('ISA')
    expect(wrapper.text()).toContain('Buy')
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

  it('should show notes when present', () => {
    const wrapper = mountCard({
      transaction: { ...sampleTransaction, notes: 'Monthly investment' },
    })
    expect(wrapper.text()).toContain('Monthly investment')
  })
})
