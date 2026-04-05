import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { useInstrumentDetail } from '../useInstrumentDetail'
import * as instrumentsApi from '@/api/instrumentsApi'
import type { InstrumentDetailDto } from '@/api/dtos/Instruments/InstrumentDetailDto'
import type { InstrumentWriteDto } from '@/api/dtos/Instruments/InstrumentWriteDto'

vi.mock('@/api/instrumentsApi')

describe('useInstrumentDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { instrument, isLoading, isSaving, isDeleting, error } = useInstrumentDetail()

    expect(instrument.value).toBeNull()
    expect(isLoading.value).toBe(false)
    expect(isSaving.value).toBe(false)
    expect(isDeleting.value).toBe(false)
    expect(error.value).toBeNull()
  })

  describe('fetchInstrument', () => {
    it('should fetch instrument successfully', async () => {
      const mockInstrument = { id: 1, name: 'Fundsmith Equity' } as InstrumentDetailDto
      vi.mocked(instrumentsApi.getInstrumentDetail).mockResolvedValue(mockInstrument)

      const { instrument, isLoading, error, fetchInstrument } = useInstrumentDetail()

      const fetchPromise = fetchInstrument(1)
      expect(isLoading.value).toBe(true)

      await fetchPromise

      expect(instrument.value).toEqual(mockInstrument)
      expect(isLoading.value).toBe(false)
      expect(error.value).toBeNull()
      expect(instrumentsApi.getInstrumentDetail).toHaveBeenCalledWith(1, expect.any(AbortSignal))
    })

    it('should silently ignore cancelled fetch', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockRejectedValue(new axios.CanceledError())
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { instrument, error, fetchInstrument } = useInstrumentDetail()
      await fetchInstrument(1)

      expect(error.value).toBeNull()
      expect(instrument.value).toBeNull()
    })

    it('should handle fetch error', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockRejectedValue(new Error('Not found'))
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      const { instrument, error, fetchInstrument } = useInstrumentDetail()
      await fetchInstrument(99)

      expect(instrument.value).toBeNull()
      expect(error.value).toBe('Not found')
      expect(consoleErrorSpy).toHaveBeenCalled()

      consoleErrorSpy.mockRestore()
    })

    it('should use fallback error message for non-Error throws', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockRejectedValue('string error')
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, fetchInstrument } = useInstrumentDetail()
      await fetchInstrument(1)

      expect(error.value).toBe('Failed to load instrument')
    })
  })

  describe('create', () => {
    it('should create instrument and return id', async () => {
      vi.mocked(instrumentsApi.createInstrument).mockResolvedValue(42)

      const { isSaving, error, create } = useInstrumentDetail()
      const data = { name: 'New Fund' } as InstrumentWriteDto

      const createPromise = create(data)
      expect(isSaving.value).toBe(true)

      const id = await createPromise

      expect(id).toBe(42)
      expect(isSaving.value).toBe(false)
      expect(error.value).toBeNull()
      expect(instrumentsApi.createInstrument).toHaveBeenCalledWith(data)
    })

    it('should return null on error', async () => {
      vi.mocked(instrumentsApi.createInstrument).mockRejectedValue(new Error('Validation failed'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, create } = useInstrumentDetail()
      const id = await create({} as InstrumentWriteDto)

      expect(id).toBeNull()
      expect(error.value).toBe('Validation failed')
    })
  })

  describe('update', () => {
    it('should update instrument and return true', async () => {
      vi.mocked(instrumentsApi.updateInstrument).mockResolvedValue(undefined)

      const { isSaving, update } = useInstrumentDetail()
      const data = { name: 'Updated' } as InstrumentWriteDto

      const result = await update(1, data)

      expect(result).toBe(true)
      expect(isSaving.value).toBe(false)
      expect(instrumentsApi.updateInstrument).toHaveBeenCalledWith(1, data)
    })

    it('should return false on error', async () => {
      vi.mocked(instrumentsApi.updateInstrument).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, update } = useInstrumentDetail()
      const result = await update(1, {} as InstrumentWriteDto)

      expect(result).toBe(false)
      expect(error.value).toBe('fail')
    })
  })

  describe('remove', () => {
    it('should delete instrument and return true', async () => {
      vi.mocked(instrumentsApi.deleteInstrument).mockResolvedValue(undefined)

      const { isDeleting, remove } = useInstrumentDetail()

      const removePromise = remove(1)
      expect(isDeleting.value).toBe(true)

      const result = await removePromise

      expect(result).toBe(true)
      expect(isDeleting.value).toBe(false)
      expect(instrumentsApi.deleteInstrument).toHaveBeenCalledWith(1)
    })

    it('should return false on error', async () => {
      vi.mocked(instrumentsApi.deleteInstrument).mockRejectedValue(new Error('Conflict'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, remove } = useInstrumentDetail()
      const result = await remove(1)

      expect(result).toBe(false)
      expect(error.value).toBe('Conflict')
    })
  })
})
