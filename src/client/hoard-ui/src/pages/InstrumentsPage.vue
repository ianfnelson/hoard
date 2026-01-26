<script setup lang="ts">
import { ref, watch } from "vue";
import { useInstruments } from "@/composables/useInstruments";
import { formatYesNo } from "@/utils/formatters";

const { items, totalCount, isLoading, fetchInstruments } = useInstruments();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'name', order: 'asc' }
]);

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
    sortDirection: sortBy.value[0]?.order
  };
  await fetchInstruments(params);
}

// Watch for changes and reload
watch([page, itemsPerPage, sortBy], () => {
  loadItems();
}, { immediate: true });
</script>

<template>
  <v-container fluid>
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
            No transactions found
          </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
