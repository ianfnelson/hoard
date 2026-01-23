<script setup lang="ts">
import { computed } from "vue";

import { usePortfolioStore } from "@/stores/portfolioStore";
import SummaryCard from "@/components/SummaryCard.vue";
import { formatCurrency, formatPercentage, getTrendClass } from "@/utils/formatters";

const store = usePortfolioStore();

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

    <!-- Summary strip -->
    <v-row dense class="mt-0">
      <v-col cols="6" sm="3">
        <summary-card
          title="Portfolio value"
          :value="store.portfolio?.performance?.value"
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Cash"
          :value="store.portfolio?.performance?.cashValue"
          :percentage="store.portfolio?.performance?.cashPercentage"
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Day change"
          :value="store.portfolio?.performance?.valueChange"
          :percentage="store.portfolio?.performance?.return1D ?? undefined"
          :trend="store.portfolio?.performance?.valueChange"
        />
      </v-col>

      <v-col cols="6" sm="3">
        <summary-card
          title="Year change"
          :value="store.portfolio?.performance?.valueChange1Y ?? undefined"
          :percentage="store.portfolio?.performance?.return1Y ?? undefined"
          :trend="store.portfolio?.performance?.valueChange1Y ?? undefined"
        />
      </v-col>
    </v-row>

    <!-- Positions table -->
    <v-row dense>
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
            <span>{{ formatCurrency(value) }}</span>
          </template>

          <template #item.valueChange="{ value }">
            <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
          </template>

          <template #item.return1D="{ value }">
            <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
          </template>

          <template #item.profit="{ value }">
            <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
          </template>

          <template #item.returnAllTime="{ value }">
            <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
          </template>

          <template #item.annualisedReturn="{ value }">
            <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
          </template>

          <template #item.portfolioPercentage="{ value }">
            <span>{{ formatPercentage(value) }}</span>
          </template>

          <template #no-data>
            No open positions
          </template>
        </v-data-table>
      </v-col>
    </v-row>
  </v-container>
</template>
