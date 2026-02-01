import { ref } from 'vue'
import { getInstruments, type GetInstrumentsParams } from '@/api/instrumentsApi.ts'
import type { InstrumentSummaryDto } from '@/api/dtos/Instruments/InstrumentSummaryDto'

export function useInstruments() {
  const items = ref<InstrumentSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchInstruments(params?: GetInstrumentsParams) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getInstruments(params)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      error.value = 'Failed to load instruments'
      console.error('Failed to load instruments:', e)
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
    fetchInstruments,
  }
}
