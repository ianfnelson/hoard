<script setup lang="ts">
import { computed } from "vue";
import { usePortfolioStore } from "@/stores/portfolioStore";
import { formatCurrency, formatPercentage, getTrendClass } from "@/utils/formatters";

const store = usePortfolioStore();

const allPositions = computed(() => store.positions?.positions ?? []);

const headers = [
  { title: "Ticker", key: "instrumentTicker" },
  { title: "Open Date", key: "openDate" },
  { title: "Close Date", key: "closeDate" },
  { title: "Status", key: "status" },
  { title: "Value", key: "value", align: 'end' },
  { title: "Return", key: "return", align: 'end' }
] as const;

const rows = computed(() =>
  allPositions.value.map(p => ({
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    openDate: p.openDate ? new Date(p.openDate).toLocaleDateString() : '-',
    closeDate: p.closeDate ? new Date(p.closeDate).toLocaleDateString() : '-',
    status: p.closeDate ? 'Closed' : 'Open',
    value: p.performance?.value,
    return: p.performance?.returnAllTime
  }))
);
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-data-table
          :headers="headers"
          :items="rows"
          :items-per-page="-1"
          :loading="store.isLoading"
          density="compact"
          :sort-by="[{ key: 'openDate', order: 'desc' }]"
          hide-default-footer
        >
          <template #item.value="{ value }">
            <span>{{ formatCurrency(value) }}</span>
          </template>

          <template #item.return="{ value }">
            <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
          </template>

          <template #no-data>
            No positions
          </template>
        </v-data-table>
      </v-col>
    </v-row>
  </v-container>
</template>
