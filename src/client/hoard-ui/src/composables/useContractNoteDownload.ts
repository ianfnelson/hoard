import { ref } from 'vue'
import { downloadContractNote } from '@/api/transactionsApi'

export function useContractNoteDownload() {
  const downloadingRefs = ref<Set<string>>(new Set())

  async function handleDownloadContractNote(transactionId: number, reference: string) {
    downloadingRefs.value.add(reference)
    try {
      const blob = await downloadContractNote(transactionId)

      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `${reference}.pdf`
      document.body.appendChild(link)
      link.click()

      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (err) {
      console.error('Error downloading contract note:', err)
    } finally {
      downloadingRefs.value.delete(reference)
    }
  }

  function isDownloading(reference: string): boolean {
    return downloadingRefs.value.has(reference)
  }

  return {
    handleDownloadContractNote,
    isDownloading,
  }
}
