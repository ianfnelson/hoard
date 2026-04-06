<script setup lang="ts">
import { computed, defineAsyncComponent } from 'vue'

import { usePortfolioStore } from '@/stores/portfolioStore'
import { usePageTitle } from '@/composables/usePageTitle'
import ApiErrorAlert from '@/components/ApiErrorAlert.vue'
import { formatCurrency, formatPercentage, getTrendClass } from '@/utils/formatters'
import type { PieChartData } from '@/components/charts/ExposurePieChart.vue'

const ExposurePieChart = defineAsyncComponent(
  () => import('@/components/charts/ExposurePieChart.vue')
)

const store = usePortfolioStore()

const pageTitle = computed(() => {
  const portfolioName = store.portfolio?.name
  return portfolioName ? `${portfolioName} : Exposure` : null
})
usePageTitle(pageTitle)

const instrumentTypesHeaders = [
  { title: '', key: 'colour', width: '40px', sortable: false },
  { title: 'Name', key: 'name' },
  { title: 'Value', key: 'value', align: 'end' },
  { title: '%', key: 'percentage', align: 'end' },
] as const

const assetClassHeaders = [
  { title: '', key: 'colour', width: '40px', sortable: false },
  { title: 'Name', key: 'name' },
  { title: 'Target %', key: 'targetPercentage', align: 'end' },
  { title: 'Actual %', key: 'percentage', align: 'end' },
  { title: 'Actual Value', key: 'value', align: 'end' },
  { title: 'Deviation', key: 'deviationValue', align: 'end' },
] as const

const assetSubclassHeaders = [
  { title: '', key: 'colour', width: '40px', sortable: false },
  { title: 'Name', key: 'name' },
  { title: 'Target %', key: 'targetPercentage', align: 'end' },
  { title: 'Actual %', key: 'percentage', align: 'end' },
  { title: 'Actual Value', key: 'value', align: 'end' },
  { title: 'Deviation', key: 'deviationValue', align: 'end' },
] as const

const rebalanceHeaders = [
  { title: '', key: 'colour', width: '40px', sortable: false },
  { title: 'Action', key: 'action' },
  { title: 'Subclass', key: 'assetSubclassName' },
  { title: 'Value', key: 'amount', align: 'end' },
] as const

const instrumentTypesRows = computed(() => store.instrumentTypes?.instrumentTypes ?? [])

const assetClassRows = computed(() => store.exposure?.assetClasses ?? [])

const assetSubclassRows = computed(() => store.exposure?.assetSubclasses ?? [])

const rebalanceRows = computed(() =>
  (store.exposure?.rebalanceActions ?? []).map((r) => ({
    colour: r.colour,
    action: r.rebalanceAction,
    assetSubclassName: r.assetSubclassName,
    amount: r.amount,
  }))
)

const assetClassPieData = computed<PieChartData[]>(() =>
  (store.exposure?.assetClasses ?? []).map((ac) => ({
    name: ac.name,
    colour: ac.colour,
    percentage: ac.percentage,
  }))
)

const assetSubclassPieData = computed<PieChartData[]>(() =>
  (store.exposure?.assetSubclasses ?? []).map((asc) => ({
    name: asc.name,
    colour: asc.colour,
    percentage: asc.percentage,
  }))
)

const instrumentTypePieData = computed<PieChartData[]>(() =>
  (store.instrumentTypes?.instrumentTypes ?? []).map((it) => ({
    name: it.name,
    colour: it.colour,
    percentage: it.percentage,
  }))
)
</script>

<template>
  <v-container fluid>
    <ApiErrorAlert :error="store.error" />

    <!-- Asset classes row -->
    <v-row dense>
      <v-col cols="12" md="6">
        <v-card class="h-100">
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
              <template #item.colour="{ value }">
                <v-icon :color="value" size="small">mdi-circle</v-icon>
              </template>

              <template #item.targetPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.percentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.value="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.deviationValue="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data>
                No asset class exposure data. Ensure positions have asset classes assigned.
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6">
        <v-card class="h-100">
          <v-card-title>Asset Classes</v-card-title>
          <v-card-text>
            <ExposurePieChart :data="assetClassPieData" :is-loading="store.isLoading" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Asset subclasses row -->
    <v-row dense>
      <v-col cols="12" md="6">
        <v-card class="h-100">
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
              <template #item.colour="{ value }">
                <v-icon :color="value" size="small">mdi-circle</v-icon>
              </template>

              <template #item.targetPercentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.percentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #item.value="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.deviationValue="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data>
                No asset subclass exposure data. Ensure positions have asset subclasses assigned.
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6">
        <v-card class="h-100">
          <v-card-title>Asset Subclasses</v-card-title>
          <v-card-text>
            <ExposurePieChart :data="assetSubclassPieData" :is-loading="store.isLoading" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Instrument types row -->
    <v-row dense>
      <v-col cols="12" md="6">
        <v-card class="h-100">
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
              <template #item.colour="{ value }">
                <v-icon :color="value" size="small">mdi-circle</v-icon>
              </template>

              <template #item.value="{ value }">
                <span>{{ formatCurrency(value) }}</span>
              </template>

              <template #item.percentage="{ value }">
                <span>{{ formatPercentage(value) }}</span>
              </template>

              <template #no-data>
                No instrument type exposure data. Ensure positions have instrument types assigned.
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6">
        <v-card class="h-100">
          <v-card-title>Instrument Types</v-card-title>
          <v-card-text>
            <ExposurePieChart :data="instrumentTypePieData" :is-loading="store.isLoading" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Provisional rebalancing row -->
    <v-row v-if="rebalanceRows.length > 0" dense>
      <v-col cols="12" md="6">
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
              <template #item.colour="{ value }">
                <v-icon :color="value" size="small">mdi-circle</v-icon>
              </template>

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
