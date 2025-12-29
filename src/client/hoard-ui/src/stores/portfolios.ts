import { defineStore } from "pinia"
import { hoardApi } from "@/api/hoardApi"

export interface PortfolioSummary {
    id: string
    name: string
    isActive: boolean
    createdUtc: string
}

export const usePortfolioStore = defineStore("portfolios", {
    state: () => ({
        loading: false,
        portfolios: [] as PortfolioSummary[]
    }),
    actions: {
        async loadPortfolios() {
            this.loading = true
            try {
                const response = await hoardApi.get<PortfolioSummary[]>("/portfolios")
                this.portfolios = response.data;
            } finally {
                this.loading = false
            }
        }
    }
})
