import { watch, type Ref } from 'vue'

/**
 * Composable for managing page titles with consistent "Hoard : " prefix
 * @param title - Reactive reference to the title (without "Hoard : " prefix)
 * @param options - Optional configuration
 */
export function usePageTitle(
  title: Ref<string | null | undefined>,
  options?: {
    prefix?: string
    defaultTitle?: string
  }
) {
  const prefix = options?.prefix ?? 'Hoard : '
  const defaultTitle = options?.defaultTitle ?? 'Hoard'

  watch(
    title,
    (newTitle) => {
      if (newTitle) {
        document.title = `${prefix}${newTitle}`
      } else {
        document.title = defaultTitle
      }
    },
    { immediate: true }
  )
}
