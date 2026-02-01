export interface NewsArticleDetailDto {
  id: number
  publishedUtc: string
  retrievedUtc: string
  headline: string
  bodyHtml: string
  url: string
  instrumentId: number
  instrumentName: string
  instrumentTicker: string
}
