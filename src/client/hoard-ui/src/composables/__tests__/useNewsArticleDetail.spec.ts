import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
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
    const mockArticle: NewsArticleDetailDto = {
      id: 1,
      publishedUtc: '2026-01-01T00:00:00Z',
      retrievedUtc: '2026-01-01T00:00:00Z',
      headline: 'Annual results',
      bodyHtml: '<p>Results</p>',
      url: 'https://example.com',
      instrumentId: 1,
      instrumentName: 'Test Co',
      instrumentTicker: 'TST',
    }
    vi.mocked(newsArticlesApi.getNewsArticleDetail).mockResolvedValue(mockArticle)

    const { article, fetchNewsArticle } = useNewsArticleDetail()
    await fetchNewsArticle(1)

    expect(article.value).toEqual(mockArticle)
    expect(newsArticlesApi.getNewsArticleDetail).toHaveBeenCalledWith(1, expect.any(AbortSignal))
  })

  it('should silently ignore cancelled requests', async () => {
    vi.mocked(newsArticlesApi.getNewsArticleDetail).mockRejectedValue(new axios.CanceledError())
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { article, error, fetchNewsArticle } = useNewsArticleDetail()
    await fetchNewsArticle(1)

    expect(error.value).toBeNull()
    expect(article.value).toBeNull()
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
