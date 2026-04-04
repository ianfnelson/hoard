import { describe, it, expect, vi, beforeEach } from 'vitest'
import { nextTick } from 'vue'

const mockThemeName = { value: 'light' }
vi.mock('vuetify', () => ({
  useTheme: () => ({
    global: {
      name: mockThemeName,
    },
  }),
}))

import { useTheme } from '../useTheme'

describe('useTheme', () => {
  beforeEach(() => {
    localStorage.clear()
    mockThemeName.value = 'light'

    // Reset the module-level isDark ref
    const { isDark } = useTheme()
    isDark.value = false
  })

  it('should default to light theme', () => {
    const { isDark } = useTheme()
    expect(isDark.value).toBe(false)
  })

  it('should toggle theme and persist to localStorage', async () => {
    const { isDark, toggleTheme } = useTheme()

    toggleTheme()
    await nextTick()

    expect(isDark.value).toBe(true)
    expect(mockThemeName.value).toBe('dark')
    expect(localStorage.getItem('hoard-theme')).toBe('dark')

    toggleTheme()
    await nextTick()

    expect(isDark.value).toBe(false)
    expect(mockThemeName.value).toBe('light')
    expect(localStorage.getItem('hoard-theme')).toBe('light')
  })

  it('should restore dark theme from localStorage', () => {
    localStorage.setItem('hoard-theme', 'dark')

    // Reset isDark so the initialization check fires
    const { isDark } = useTheme()
    isDark.value = false

    // Re-call to trigger the localStorage check path
    const result = useTheme()
    expect(result.isDark.value).toBe(true)
    expect(mockThemeName.value).toBe('dark')
  })
})
