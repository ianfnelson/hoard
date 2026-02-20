import { ref } from 'vue'
import { getAlerts, type GetAlertsParams } from '@/api/alertsApi'
import type { AlertSummaryDto } from '@/api/dtos/Alerts/AlertSummaryDto'

export function useAlerts() {
  const items = ref<AlertSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchAlerts(params?: GetAlertsParams) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getAlerts(params)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      error.value = 'Failed to load alerts'
      console.error('Failed to load alerts:', e)
      items.value = []
      totalCount.value = 0
    } finally {
      isLoading.value = false
    }
  }

  return {
    items,
    totalCount,
    isLoading,
    error,
    fetchAlerts,
  }
}
