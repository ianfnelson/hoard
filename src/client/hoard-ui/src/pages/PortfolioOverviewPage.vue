<script setup lang="ts">
import { onMounted, watch, computed } from "vue";
import { useRoute } from "vue-router";

import { usePortfolioOverviewStore } from "@/stores/portfolioOverviewStore";

const route = useRoute();
const store = usePortfolioOverviewStore();

function getPortfolioId(): number {
  const id = Number(route.params.id);
  if (Number.isNaN(id)) {
    throw new Error("Invalid portfolio id");
  }
  return id;
}

onMounted(() => {
  store.load(getPortfolioId());
});

watch(
  () => route.params.id,
  () => {
    store.load(getPortfolioId());
  }
);

const header_data = [
  { title: "Ticker", key: "instrumentTicker" },
  { title: "Value", key: "value", align: 'end' },
  { title: "Change", key: "valueChange", align: 'end'},
  { title: "1D", key: "return1D", align: 'end'},
  { title: "1Y", key: "return1Y", align: 'end'},
  { title: "Profit", key: "profit", align: 'end'},
  { title: "All", key: "returnAllTime", align: 'end'},
  { title: "Annual", key: "annualisedReturn", align: 'end'},
  { title: "%", key: "portfolioPercentage", align: 'end'}
] as const;

// Shape rows for the table (keep template dumb)
const rows = computed(() =>
  store.openPositions.map(p => ({
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    value: p.performance?.value,
    valueChange: p.performance?.valueChange,
    return1D: p.performance?.return1D,
    return1Y: p.performance?.return1Y,
    returnAllTime: p.performance?.returnAllTime,
    annualisedReturn: p.performance?.annualisedReturn,
    portfolioPercentage: p.portfolioPercentage,
    profit: p.performance ? p.performance?.unrealisedGain + p.performance?.realisedGain + p.performance?.income : null
  }))
);
</script>

<template>
  <v-container>
    <v-card :loading="store.isLoading">
      <v-card-title>
        {{ store.portfolio?.name || '' }}
      </v-card-title>

      <v-card-text>
        <v-data-table
          :headers="header_data"
          :items="rows"
          :items-per-page="-1"
          :loading="store.isLoading"
          density="compact"
          :sort-by="[{ key: 'value', order: 'desc' }]"
          hide-default-footer
        >
          <template #item.value="{ value }">
            {{ value.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
          </template>

          <template #item.valueChange="{ value }">
            {{ value.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
          </template>

          <template #item.return1D="{ value }">
            {{ value.toFixed(2) || '' }}
          </template>

          <template #item.return1Y="{ value }">
            {{ value?.toFixed(2) || '' }}
          </template>

          <template #item.profit="{ value }">
            {{ value.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
          </template>

          <template #item.returnAllTime="{ value }">
            {{ value?.toFixed(2) || '' }}
          </template>

          <template #item.annualisedReturn="{ value }">
            {{ value?.toFixed(2) || '' }}
          </template>

          <template #item.portfolioPercentage="{ value }">
            {{ value.toFixed(2) }}
          </template>

          <template #no-data>
            No open positions
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>
</template>
