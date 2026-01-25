import { ref } from "vue";
import { getNewsArticles, type GetNewsArticlesParams } from "@/api/newsArticlesApi.ts";
import type { NewsArticleSummaryDto } from "@/api/dtos/NewsArticleSummaryDto";

export function useNewsArticles() {
  const items = ref<NewsArticleSummaryDto[]>([]);
  const totalCount = ref(0);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  async function fetchNewsArticles(params?: GetNewsArticlesParams) {
    isLoading.value = true;
    error.value = null;
    try {
      const result = await getNewsArticles(params);
      items.value = result.items;
      totalCount.value = result.totalCount;
    } catch (e: any) {
      error.value = "Failed to load news articles";
      console.error("Failed to load news articles:", e);
      items.value = [];
      totalCount.value = 0;
    } finally {
      isLoading.value = false;
    }
  }

  return {
    items,
    totalCount,
    isLoading,
    error,
    fetchNewsArticles
  };
}
