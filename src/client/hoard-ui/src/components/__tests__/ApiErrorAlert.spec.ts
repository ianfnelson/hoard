import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import ApiErrorAlert from '../ApiErrorAlert.vue'

const vuetify = createVuetify()

function mountAlert(props: { error: string | null; retryable?: boolean }) {
  return mount(ApiErrorAlert, {
    props,
    global: {
      plugins: [vuetify],
    },
  })
}

describe('ApiErrorAlert', () => {
  it('should not render when error is null', () => {
    const wrapper = mountAlert({ error: null })
    expect(wrapper.find('.v-alert').exists()).toBe(false)
  })

  it('should render error message', () => {
    const wrapper = mountAlert({ error: 'Failed to load data' })
    expect(wrapper.text()).toContain('Failed to load data')
  })

  it('should render as error type alert', () => {
    const wrapper = mountAlert({ error: 'Something broke' })
    const alert = wrapper.find('.v-alert')
    expect(alert.exists()).toBe(true)
    expect(alert.html()).toContain('error')
  })

  it('should show retry button when retryable is true', () => {
    const wrapper = mountAlert({ error: 'Failed', retryable: true })
    const btn = wrapper.find('.v-btn')
    expect(btn.exists()).toBe(true)
    expect(btn.text()).toContain('Retry')
  })

  it('should not show retry button by default', () => {
    const wrapper = mountAlert({ error: 'Failed' })
    const btn = wrapper.find('.v-btn')
    expect(btn.exists()).toBe(false)
  })

  it('should emit retry when retry button is clicked', async () => {
    const wrapper = mountAlert({ error: 'Failed', retryable: true })
    await wrapper.find('.v-btn').trigger('click')
    expect(wrapper.emitted('retry')).toHaveLength(1)
  })
})
