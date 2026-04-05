import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getAlertDetail } from '@/api/alertsApi'
import type { AlertDetailDto } from '@/api/dtos/Alerts/AlertDetailDto'

export function useAlertDetail() {
  const alert = ref<AlertDetailDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchAlert(alertId: number) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      alert.value = await getAlertDetail(alertId, controller.signal)
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load alert'
      console.error('Failed to load alert:', e)
      alert.value = null
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
    alert,
    isLoading,
    error,
    fetchAlert,
  }
}
