<script setup lang="ts">
import { onMounted, watch, computed } from "vue";
import { useRoute } from "vue-router";

import { usePortfolioOverviewStore } from "@/stores/portfolioOverviewStore";
import SummaryCard from "@/components/SummaryCard.vue";

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
  { title: "Profit", key: "profit", align: 'end'},
  { title: "All", key: "returnAllTime", align: 'end'},
  { title: "Annual", key: "annualisedReturn", align: 'end'},
  { title: "%", key: "portfolioPercentage", align: 'end'}
] as const;

// Shape rows for the table (keep template dumb)
const rows = computed(() =>
  store.openPositionsList.map(p => ({
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    value: p.performance?.value,
    valueChange: p.performance?.valueChange,
    return1D: p.performance?.return1D,
    returnAllTime: p.performance?.returnAllTime,
    annualisedReturn: p.performance?.annualisedReturn,
    portfolioPercentage: p.portfolioPercentage,
    profit: p.performance?.profit
  }))
);
</script>

<template>
  <v-container fluid>
    <!-- Portfolio header -->
    <v-row class="mb-2">
      <v-col>
        <v-toolbar density="compact">
          <v-toolbar-title class="text-h6">
            {{ store.portfolio?.name || '' }}
          </v-toolbar-title>
          <div class="text-caption pr-4">
            {{ store.portfolio?.performance?.updatedUtc?.toLocaleString() || '' }}
          </div>
        </v-toolbar>
      </v-col>
    </v-row>

    <!-- Summary strip -->
    <v-row dense class="mb-4">
      <v-col cols="6" sm="3">
        <summary-card
          title="Portfolio value"
          :value="store.portfolio?.performance?.value?.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || ''"
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Cash"
          :value="store.portfolio?.performance?.cashValue?.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || ''"
          :secondary="store.portfolio?.performance?.cashPercentage.toFixed(2) || '' "
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Day change"
          :value="store.portfolio?.performance?.valueChange?.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || ''"
          :secondary="store.portfolio?.performance?.return1D?.toFixed(2) || '' "
          :trend="store.portfolio?.performance?.valueChange"
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Year change"
          :value="(store.portfolio?.performance?.valueChange1Y ?? undefined)?.toLocaleString('en-GB', { style: 'currency', currency: 'GBP', minimumFractionDigits: 2, maximumFractionDigits: 2 }) || ''"
          :secondary="store.portfolio?.performance?.return1Y ? `${store.portfolio.performance.return1Y.toFixed(2)}%` : ''"
          :trend="store.portfolio?.performance?.valueChange1Y ?? undefined"
        />
      </v-col>
    </v-row>

    <!-- Positions table -->
    <v-row>
      <v-col>
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
      </v-col>
    </v-row>
  </v-container>
</template>
