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
  { title: "Name", key: "instrumentName"},
  { title: "Value", key: "value" },
  { title: "%", key: "portfolioPercentage"}
] as const;

// Shape rows for the table (keep template dumb)
const rows = computed(() =>
  store.openPositions.map(p => ({
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    value: p.performance?.value ?? 0,
    portfolioPercentage: p.portfolioPercentage
  }))
);
</script>

<template>
  <v-container>
    <v-card>
      <v-card-title>
        Portfolio positions
      </v-card-title>

      <v-card-text>
        <v-data-table
          :headers="header_data"
          :items="rows"
          :loading="store.isLoading"
          density="compact"
        >
          <template #item.value="{ value }">
            {{ value.toFixed(2) }}
          </template>

          <template #item.portfolioPercentage="{ value }">
            {{ value.toFixed(2) }}%
          </template>

          <template #no-data>
            No open positions
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>
</template>
