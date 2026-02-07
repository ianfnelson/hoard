import { describe, it, expect, beforeEach, afterEach } from 'vitest'
import { ref, nextTick } from 'vue'
import { usePageTitle } from '../usePageTitle'

describe('usePageTitle', () => {
  let originalTitle: string

  beforeEach(() => {
    originalTitle = document.title
  })

  afterEach(() => {
    document.title = originalTitle
  })

  it('should set initial title with prefix', () => {
    const title = ref('Test Page')
    usePageTitle(title)
    expect(document.title).toBe('Hoard : Test Page')
  })

  it('should update title when ref changes', async () => {
    const title = ref('Initial')
    usePageTitle(title)
    expect(document.title).toBe('Hoard : Initial')

    title.value = 'Updated'
    await nextTick()
    expect(document.title).toBe('Hoard : Updated')
  })

  it('should handle null title', () => {
    const title = ref<string | null>(null)
    usePageTitle(title)
    expect(document.title).toBe('Hoard')
  })

  it('should handle undefined title', () => {
    const title = ref<string | undefined>(undefined)
    usePageTitle(title)
    expect(document.title).toBe('Hoard')
  })

  it('should support custom prefix', () => {
    const title = ref('Test')
    usePageTitle(title, { prefix: 'Custom : ' })
    expect(document.title).toBe('Custom : Test')
  })

  it('should support custom default title', () => {
    const title = ref<string | null>(null)
    usePageTitle(title, { defaultTitle: 'Custom Default' })
    expect(document.title).toBe('Custom Default')
  })
})
