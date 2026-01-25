export interface NewsArticleSummaryDto {
  id: number;
  publishedUtc: string;
  retrievedUtc: string;
  headline: string;
  instrumentId: number;
  instrumentName: string;
  instrumentTicker: string;
}
