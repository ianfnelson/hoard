import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getInstrumentDetail } from '@/api/instrumentsApi'
import { useSignalRSubscription } from '@/composables/useSignalRSubscription'
import type { InstrumentDetailDto } from '@/api/dtos/Instruments/InstrumentDetailDto'

export const useInstrumentStore = defineStore('instrument', () => {
  const instrumentId = ref<number | null>(null)

  const instrument = ref<InstrumentDetailDto | null>(null)

  const isLoading = ref(false)
  const lastUpdated = ref<Date | null>(null)
  const error = ref<string | null>(null)

  const { subscribe, unsubscribe } = useSignalRSubscription(
    {
      hubUrl: '/hubs/instrument',
      eventName: 'InstrumentUpdated',
      subscribeMethod: 'SubscribeToInstrument',
      unsubscribeMethod: 'UnsubscribeFromInstrument',
    },
    instrumentId,
    refresh
  )

  async function load(id: number) {
    if (instrumentId.value !== id) {
      await unsubscribe()
      reset()
      instrumentId.value = id
    }

    await subscribe(id)
    await refresh()
  }

  async function refresh() {
    if (!instrumentId.value) return

    isLoading.value = true
    error.value = null

    try {
      instrument.value = await getInstrumentDetail(instrumentId.value)
      lastUpdated.value = new Date()
    } catch (e) {
      error.value = 'Failed to load instrument data'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    instrumentId.value = null
    instrument.value = null
    lastUpdated.value = null
    error.value = null
  }

  return {
    // identity
    instrumentId,

    // raw state
    instrument,

    // lifecycle
    isLoading,
    lastUpdated,
    error,

    // actions
    load,
  }
})
