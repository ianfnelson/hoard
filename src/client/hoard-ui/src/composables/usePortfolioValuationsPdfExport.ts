import { ref } from 'vue'
import {
  downloadPortfolioValuationsPdf,
  type GetPortfolioValuationsParams,
} from '@/api/portfoliosApi'

export function usePortfolioValuationsPdfExport() {
  const isExporting = ref(false)

  async function exportPdf(portfolioId: number, params?: GetPortfolioValuationsParams) {
    isExporting.value = true
    try {
      const response = await downloadPortfolioValuationsPdf(portfolioId, params)

      const contentDisposition = response.headers['content-disposition']
      let filename = 'valuations.pdf'
      if (contentDisposition) {
        const filenameMatch = contentDisposition.match(/filename="?([^"]+)"?/)
        if (filenameMatch) {
          filename = filenameMatch[1]
        }
      }

      const url = window.URL.createObjectURL(response.data)
      const link = document.createElement('a')
      link.href = url
      link.download = filename
      document.body.appendChild(link)
      link.click()

      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (err) {
      console.error('Error exporting valuations PDF:', err)
    } finally {
      isExporting.value = false
    }
  }

  return {
    exportPdf,
    isExporting,
  }
}
