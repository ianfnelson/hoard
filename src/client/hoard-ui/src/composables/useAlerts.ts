import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getAlerts, type GetAlertsParams } from '@/api/alertsApi'
import type { AlertSummaryDto } from '@/api/dtos/Alerts/AlertSummaryDto'

export function useAlerts() {
  const items = ref<AlertSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchAlerts(params?: GetAlertsParams) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getAlerts(params, controller.signal)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load alerts'
      console.error('Failed to load alerts:', e)
      items.value = []
      totalCount.value = 0
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
    totalCount,
    isLoading,
    error,
    fetchAlerts,
  }
}
