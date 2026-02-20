import { ref } from 'vue'
import { getAlertDetail } from '@/api/alertsApi'
import type { AlertDetailDto } from '@/api/dtos/Alerts/AlertDetailDto'

export function useAlertDetail() {
  const alert = ref<AlertDetailDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchAlert(alertId: number) {
    isLoading.value = true
    error.value = null
    try {
      alert.value = await getAlertDetail(alertId)
    } catch (e) {
      error.value = 'Failed to load alert'
      console.error('Failed to load alert:', e)
      alert.value = null
    } finally {
      isLoading.value = false
    }
  }

  return {
    alert,
    isLoading,
    error,
    fetchAlert,
  }
}
