import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getPortfolioSnapshots } from '@/api/portfoliosApi'
import type { PortfolioSnapshotDto } from '@/api/dtos/Portfolios/PortfolioSnapshotDto'

export function usePortfolioSnapshots() {
  const items = ref<PortfolioSnapshotDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchSnapshots(portfolioId: number) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null

    try {
      const result = await getPortfolioSnapshots(portfolioId, controller.signal)
      items.value = result.snapshots
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load snapshots'
      console.error('Failed to load snapshots:', e)
      items.value = []
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
    items,
    isLoading,
    error,
    fetchSnapshots,
  }
}
