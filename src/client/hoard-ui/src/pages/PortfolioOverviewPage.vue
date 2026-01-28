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

// Check if a date string is "today" in UTC
function isToday(dateString: string | null | undefined): boolean {
  if (!dateString) return false;
  const date = new Date(dateString);
  const today = new Date();
  return date.getUTCFullYear() === today.getUTCFullYear()
    && date.getUTCMonth() === today.getUTCMonth()
    && date.getUTCDate() === today.getUTCDate();
}

// Shape rows for the table (keep template dumb)
const rows = computed(() =>
  store.openPositionsList.map(p => ({
    instrumentId: p.instrumentId,
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    latestNewsPublishedUtc: p.latestNewsPublishedUtc,
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
          title="Value / yield"
          :value="store.portfolio?.performance?.value"
          :percentage="store.portfolio?.performance?.yield"
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
          <template #item.instrumentTicker="{ item }">
            {{ item.instrumentTicker }}<template v-if="isToday(item.latestNewsPublishedUtc)">&nbsp;<router-link
                :to="{ name: 'news', query: { instrumentId: item.instrumentId } }"
              ><v-icon size="small" color="#B87333" style="position: relative; top: -2px">mdi-newspaper</v-icon></router-link></template>
          </template>

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
