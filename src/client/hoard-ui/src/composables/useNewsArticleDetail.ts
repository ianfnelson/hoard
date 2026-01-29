import { ref } from 'vue'
import { getNewsArticleDetail } from '@/api/newsArticlesApi.ts'
import type { NewsArticleDetailDto } from '@/api/dtos/NewsArticleDetailDto'

export function useNewsArticleDetail() {
  const article = ref<NewsArticleDetailDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchNewsArticle(id: number) {
    isLoading.value = true
    error.value = null
    try {
      article.value = await getNewsArticleDetail(id)
    } catch (e) {
      error.value = 'Failed to load news article'
      console.error('Failed to load news article:', e)
      article.value = null
    } finally {
      isLoading.value = false
    }
  }

  return {
    article,
    isLoading,
    error,
    fetchNewsArticle,
  }
}
