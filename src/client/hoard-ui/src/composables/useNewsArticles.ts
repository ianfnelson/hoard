import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getNewsArticles, type GetNewsArticlesParams } from '@/api/newsArticlesApi'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'

export function useNewsArticles() {
  const items = ref<NewsArticleSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchNewsArticles(params?: GetNewsArticlesParams) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getNewsArticles(params, controller.signal)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load news articles'
      console.error('Failed to load news articles:', e)
      items.value = []
      totalCount.value = 0
    } finally {
      if (abortController === controller) {
        isLoading.value = false
      }
    }
  }

  onUnmounted(() => {
    abortController?.abort()
  })

  return {
    items,
    totalCount,
    isLoading,
    error,
    fetchNewsArticles,
  }
}
