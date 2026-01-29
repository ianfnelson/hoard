<script setup lang="ts">
import { computed } from 'vue'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { formatCurrency, formatPercentage, getTrendClass, formatDate } from '@/utils/formatters'
import type { PortfolioPositionDto } from '@/api/dtos/PortfolioPositionDto'

const store = usePortfolioStore()

const allPositions = computed(() => store.positions?.positions ?? [])

const headers = [
  { title: 'Ticker', key: 'instrumentTicker', sortable: true },
  { title: 'Instrument Name', key: 'instrumentName', sortable: true },
  { title: 'Open Date', key: 'openDate', sortable: true },
  { title: 'Close Date', key: 'closeDate', sortable: true },
  { title: 'Profit', key: 'profit', sortable: true, align: 'end' },
  { title: 'Return', key: 'return', sortable: true, align: 'end' },
] as const

const rows = computed(() =>
  allPositions.value.map((p: PortfolioPositionDto) => ({
    instrumentTicker: p.instrumentTicker,
    instrumentName: p.instrumentName,
    openDate: p.openDate,
    closeDate: p.closeDate,
    profit: p.performance?.profit,
    return: p.performance?.returnAllTime,
  }))
)
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-data-table
          :headers="headers"
          :items="rows"
          :items-per-page="15"
          :loading="store.isLoading"
          density="compact"
          :items-per-page-options="[
            { value: 10, title: '10' },
            { value: 15, title: '15' },
            { value: 25, title: '25' },
            { value: 50, title: '50' },
            { value: 100, title: '100' },
            { value: -1, title: 'All' },
          ]"
          :sort-by="[{ key: 'openDate', order: 'desc' }]"
        >
          <template #item.openDate="{ value }">
            <span>{{ formatDate(value) }}</span>
          </template>

          <template #item.closeDate="{ value }">
            <span>{{ formatDate(value) }}</span>
          </template>

          <template #item.profit="{ value }">
            <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
          </template>

          <template #item.return="{ value }">
            <span :class="getTrendClass(value)">{{ formatPercentage(value) }}</span>
          </template>

          <template #no-data> No positions </template>
        </v-data-table>
      </v-col>
    </v-row>
  </v-container>
</template>
