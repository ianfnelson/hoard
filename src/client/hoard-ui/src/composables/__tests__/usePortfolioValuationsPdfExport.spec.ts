import { describe, it, expect, vi, beforeEach } from 'vitest'
import { usePortfolioValuationsPdfExport } from '../usePortfolioValuationsPdfExport'
import * as portfoliosApi from '@/api/portfoliosApi'

vi.mock('@/api/portfoliosApi')

describe('usePortfolioValuationsPdfExport', () => {
  let mockCreateObjectURL: ReturnType<typeof vi.fn>
  let mockRevokeObjectURL: ReturnType<typeof vi.fn>
  let clickSpy: ReturnType<typeof vi.fn>
  beforeEach(() => {
    vi.clearAllMocks()

    mockCreateObjectURL = vi.fn().mockReturnValue('blob:test-url')
    mockRevokeObjectURL = vi.fn()
    window.URL.createObjectURL = mockCreateObjectURL as typeof URL.createObjectURL
    window.URL.revokeObjectURL = mockRevokeObjectURL as typeof URL.revokeObjectURL

    clickSpy = vi.fn()
    vi.spyOn(document, 'createElement').mockReturnValue({
      href: '',
      download: '',
      click: clickSpy,
    } as unknown as HTMLAnchorElement)

    vi.spyOn(document.body, 'appendChild').mockImplementation((node) => node)
    vi.spyOn(document.body, 'removeChild').mockImplementation((node) => node)
  })

  it('should have correct initial state', () => {
    const { isExporting } = usePortfolioValuationsPdfExport()
    expect(isExporting.value).toBe(false)
  })

  it('should download PDF with filename from Content-Disposition', async () => {
    const mockResponse = {
      data: new Blob(['pdf-content']),
      headers: { 'content-disposition': 'attachment; filename="portfolio-2026.pdf"' },
    }
    vi.mocked(portfoliosApi.downloadPortfolioValuationsPdf).mockResolvedValue(mockResponse as never)

    const { exportPdf, isExporting } = usePortfolioValuationsPdfExport()
    await exportPdf(1, { from: '2026-01-01' })

    expect(portfoliosApi.downloadPortfolioValuationsPdf).toHaveBeenCalledWith(1, {
      from: '2026-01-01',
    })
    expect(mockCreateObjectURL).toHaveBeenCalled()
    expect(clickSpy).toHaveBeenCalled()
    expect(mockRevokeObjectURL).toHaveBeenCalledWith('blob:test-url')
    expect(isExporting.value).toBe(false)
  })

  it('should use default filename when Content-Disposition missing', async () => {
    const mockResponse = {
      data: new Blob(['pdf-content']),
      headers: {},
    }
    vi.mocked(portfoliosApi.downloadPortfolioValuationsPdf).mockResolvedValue(mockResponse as never)

    const { exportPdf } = usePortfolioValuationsPdfExport()
    await exportPdf(1)

    const createdLink = document.createElement('a')
    expect(createdLink.download).toBe('valuations.pdf')
  })

  it('should handle export error gracefully', async () => {
    vi.mocked(portfoliosApi.downloadPortfolioValuationsPdf).mockRejectedValue(
      new Error('Network error')
    )
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    const { exportPdf, isExporting } = usePortfolioValuationsPdfExport()
    await exportPdf(1)

    expect(isExporting.value).toBe(false)
    expect(consoleErrorSpy).toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })
})
