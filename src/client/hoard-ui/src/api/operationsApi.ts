import { hoardApi } from './hoardApi'

interface OperationResponse {
  message: string
}

// Analysis Operations
export async function analyseInstruments(params: { asOfDate?: string }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>(
    '/operations/analysis/analyse-instruments',
    params
  )
  return response.data.message
}

// Chrono Operations
export async function triggerCloseOfDay(params: { asOfDate?: string }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>(
    '/operations/chrono/trigger-close-of-day',
    params
  )
  return response.data.message
}

// Holdings Operations
export async function backfillHoldings(params: {
  startDate?: string
  endDate?: string
}): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/holdings/backfill', params)
  return response.data.message
}

export async function calculateHoldings(params: { asOfDate?: string }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/holdings/calculate', params)
  return response.data.message
}

// News Operations
export async function refreshNews(params: { instrumentId?: number }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/news/refresh', params)
  return response.data.message
}

// Performance Operations
export async function calculatePerformance(params: { instrumentId?: number }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>(
    '/operations/performance/calculate',
    params
  )
  return response.data.message
}

// Prices Operations
export async function backfillPrices(params: {
  instrumentId?: number
  startDate?: string
  endDate?: string
}): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/prices/backfill', params)
  return response.data.message
}

export async function refreshPrices(params: {
  instrumentId?: number
  startDate?: string
  endDate?: string
}): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/prices/refresh', params)
  return response.data.message
}

// Quotes Operations
export async function refreshQuotes(): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/quotes/refresh', {})
  return response.data.message
}

// Snapshots Operations
export async function backfillSnapshots(params: {
  portfolioId?: number
  startYear?: number
  endYear?: number
}): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/snapshots/backfill', params)
  return response.data.message
}

export async function calculateSnapshots(params: { year?: number }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/snapshots/calculate', params)
  return response.data.message
}

// Valuations Operations
export async function backfillValuations(params: {
  instrumentId?: number
  startDate?: string
  endDate?: string
}): Promise<string> {
  const response = await hoardApi.post<OperationResponse>('/operations/valuations/backfill', params)
  return response.data.message
}

export async function calculateValuations(params: { asOfDate?: string }): Promise<string> {
  const response = await hoardApi.post<OperationResponse>(
    '/operations/valuations/calculate',
    params
  )
  return response.data.message
}
