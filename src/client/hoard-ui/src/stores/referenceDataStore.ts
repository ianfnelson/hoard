import { defineStore } from "pinia";
import { ref } from "vue";
import { getInstrumentTypes, getAssetClasses, getTransactionTypes } from "@/api/referencesApi";
import type { InstrumentTypeDto } from "@/api/dtos/InstrumentTypeDto";
import type { AssetClassDto } from "@/api/dtos/AssetClassDto";
import type { TransactionTypeDto } from "@/api/dtos/TransactionTypeDto";
import { getInstrumentsLookup } from "@/api/instrumentsApi";
import type { LookupDto } from "@/api/dtos/LookupDto";

export const useReferenceDataStore = defineStore("referenceData", () => {
  const instrumentTypes = ref<InstrumentTypeDto[]>([]);
  const assetClasses = ref<AssetClassDto[]>([]);
  const transactionTypes = ref<TransactionTypeDto[]>([]);
  const instruments = ref<LookupDto[]>([]);
  const isLoading = ref(false);
  const lastUpdated = ref<Date | null>(null);
  const assetClassesLoaded = ref(false);
  const transactionTypesLoaded = ref(false);
  const instrumentsLoaded = ref(false);
  const error = ref<string | null>(null);

  async function loadInstrumentTypes() {
    if (lastUpdated.value) return;

    isLoading.value = true;
    error.value = null;
    try {
      instrumentTypes.value = await getInstrumentTypes();
      lastUpdated.value = new Date();
    } catch (e: any) {
      error.value = "Failed to load instrument types";
      console.error("Failed to load instrument types:", e);
      instrumentTypes.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  async function loadAssetClasses() {
    if (assetClassesLoaded.value) return;

    isLoading.value = true;
    error.value = null;
    try {
      assetClasses.value = await getAssetClasses();
      assetClassesLoaded.value = true;
    } catch (e: any) {
      error.value = "Failed to load asset classes";
      console.error("Failed to load asset classes:", e);
      assetClasses.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  async function loadTransactionTypes() {
    if (transactionTypesLoaded.value) return;

    isLoading.value = true;
    error.value = null;
    try {
      transactionTypes.value = await getTransactionTypes();
      transactionTypesLoaded.value = true;
    } catch (e: any) {
      error.value = "Failed to load transaction types";
      console.error("Failed to load transaction types:", e);
      transactionTypes.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  async function loadInstruments() {
    if (instrumentsLoaded.value) return;

    isLoading.value = true;
    error.value = null;
    try {
      instruments.value = await getInstrumentsLookup();
      instrumentsLoaded.value = true;
    } catch (e: any) {
      error.value = "Failed to load instruments";
      console.error("Failed to load instruments:", e);
      instruments.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  async function refresh() {
    lastUpdated.value = null;
    assetClassesLoaded.value = false;
    transactionTypesLoaded.value = false;
    instrumentsLoaded.value = false;
    await Promise.all([
      loadInstrumentTypes(),
      loadAssetClasses(),
      loadTransactionTypes(),
      loadInstruments()
    ]);
  }

  return {
    instrumentTypes,
    assetClasses,
    transactionTypes,
    instruments,
    isLoading,
    lastUpdated,
    error,
    loadInstrumentTypes,
    loadAssetClasses,
    loadTransactionTypes,
    loadInstruments,
    refresh
  };
});
