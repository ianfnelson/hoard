import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import { setActivePinia, createPinia, type Pinia } from 'pinia'
import TransactionForm from '../TransactionForm.vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'

const vuetify = createVuetify()

let pinia: Pinia

function mountForm(props: Record<string, unknown> = {}) {
  return mount(TransactionForm, {
    props: {
      transaction: null,
      isSaving: false,
      ...props,
    },
    global: {
      plugins: [vuetify, pinia],
    },
  })
}

describe('TransactionForm', () => {
  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)

    const refStore = useReferenceDataStore()
    refStore.transactionTypes = [
      { id: 100, name: 'Buy' },
      { id: 200, name: 'Sell' },
      { id: 401, name: 'Deposit (Personal)' },
    ] as never

    const navStore = useNavigationStore()
    navStore.accounts = [{ id: 1, name: 'ISA' }] as never
  })

  it('should render create title when no transaction provided', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('New Transaction')
  })

  it('should render edit title when transaction provided', () => {
    const wrapper = mountForm({
      transaction: {
        id: 1,
        accountId: 1,
        accountName: 'ISA',
        transactionTypeId: 100,
        transactionTypeName: 'Buy',
        date: '2026-01-15',
        value: 1000,
        units: 100,
        price: 10,
      },
    })
    expect(wrapper.text()).toContain('Edit Transaction')
  })

  it('should emit cancel when cancel button is clicked', async () => {
    const wrapper = mountForm()
    const cancelBtn = wrapper.findAll('button').find((b) => b.text().includes('Cancel'))
    await cancelBtn!.trigger('click')
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('should render core form fields', () => {
    const wrapper = mountForm()
    expect(wrapper.text()).toContain('Date')
    expect(wrapper.text()).toContain('Account')
    expect(wrapper.text()).toContain('Transaction Type')
    expect(wrapper.text()).toContain('Value')
  })
})
