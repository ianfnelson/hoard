import { hoardApi } from './hoardApi';
import type { NewsArticleDetailDto} from "@/api/dtos/NewsArticleDetailDto.ts";
import type { NewsArticleSummaryDto } from "@/api/dtos/NewsArticleSummaryDto.ts";
import type { PagedResult } from "@/api/dtos/PagedResult.ts";

export interface GetNewsArticlesParams {
  instrumentId?: number;
  fromDate?: string;
  toDate?: string;

  search?: string;

  pageNumber?: number;
  pageSize?: number;

  sortBy?: string;
  sortDirection?: string;
}

export async function getNewsArticles(
  params?: GetNewsArticlesParams
): Promise<PagedResult<NewsArticleSummaryDto>> {
  const response = await hoardApi.get("/news-articles", { params });
  return response.data;
}

export async function getNewsArticleDetail(
  newsArticleId: number
): Promise<NewsArticleDetailDto> {
  const response = await hoardApi.get(
    `/news-articles/${newsArticleId}`
  );
  return response.data;
}
