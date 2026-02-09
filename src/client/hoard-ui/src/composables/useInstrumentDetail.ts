import { ref } from 'vue'
import type { InstrumentDetailDto } from '@/api/dtos/Instruments/InstrumentDetailDto'
import type { InstrumentWriteDto } from '@/api/dtos/Instruments/InstrumentWriteDto'
import {
  getInstrumentDetail,
  createInstrument,
  updateInstrument,
  deleteInstrument,
} from '@/api/instrumentsApi'

export function useInstrumentDetail() {
  const instrument = ref<InstrumentDetailDto | null>(null)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const isDeleting = ref(false)
  const error = ref<string | null>(null)

  async function fetchInstrument(id: number): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      instrument.value = await getInstrumentDetail(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load instrument'
      console.error('Error fetching instrument:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(data: InstrumentWriteDto): Promise<number | null> {
    isSaving.value = true
    error.value = null
    try {
      const id = await createInstrument(data)
      return id
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create instrument'
      console.error('Error creating instrument:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function update(id: number, data: InstrumentWriteDto): Promise<boolean> {
    isSaving.value = true
    error.value = null
    try {
      await updateInstrument(id, data)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update instrument'
      console.error('Error updating instrument:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function remove(id: number): Promise<boolean> {
    isDeleting.value = true
    error.value = null
    try {
      await deleteInstrument(id)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete instrument'
      console.error('Error deleting instrument:', err)
      return false
    } finally {
      isDeleting.value = false
    }
  }

  return {
    instrument,
    isLoading,
    isSaving,
    isDeleting,
    error,
    fetchInstrument,
    create,
    update,
    remove,
  }
}
