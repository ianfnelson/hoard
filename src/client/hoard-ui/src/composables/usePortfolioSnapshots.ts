import { ref } from 'vue'
import { getPortfolioSnapshots } from '@/api/portfoliosApi'
import type { PortfolioSnapshotDto } from '@/api/dtos/Portfolios/PortfolioSnapshotDto'

export function usePortfolioSnapshots() {
  const items = ref<PortfolioSnapshotDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSnapshots(portfolioId: number) {
    isLoading.value = true
    error.value = null

    try {
      const result = await getPortfolioSnapshots(portfolioId)
      items.value = result.snapshots
    } catch (e) {
      error.value = 'Failed to load snapshots'
      console.error('Failed to load snapshots:', e)
      items.value = []
    } finally {
      isLoading.value = false
    }
  }

  return {
    items,
    isLoading,
    error,
    fetchSnapshots,
  }
}
