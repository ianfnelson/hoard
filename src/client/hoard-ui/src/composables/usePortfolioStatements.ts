import { ref } from "vue";
import { getPortfolioStatements } from "@/api/portfoliosApi";
import type { PortfolioStatementDto } from "@/api/dtos/PortfolioStatementDto";

export function usePortfolioStatements() {
  const items = ref<PortfolioStatementDto[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  async function fetchStatements(portfolioId: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const result = await getPortfolioStatements(portfolioId);
      items.value = result.snapshots;
    } catch (e: any) {
      error.value = "Failed to load statements";
      console.error("Failed to load statements:", e);
      items.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  return {
    items,
    isLoading,
    error,
    fetchStatements
  };
}
