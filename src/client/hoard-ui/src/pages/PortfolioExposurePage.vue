<script setup lang="ts">
import { computed } from 'vue'

import { usePortfolioStore } from '@/stores/portfolioStore'
import { formatCurrency, formatPercentage, getTrendClass } from '@/utils/formatters'

const store = usePortfolioStore()

const instrumentTypesHeaders = [
  { title: 'Name', key: 'name' },
  { title: 'Value', key: 'value', align: 'end' },
  { title: '%', key: 'percentage', align: 'end' },
] as const

const assetClassHeaders = [
  { title: 'Name', key: 'assetClassName' },
  { title: 'Target %', key: 'targetPercentage', align: 'end' },
  { title: 'Actual %', key: 'actualPercentage', align: 'end' },
  { title: 'Actual Value', key: 'actualValue', align: 'end' },
  { title: 'Deviation', key: 'deviationValue', align: 'end' },
] as const

const assetSubclassHeaders = [
  { title: 'Name', key: 'assetSubclassName' },
  { title: 'Target %', key: 'targetPercentage', align: 'end' },
  { title: 'Actual %', key: 'actualPercentage', align: 'end' },
  { title: 'Actual Value', key: 'actualValue', align: 'end' },
  { title: 'Deviation', key: 'deviationValue', align: 'end' },
] as const

const rebalanceHeaders = [
  { title: 'Action', key: 'action' },
  { title: 'Subclass', key: 'assetSubclassName' },
  { title: 'Value', key: 'amount', align: 'end' },
] as const

const instrumentTypesRows = computed(() => store.instrumentTypes?.instrumentTypes ?? [])

const assetClassRows = computed(() => store.exposure?.assetClasses ?? [])

const assetSubclassRows = computed(() => store.exposure?.assetSubclasses ?? [])

const rebalanceRows = computed(() =>
  (store.exposure?.rebalanceActions ?? []).map((r) => ({
    action: r.rebalanceAction === 1 ? 'Sell' : 'Buy',
    assetSubclassName: r.assetSubclassName,
    amount: r.amount,
  }))
)
</script>

<template>
  <v-container fluid>
    <v-row dense class="mt-0">
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>Asset Subclasses</v-card-title>
          <v-card-text>
            <v-data-table
              :headers="assetSubclassHeaders"
              :items="assetSubclassRows"
              :items-per-page="-1"
              :loading="store.isLoading"
              density="compact"
              hide-default-footer
            >
              <template #item.targetPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.actualPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.actualValue="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.deviationValue="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data> No asset subclasses </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>Asset Classes</v-card-title>
          <v-card-text>
            <v-data-table
              :headers="assetClassHeaders"
              :items="assetClassRows"
              :items-per-page="-1"
              :loading="store.isLoading"
              density="compact"
              hide-default-footer
            >
              <template #item.targetPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.actualPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.actualValue="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.deviationValue="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data> No asset classes </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>Instrument Types</v-card-title>
          <v-card-text>
            <v-data-table
              :headers="instrumentTypesHeaders"
              :items="instrumentTypesRows"
              :items-per-page="-1"
              :loading="store.isLoading"
              density="compact"
              hide-default-footer
            >
              <template #item.value="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.percentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #no-data> No instrument types </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col v-if="rebalanceRows.length > 0" cols="12" md="6">
        <v-card>
          <v-card-title>Provisional Rebalancing Trades</v-card-title>
          <v-card-text>
            <v-data-table
              :headers="rebalanceHeaders"
              :items="rebalanceRows"
              :items-per-page="-1"
              :loading="store.isLoading"
              density="compact"
              hide-default-footer
            >
              <template #item.amount="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data> No rebalancing actions </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
