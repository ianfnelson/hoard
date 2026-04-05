import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getNewsArticleDetail } from '@/api/newsArticlesApi'
import type { NewsArticleDetailDto } from '@/api/dtos/News/NewsArticleDetailDto'

export function useNewsArticleDetail() {
  const article = ref<NewsArticleDetailDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchNewsArticle(id: number) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      article.value = await getNewsArticleDetail(id, controller.signal)
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load news article'
      console.error('Failed to load news article:', e)
      article.value = null
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
    article,
    isLoading,
    error,
    fetchNewsArticle,
  }
}
