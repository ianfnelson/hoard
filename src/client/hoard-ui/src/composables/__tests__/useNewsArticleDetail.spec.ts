import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useNewsArticleDetail } from '../useNewsArticleDetail'
import * as newsArticlesApi from '@/api/newsArticlesApi'
import type { NewsArticleDetailDto } from '@/api/dtos/News/NewsArticleDetailDto'

vi.mock('@/api/newsArticlesApi')

describe('useNewsArticleDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { article, isLoading, error } = useNewsArticleDetail()

    expect(article.value).toBeNull()
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch article successfully', async () => {
    const mockArticle = { id: 1, title: 'Annual results' } as NewsArticleDetailDto
    vi.mocked(newsArticlesApi.getNewsArticleDetail).mockResolvedValue(mockArticle)

    const { article, fetchNewsArticle } = useNewsArticleDetail()
    await fetchNewsArticle(1)

    expect(article.value).toEqual(mockArticle)
    expect(newsArticlesApi.getNewsArticleDetail).toHaveBeenCalledWith(1)
  })

  it('should handle error and clear article', async () => {
    vi.mocked(newsArticlesApi.getNewsArticleDetail).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { article, error, fetchNewsArticle } = useNewsArticleDetail()
    await fetchNewsArticle(99)

    expect(article.value).toBeNull()
    expect(error.value).toBe('Failed to load news article')
  })
})
