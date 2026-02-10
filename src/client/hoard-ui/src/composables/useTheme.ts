import { ref, watch } from 'vue'
import { useTheme as useVuetifyTheme } from 'vuetify'

const THEME_STORAGE_KEY = 'hoard-theme'

const isDark = ref<boolean>(false)

/**
 * Composable for managing light/dark theme toggle
 * Persists theme preference to localStorage
 */
export function useTheme() {
  const vuetifyTheme = useVuetifyTheme()

  // Initialize from localStorage on first use
  if (isDark.value === false && localStorage.getItem(THEME_STORAGE_KEY) === 'dark') {
    isDark.value = true
    vuetifyTheme.global.name.value = 'dark'
  }

  // Watch for changes and persist to localStorage
  watch(isDark, (newValue) => {
    vuetifyTheme.global.name.value = newValue ? 'dark' : 'light'
    localStorage.setItem(THEME_STORAGE_KEY, newValue ? 'dark' : 'light')
  })

  const toggleTheme = () => {
    isDark.value = !isDark.value
  }

  return {
    isDark,
    toggleTheme,
  }
}
