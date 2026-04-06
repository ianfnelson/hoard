import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import DeleteConfirmDialog from '../DeleteConfirmDialog.vue'

const vuetify = createVuetify()

function mountDialog(props: Record<string, unknown> = {}) {
  return mount(DeleteConfirmDialog, {
    props: {
      modelValue: true,
      title: 'Delete Item?',
      message: 'Are you sure?',
      loading: false,
      ...props,
    },
    global: {
      plugins: [vuetify],
      stubs: {
        VDialog: {
          template: '<div><slot /></div>',
          props: ['modelValue'],
        },
      },
    },
  })
}

describe('DeleteConfirmDialog', () => {
  it('should render title and message', () => {
    const wrapper = mountDialog()
    expect(wrapper.text()).toContain('Delete Item?')
    expect(wrapper.text()).toContain('Are you sure?')
  })

  it('should emit confirm when delete button is clicked', async () => {
    const wrapper = mountDialog()
    const deleteBtn = wrapper.findAll('button').find((b) => b.text().includes('Delete'))
    await deleteBtn!.trigger('click')
    expect(wrapper.emitted('confirm')).toHaveLength(1)
  })

  it('should emit update:modelValue false when cancel is clicked', async () => {
    const wrapper = mountDialog()
    const cancelBtn = wrapper.findAll('button').find((b) => b.text().includes('Cancel'))
    await cancelBtn!.trigger('click')
    expect(wrapper.emitted('update:modelValue')).toEqual([[false]])
  })
})
