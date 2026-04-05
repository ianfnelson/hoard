import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { usePriceDetail } from '../usePriceDetail'
import * as pricesApi from '@/api/pricesApi'
import type { PriceDetailDto } from '@/api/dtos/Prices/PriceDetailDto'
import type { PriceWriteDto } from '@/api/dtos/Prices/PriceWriteDto'

vi.mock('@/api/pricesApi')

describe('usePriceDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { price, isLoading, isSaving, isDeleting, error } = usePriceDetail()

    expect(price.value).toBeNull()
    expect(isLoading.value).toBe(false)
    expect(isSaving.value).toBe(false)
    expect(isDeleting.value).toBe(false)
    expect(error.value).toBeNull()
  })

  describe('fetchPrice', () => {
    it('should fetch successfully', async () => {
      const mockPrice = { id: 1, close: 100.5 } as PriceDetailDto
      vi.mocked(pricesApi.getPriceDetail).mockResolvedValue(mockPrice)

      const { price, fetchPrice } = usePriceDetail()
      await fetchPrice(1)

      expect(price.value).toEqual(mockPrice)
    })

    it('should silently ignore cancelled fetch', async () => {
      vi.mocked(pricesApi.getPriceDetail).mockRejectedValue(new axios.CanceledError())
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { price, error, fetchPrice } = usePriceDetail()
      await fetchPrice(1)

      expect(error.value).toBeNull()
      expect(price.value).toBeNull()
    })

    it('should handle error', async () => {
      vi.mocked(pricesApi.getPriceDetail).mockRejectedValue(new Error('Not found'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, fetchPrice } = usePriceDetail()
      await fetchPrice(99)

      expect(error.value).toBe('Not found')
    })
  })

  describe('create', () => {
    it('should create and return id', async () => {
      vi.mocked(pricesApi.createPrice).mockResolvedValue(5)

      const { create } = usePriceDetail()
      const id = await create({ close: 100 } as PriceWriteDto)

      expect(id).toBe(5)
    })

    it('should return null on error', async () => {
      vi.mocked(pricesApi.createPrice).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { create } = usePriceDetail()
      const id = await create({} as PriceWriteDto)

      expect(id).toBeNull()
    })
  })

  describe('update', () => {
    it('should update and return true', async () => {
      vi.mocked(pricesApi.updatePrice).mockResolvedValue(undefined)

      const { update } = usePriceDetail()
      const result = await update(1, {} as PriceWriteDto)

      expect(result).toBe(true)
    })

    it('should return false on error', async () => {
      vi.mocked(pricesApi.updatePrice).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { update } = usePriceDetail()
      const result = await update(1, {} as PriceWriteDto)

      expect(result).toBe(false)
    })
  })

  describe('remove', () => {
    it('should delete and return true', async () => {
      vi.mocked(pricesApi.deletePrice).mockResolvedValue(undefined)

      const { remove } = usePriceDetail()
      const result = await remove(1)

      expect(result).toBe(true)
    })

    it('should return false on error', async () => {
      vi.mocked(pricesApi.deletePrice).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { remove } = usePriceDetail()
      const result = await remove(1)

      expect(result).toBe(false)
    })
  })
})
