import { defineStore } from "pinia";
import { ref, computed } from "vue";
import * as signalR from "@microsoft/signalr";
import {
  getPortfolioDetail,
  getPortfolioPositions,
  getPortfolioExposure,
  getPortfolioInstrumentTypes
} from "@/api/portfoliosApi";
import type { PortfolioDetailDto } from "@/api/dtos/PortfolioDetailDto.ts";
import type { PortfolioPositionsDto } from "@/api/dtos/PortfolioPositionsDto.ts";
import type { PortfolioExposureDto } from "@/api/dtos/PortfolioExposureDto";
import type { PortfolioInstrumentTypesDto } from "@/api/dtos/PortfolioInstrumentTypesDto";

export const usePortfolioStore = defineStore("portfolio", () => {
  const portfolioId = ref<number | null>(null);

  const portfolio = ref<PortfolioDetailDto | null>(null);
  const positions = ref<PortfolioPositionsDto | null>(null);
  const exposure = ref<PortfolioExposureDto | null>(null);
  const instrumentTypes = ref<PortfolioInstrumentTypesDto | null>(null);

  const openPositionsList = computed(() =>
    (positions.value?.positions ?? []).filter(p => p.closeDate === null)
  );

  const isLoading = ref(false)
  const lastUpdated = ref<Date | null>(null)
  const error = ref<string | null>(null)

  const hubConnection = ref<signalR.HubConnection | null>(null)
  const hubStarted = ref(false)

  async function ensureSignalR() {
    if (hubConnection.value && hubStarted.value) {
      return
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/portfolio")
      .withAutomaticReconnect()
      .build()

    connection.on("PortfolioUpdated", async (payload: { portfolioId: number }) => {
      if (payload.portfolioId === portfolioId.value) {
        await refresh()
      }
    })

    connection.onreconnected(async () => {
      if (portfolioId.value !== null) {
        try {
          await connection.invoke("SubscribeToPortfolio", portfolioId.value)
        } catch {
          // swallow reconnect edge cases
        }
      }
    })

    await connection.start()

    hubConnection.value = connection
    hubStarted.value = true
  }

  async function unsubscribe() {
    if (hubConnection.value && hubStarted.value && portfolioId.value !== null) {
      try {
        await hubConnection.value.invoke("UnsubscribeFromPortfolio", portfolioId.value)
      } catch {
        // ignore
      }
    }
  }

  async function load(id: number) {
    if (portfolioId.value !== id) {
      await unsubscribe()
      reset()
      portfolioId.value = id
    }

    await ensureSignalR()

    try {
      await hubConnection.value?.invoke("SubscribeToPortfolio", id)
    } catch {
      // connection race / reconnect – ignore
    }

    await refresh()
  }

  async function refresh() {
    if (!portfolioId.value) return

    isLoading.value = true
    error.value = null

    try {
      const [portfolioResult, positionsResult, exposureResult, instrumentTypesResult] = await Promise.all([
        getPortfolioDetail(portfolioId.value),
        getPortfolioPositions(portfolioId.value),
        getPortfolioExposure(portfolioId.value),
        getPortfolioInstrumentTypes(portfolioId.value)
      ])

      portfolio.value = portfolioResult
      positions.value = positionsResult
      exposure.value = exposureResult
      instrumentTypes.value = instrumentTypesResult
      lastUpdated.value = new Date()
    } catch (e: any) {
      error.value = "Failed to load portfolio data";
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    portfolioId.value = null;
    portfolio.value = null;
    positions.value = null;
    exposure.value = null;
    instrumentTypes.value = null;
    lastUpdated.value = null;
    error.value = null;
  }

  return {
    // identity
    portfolioId,

    // raw state
    portfolio,
    exposure,
    instrumentTypes,

    // derived state
    openPositionsList,

    // lifecycle
    isLoading,
    lastUpdated,
    error,

    // actions
    load
  };
});
