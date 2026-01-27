<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { useInstruments } from "@/composables/useInstruments";
import { useReferenceDataStore } from "@/stores/referenceDataStore";
import { formatYesNo } from "@/utils/formatters";

const { items, totalCount, isLoading, fetchInstruments } = useInstruments();
const refStore = useReferenceDataStore();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'name', order: 'asc' }
]);

const selectedInstrumentTypeId = ref<number | null>(null);
const selectedAssetClassId = ref<number | null>(null);
const selectedAssetSubclassId = ref<number | null>(null);
const enablePriceUpdates = ref<boolean | null>(null);
const enableNewsUpdates = ref<boolean | null>(null);
const searchText = ref("");

const booleanOptions = [
  { title: "Yes", value: true },
  { title: "No", value: false },
];
const debouncedSearch = ref("");

const availableSubclasses = computed(() => {
  if (!selectedAssetClassId.value) return [];
  const ac = refStore.assetClasses.find(c => c.id === selectedAssetClassId.value);
  return ac?.subclasses ?? [];
});

watch(selectedAssetClassId, () => {
  selectedAssetSubclassId.value = null;
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
