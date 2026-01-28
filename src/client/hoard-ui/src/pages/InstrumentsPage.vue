<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useInstruments } from "@/composables/useInstruments";
import { useReferenceDataStore } from "@/stores/referenceDataStore";
import { formatYesNo } from "@/utils/formatters";

const router = useRouter();
const route = useRoute();
const { items, totalCount, isLoading, fetchInstruments } = useInstruments();
const refStore = useReferenceDataStore();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'name', order: 'asc' }
]);

// Filter state - initialize from query params
const initialSearch = (route.query.search as string) || "";
const initialInstrumentTypeId = route.query.instrumentTypeId ? Number(route.query.instrumentTypeId) : null;
const initialAssetClassId = route.query.assetClassId ? Number(route.query.assetClassId) : null;
const initialAssetSubclassId = route.query.assetSubclassId ? Number(route.query.assetSubclassId) : null;
const initialPriceUpdates = route.query.priceUpdates === "true" ? true : route.query.priceUpdates === "false" ? false : null;
const initialNewsUpdates = route.query.newsUpdates === "true" ? true : route.query.newsUpdates === "false" ? false : null;

const selectedInstrumentTypeId = ref<number | null>(initialInstrumentTypeId);
const selectedAssetClassId = ref<number | null>(initialAssetClassId);
const selectedAssetSubclassId = ref<number | null>(initialAssetSubclassId);
const enablePriceUpdates = ref<boolean | null>(initialPriceUpdates);
const enableNewsUpdates = ref<boolean | null>(initialNewsUpdates);
const searchText = ref(initialSearch);

const booleanOptions = [
  { title: "Yes", value: true },
  { title: "No", value: false },
];
const debouncedSearch = ref(initialSearch);

const availableSubclasses = computed(() => {
  if (!selectedAssetClassId.value) return [];
  const ac = refStore.assetClasses.find(c => c.id === selectedAssetClassId.value);
  return ac?.subclasses ?? [];
});

// Helper to find parent asset class from subclass ID
function findParentAssetClassId(subclassId: number): number | null {
  const parent = refStore.assetClasses.find(ac =>
    ac.subclasses.some(s => s.id === subclassId)
  );
  return parent?.id ?? null;
}

// Auto-resolve parent asset class when subclass is provided in URL without asset class
watch(
  () => refStore.assetClasses,
  (classes) => {
    if (classes.length > 0 && initialAssetSubclassId && !selectedAssetClassId.value) {
      const parentId = findParentAssetClassId(initialAssetSubclassId);
      if (parentId) {
        selectedAssetClassId.value = parentId;
        // Re-set the subclass after asset class is set (since the watcher below clears it)
        selectedAssetSubclassId.value = initialAssetSubclassId;
      }
    }
  },
  { immediate: true }
);

watch(selectedAssetClassId, (_newVal, oldVal) => {
  // Only clear subclass when user manually changes asset class, not on initial auto-resolve
  if (oldVal !== null) {
    selectedAssetSubclassId.value = null;
  }
});

let debounceTimer: ReturnType<typeof setTimeout> | undefined;
watch(searchText, (val) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = val;
  }, 400);
});

const headers = [
  { title: "Ticker", key: "tickerDisplay", sortable: true, width: '160px' },
  { title: "Name", key: "name", sortable: true },
  { title: "Instrument Type", key: "instrumentTypeName", sortable: false },
  { title: "Asset Class", key: "assetClassName", sortable: true },
  { title: "Asset Subclass", key: "assetSubclassName", sortable: false },
  { title: "Prices", key: "enablePriceUpdates", sortable: false},
  { title: "News", key: "enableNewsUpdates", sortable: false },
] as const;

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order,
    instrumentTypeId: selectedInstrumentTypeId.value ?? undefined,
    assetClassId: selectedAssetClassId.value ?? undefined,
    assetSubclassId: selectedAssetSubclassId.value ?? undefined,
    enablePriceUpdates: enablePriceUpdates.value ?? undefined,
    enableNewsUpdates: enableNewsUpdates.value ?? undefined,
    search: debouncedSearch.value || undefined,
  };
  await fetchInstruments(params);
}

// Reset page when filters change
watch([selectedInstrumentTypeId, selectedAssetClassId, selectedAssetSubclassId, enablePriceUpdates, enableNewsUpdates, debouncedSearch], () => {
  page.value = 1;
});

// Watch filter changes and update URL
watch(
  [searchText, selectedInstrumentTypeId, selectedAssetClassId, selectedAssetSubclassId, enablePriceUpdates, enableNewsUpdates],
  () => {
    const query: Record<string, string> = {};
    if (searchText.value) query.search = searchText.value;
    if (selectedInstrumentTypeId.value) query.instrumentTypeId = String(selectedInstrumentTypeId.value);
    if (selectedAssetClassId.value) query.assetClassId = String(selectedAssetClassId.value);
    if (selectedAssetSubclassId.value) query.assetSubclassId = String(selectedAssetSubclassId.value);
    if (enablePriceUpdates.value !== null) query.priceUpdates = String(enablePriceUpdates.value);
    if (enableNewsUpdates.value !== null) query.newsUpdates = String(enableNewsUpdates.value);

    router.replace({ query });
  }
);

// Watch for changes and reload
watch([page, itemsPerPage, sortBy, selectedInstrumentTypeId, selectedAssetClassId, selectedAssetSubclassId, enablePriceUpdates, enableNewsUpdates, debouncedSearch], () => {
  loadItems();
}, { immediate: true });

onMounted(() => {
  refStore.loadInstrumentTypes();
  refStore.loadAssetClasses();
});
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="6" md="2">
        <v-text-field
          v-model="searchText"
          label="Search"
          prepend-inner-icon="mdi-magnify"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedInstrumentTypeId"
          :items="refStore.instrumentTypes"
          item-title="name"
          item-value="id"
          label="Instrument Type"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedAssetClassId"
          :items="refStore.assetClasses"
          item-title="name"
          item-value="id"
          label="Asset Class"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedAssetSubclassId"
          :items="availableSubclasses"
          item-title="name"
          item-value="id"
          label="Asset Subclass"
          clearable
          hide-details
          density="compact"
          variant="outlined"
          :disabled="!selectedAssetClassId"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="enablePriceUpdates"
          :items="booleanOptions"
          label="Price Updates"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="enableNewsUpdates"
          :items="booleanOptions"
          label="News Updates"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
    </v-row>
    <v-row dense>
      <v-col>
        <v-data-table-server
          :headers="headers"
          :items="items"
          :items-length="totalCount"
          :loading="isLoading"
          v-model:page="page"
          v-model:items-per-page="itemsPerPage"
          v-model:sort-by="sortBy"
          :items-per-page-options="[
            { value: 10, title: '10' },
            { value: 15, title: '15' },
            { value: 25, title: '25' },
            { value: 50, title: '50' },
            { value: 100, title: '100' },
            { value: -1, title: 'All' }
          ]"
          density="compact"
        >
          <template #item.enablePriceUpdates="{ value }">
            {{ formatYesNo(value) }}
          </template>

          <template #item.enableNewsUpdates="{ value }">
            {{ formatYesNo(value) }}
          </template>

          <template #no-data>
            No instruments found
          </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
