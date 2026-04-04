import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useContractNoteDownload } from '../useContractNoteDownload'
import * as transactionsApi from '@/api/transactionsApi'

vi.mock('@/api/transactionsApi')

describe('useContractNoteDownload', () => {
  let mockCreateObjectURL: ReturnType<typeof vi.fn>
  let mockRevokeObjectURL: ReturnType<typeof vi.fn>
  let clickSpy: ReturnType<typeof vi.fn>

  beforeEach(() => {
    vi.clearAllMocks()

    mockCreateObjectURL = vi.fn().mockReturnValue('blob:test-url')
    mockRevokeObjectURL = vi.fn()
    window.URL.createObjectURL = mockCreateObjectURL
    window.URL.revokeObjectURL = mockRevokeObjectURL

    clickSpy = vi.fn()
    vi.spyOn(document, 'createElement').mockReturnValue({
      href: '',
      download: '',
      click: clickSpy,
    } as unknown as HTMLAnchorElement)

    vi.spyOn(document.body, 'appendChild').mockImplementation((node) => node)
    vi.spyOn(document.body, 'removeChild').mockImplementation((node) => node)
  })

  it('should download contract note and trigger file save', async () => {
    const mockBlob = new Blob(['pdf-data'])
    vi.mocked(transactionsApi.downloadContractNote).mockResolvedValue(mockBlob)

    const { handleDownloadContractNote, isDownloading } = useContractNoteDownload()

    expect(isDownloading('REF001')).toBe(false)

    await handleDownloadContractNote(42, 'REF001')

    expect(transactionsApi.downloadContractNote).toHaveBeenCalledWith(42)
    expect(mockCreateObjectURL).toHaveBeenCalledWith(mockBlob)
    expect(clickSpy).toHaveBeenCalled()
    expect(mockRevokeObjectURL).toHaveBeenCalledWith('blob:test-url')
    expect(isDownloading('REF001')).toBe(false)
  })

  it('should track downloading state per reference', async () => {
    let resolveDownload: (value: Blob) => void
    const downloadPromise = new Promise<Blob>((resolve) => {
      resolveDownload = resolve
    })
    vi.mocked(transactionsApi.downloadContractNote).mockReturnValue(downloadPromise)

    const { handleDownloadContractNote, isDownloading } = useContractNoteDownload()

    const downloadTask = handleDownloadContractNote(1, 'REF001')

    expect(isDownloading('REF001')).toBe(true)
    expect(isDownloading('REF002')).toBe(false)

    resolveDownload!(new Blob())
    await downloadTask

    expect(isDownloading('REF001')).toBe(false)
  })

  it('should handle download error gracefully', async () => {
    vi.mocked(transactionsApi.downloadContractNote).mockRejectedValue(new Error('Not found'))
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    const { handleDownloadContractNote, isDownloading } = useContractNoteDownload()
    await handleDownloadContractNote(1, 'REF001')

    expect(isDownloading('REF001')).toBe(false)
    expect(consoleErrorSpy).toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })
})
