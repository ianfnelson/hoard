import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getInstruments, type GetInstrumentsParams } from '@/api/instrumentsApi'
import type { InstrumentSummaryDto } from '@/api/dtos/Instruments/InstrumentSummaryDto'

export function useInstruments() {
  const items = ref<InstrumentSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchInstruments(params?: GetInstrumentsParams) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getInstruments(params, controller.signal)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load instruments'
      console.error('Failed to load instruments:', e)
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
    fetchInstruments,
  }
}
