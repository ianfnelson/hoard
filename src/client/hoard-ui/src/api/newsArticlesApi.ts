import { hoardApi } from './hoardApi'
import type { NewsArticleDetailDto } from '@/api/dtos/News/NewsArticleDetailDto'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

export interface GetNewsArticlesParams {
  instrumentId?: number
  fromDate?: string
  toDate?: string

  pageNumber?: number
  pageSize?: number

  search?: string

  sortBy?: string
  sortDirection?: string
}

export async function getNewsArticles(
  params?: GetNewsArticlesParams
): Promise<PagedResult<NewsArticleSummaryDto>> {
  const response = await hoardApi.get('/news-articles', { params })
  return response.data
}

export async function getNewsArticleDetail(newsArticleId: number): Promise<NewsArticleDetailDto> {
  const response = await hoardApi.get(`/news-articles/${newsArticleId}`)
  return response.data
}
