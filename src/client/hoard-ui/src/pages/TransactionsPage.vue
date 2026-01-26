<script setup lang="ts">
import { ref, watch } from "vue";
import { useTransactions } from "@/composables/useTransactions";
import {formatCurrency, formatDate, getTrendClass} from "@/utils/formatters";

const { items, totalCount, isLoading, fetchTransactions } = useTransactions();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'date', order: 'desc' }
]);

const headers = [
  { title: "Date", key: "date", sortable: true, width: '160px' },
  { title: "Account", key: "accountName", sortable: false },
  { title: "Type", key: "transactionTypeName", sortable: false },
  { title: "Ticker", key: "instrumentTicker", sortable: true },
  { title: "Instrument Name", key: "instrumentName", sortable: true },
  { title: "Contract Note", key: "contractNoteReference", sortable: false },
  { title: "Units", key: "units", sortable: false, align: 'end' },
  { title: "Value", key: "value", sortable: true, align: 'end' },
] as const;

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order
  };
  await fetchTransactions(params);
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
          <template #item.date="{ value }">
            <span style="white-space: nowrap;">{{ formatDate(value) }}</span>
          </template>

          <template #item.value="{ value }">
            <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
          </template>

          <template #no-data>
            No transactions found
          </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
