import { hoardApi } from './hoardApi'
import type { PriceDetailDto } from './dtos/Prices/PriceDetailDto'
import type { PriceWriteDto } from './dtos/Prices/PriceWriteDto'

export async function getPriceDetail(priceId: number): Promise<PriceDetailDto> {
  const response = await hoardApi.get(`/prices/${priceId}`)
  return response.data
}

export async function createPrice(data: PriceWriteDto): Promise<number> {
  const response = await hoardApi.post('/prices', data)
  return response.data
}

export async function updatePrice(id: number, data: PriceWriteDto): Promise<void> {
  await hoardApi.put(`/prices/${id}`, data)
}

export async function deletePrice(id: number): Promise<void> {
  await hoardApi.delete(`/prices/${id}`)
}
