import { defineStore } from "pinia";
import { ref } from "vue";
import { getInstrumentTypes, getAssetClasses } from "@/api/referencesApi";
import type { InstrumentTypeDto } from "@/api/dtos/InstrumentTypeDto";
import type { AssetClassDto } from "@/api/dtos/AssetClassDto";

export const useReferenceDataStore = defineStore("referenceData", () => {
  const instrumentTypes = ref<InstrumentTypeDto[]>([]);
  const assetClasses = ref<AssetClassDto[]>([]);
  const isLoading = ref(false);
  const lastUpdated = ref<Date | null>(null);
  const assetClassesLoaded = ref(false);
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

  async function refresh() {
    lastUpdated.value = null;
    assetClassesLoaded.value = false;
    await Promise.all([loadInstrumentTypes(), loadAssetClasses()]);
  }

  return {
    instrumentTypes,
    assetClasses,
    isLoading,
    lastUpdated,
    error,
    loadInstrumentTypes,
    loadAssetClasses,
    refresh
  };
});
