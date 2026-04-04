import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useNewsArticles } from '../useNewsArticles'
import * as newsArticlesApi from '@/api/newsArticlesApi'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

vi.mock('@/api/newsArticlesApi')

describe('useNewsArticles', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, totalCount, isLoading, error } = useNewsArticles()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch news articles successfully', async () => {
    const mockResponse: PagedResult<NewsArticleSummaryDto> = {
      items: [
        {
          id: 1,
          publishedUtc: '2026-01-01T00:00:00Z',
          retrievedUtc: '2026-01-01T00:00:00Z',
          headline: 'Annual results',
          instrumentId: 1,
          instrumentName: 'Test Co',
          instrumentTicker: 'TST',
        } satisfies NewsArticleSummaryDto,
      ],
      totalCount: 1,
    }
    vi.mocked(newsArticlesApi.getNewsArticles).mockResolvedValue(mockResponse)

    const { items, totalCount, fetchNewsArticles } = useNewsArticles()
    await fetchNewsArticles({ pageSize: 5 })

    expect(items.value).toEqual(mockResponse.items)
    expect(totalCount.value).toBe(1)
    expect(newsArticlesApi.getNewsArticles).toHaveBeenCalledWith({ pageSize: 5 })
  })

  it('should handle error', async () => {
    vi.mocked(newsArticlesApi.getNewsArticles).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, totalCount, error, fetchNewsArticles } = useNewsArticles()
    await fetchNewsArticles()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(error.value).toBe('Failed to load news articles')
  })
})
