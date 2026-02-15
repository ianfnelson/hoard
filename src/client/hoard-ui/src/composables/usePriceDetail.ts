import { ref } from 'vue'
import type { PriceDetailDto } from '@/api/dtos/Prices/PriceDetailDto'
import type { PriceWriteDto } from '@/api/dtos/Prices/PriceWriteDto'
import { getPriceDetail, createPrice, updatePrice, deletePrice } from '@/api/pricesApi'

export function usePriceDetail() {
  const price = ref<PriceDetailDto | null>(null)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const isDeleting = ref(false)
  const error = ref<string | null>(null)

  async function fetchPrice(id: number): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      price.value = await getPriceDetail(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load price'
      console.error('Error fetching price:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(data: PriceWriteDto): Promise<number | null> {
    isSaving.value = true
    error.value = null
    try {
      const id = await createPrice(data)
      return id
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create price'
      console.error('Error creating price:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function update(id: number, data: PriceWriteDto): Promise<boolean> {
    isSaving.value = true
    error.value = null
    try {
      await updatePrice(id, data)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update price'
      console.error('Error updating price:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function remove(id: number): Promise<boolean> {
    isDeleting.value = true
    error.value = null
    try {
      await deletePrice(id)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete price'
      console.error('Error deleting price:', err)
      return false
    } finally {
      isDeleting.value = false
    }
  }

  return {
    price,
    isLoading,
    isSaving,
    isDeleting,
    error,
    fetchPrice,
    create,
    update,
    remove,
  }
}
