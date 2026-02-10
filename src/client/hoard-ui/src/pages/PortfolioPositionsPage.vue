<script lang="ts" setup>
import { computed, ref, watch } from 'vue'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatCurrency, formatPercentage, getTrendClass, formatDate } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'

const store = usePortfolioStore()

const pageTitle = computed(() => {
  const portfolioName = store.portfolio?.name
  return portfolioName ? `${portfolioName} : Positions` : null
})
usePageTitle(pageTitle)

const allPositions = computed(() => store.positions?.positions ?? [])

type ViewTab = 'holdings' | 'gains' | 'performance'
const activeTab = ref<ViewTab>('holdings')

// Filter state
const searchText = ref('')
const debouncedSearch = ref('')
const selectedStatus = ref<'all' | 'open' | 'closed'>('all')

// Shared state across all tabs
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'openDate', order: 'desc' },
])
const itemsPerPage = ref(15)

// Debounce search input
let debounceTimer: ReturnType<typeof setTimeout> | undefined
watch(searchText, (val) => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = val
  }, 400)
})

// Filtered positions based on search and status
const filteredPositions = computed(() => {
  let filtered = allPositions.value

  // Apply search filter
  if (debouncedSearch.value) {
    const searchLower = debouncedSearch.value.toLowerCase()
    filtered = filtered.filter(
      (p) =>
        p.instrumentName.toLowerCase().includes(searchLower) ||
        p.instrumentTicker.toLowerCase().includes(searchLower)
    )
  }

  // Apply status filter
  if (selectedStatus.value === 'open') {
    filtered = filtered.filter((p) => p.closeDate === null)
  } else if (selectedStatus.value === 'closed') {
    filtered = filtered.filter((p) => p.closeDate !== null)
  }

  return filtered
})

const statusItems = [
  { title: 'All', value: 'all' },
  { title: 'Open', value: 'open' },
  { title: 'Closed', value: 'closed' },
]

const holdingsHeaders = [
  { title: 'Instrument', key: 'instrumentName', sortable: true },
  { title: 'Open Date', key: 'openDate', sortable: true },
  { title: 'Close Date', key: 'closeDate', sortable: true },
  { title: 'Units', key: 'performance.units', sortable: false, align: 'end' },
  { title: 'Cost Basis', key: 'performance.costBasis', sortable: true, align: 'end' },
  { title: 'Value', key: 'performance.value', sortable: true, align: 'end' },
  { title: '%', key: 'portfolioPercentage', sortable: true, align: 'end' },
] as const

const gainsHeaders = [
  { title: 'Instrument', key: 'instrumentName', sortable: true },
  { title: 'Unrealised', key: 'performance.unrealisedGain', sortable: true, align: 'end' },
  { title: 'Realised', key: 'performance.realisedGain', sortable: true, align: 'end' },
  { title: 'Gain', key: 'performance.gain', sortable: true, align: 'end' },
  { title: 'Income', key: 'performance.income', sortable: true, align: 'end' },
  { title: 'Profit', key: 'performance.profit', sortable: true, align: 'end' },
] as const

const performanceHeaders = [
  { title: 'Instrument', key: 'instrumentName', sortable: true },
  { title: '1D', key: 'performance.return1D', sortable: true, align: 'end' },
  { title: '1W', key: 'performance.return1W', sortable: true, align: 'end' },
  { title: '1M', key: 'performance.return1M', sortable: true, align: 'end' },
  { title: '3M', key: 'performance.return3M', sortable: true, align: 'end' },
  { title: '6M', key: 'performance.return6M', sortable: true, align: 'end' },
  { title: '1Y', key: 'performance.return1Y', sortable: true, align: 'end' },
  { title: 'Ytd', key: 'performance.returnYtd', sortable: true, align: 'end' },
  { title: 'All', key: 'performance.returnAllTime', sortable: true, align: 'end' },
  { title: 'Annual', key: 'performance.annualisedReturn', sortable: true, align: 'end' },
] as const
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="searchText"
          label="Search"
          prepend-inner-icon="mdi-magnify"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-select
          v-model="selectedStatus"
          :items="statusItems"
          label="Status"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
    </v-row>

    <v-row dense>
      <v-col>
        <v-tabs v-model="activeTab" bg-color="transparent" class="mb-4" density="compact">
          <v-tab value="holdings">Holdings</v-tab>
          <v-tab value="gains">Gains</v-tab>
          <v-tab value="performance">Performance</v-tab>
        </v-tabs>

        <v-window v-model="activeTab">
          <v-window-item value="holdings">
            <v-data-table
              v-model:sort-by="sortBy"
              v-model:items-per-page="itemsPerPage"
              :headers="holdingsHeaders"
              :items="filteredPositions"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              :loading="store.isLoading"
              density="compact"
            >
              <template #item.instrumentName="{ item }">
                <router-link
                  v-if="item.instrumentId"
                  :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
                  class="text-decoration-none"
                >
                  {{ item.instrumentName }}
                </router-link>
              </template>
              <template #item.openDate="{ value }">
                <span>{{ formatDate(value) }}</span>
              </template>

              <template #item.closeDate="{ value }">
                <span>{{ formatDate(value) }}</span>
              </template>

              <template #item.performance.units="{ item }">
                {{ item.performance?.units ?? '-' }}
              </template>

              <template #item.performance.value="{ item }">
                {{ formatCurrency(item.performance?.value) }}
              </template>
              <template #item.performance.costBasis="{ item }">
                {{ formatCurrency(item.performance?.costBasis) }}
              </template>
              <template #item.portfolioPercentage="{ item }">
                {{ formatPercentage(item.portfolioPercentage) }}
              </template>

              <template #no-data> No positions</template>
            </v-data-table>
          </v-window-item>
          <v-window-item value="gains">
            <v-data-table
              v-model:sort-by="sortBy"
              v-model:items-per-page="itemsPerPage"
              :headers="gainsHeaders"
              :items="filteredPositions"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              :loading="store.isLoading"
              density="compact"
            >
              <template #item.instrumentName="{ item }">
                <router-link
                  v-if="item.instrumentId"
                  :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
                  class="text-decoration-none"
                >
                  {{ item.instrumentName }}
                </router-link>
              </template>

              <template #item.performance.unrealisedGain="{ item }">
                <span :class="getTrendClass(item.performance?.unrealisedGain)">{{
                  formatCurrency(item.performance?.unrealisedGain)
                }}</span>
              </template>

              <template #item.performance.realisedGain="{ item }">
                <span :class="getTrendClass(item.performance?.realisedGain)">{{
                  formatCurrency(item.performance?.realisedGain)
                }}</span>
              </template>

              <template #item.performance.gain="{ item }">
                <span :class="getTrendClass(item.performance?.gain)">{{
                  formatCurrency(item.performance?.gain)
                }}</span>
              </template>

              <template #item.performance.income="{ item }">
                <span :class="getTrendClass(item.performance?.income)">{{
                  formatCurrency(item.performance?.income)
                }}</span>
              </template>

              <template #item.performance.profit="{ item }">
                <span :class="getTrendClass(item.performance?.profit)">{{
                  formatCurrency(item.performance?.profit)
                }}</span>
              </template>

              <template #no-data> No positions</template>
            </v-data-table>
          </v-window-item>
          <v-window-item value="performance">
            <v-data-table
              v-model:sort-by="sortBy"
              v-model:items-per-page="itemsPerPage"
              :headers="performanceHeaders"
              :items="filteredPositions"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              :loading="store.isLoading"
              density="compact"
            >
              <template #item.instrumentName="{ item }">
                <router-link
                  v-if="item.instrumentId"
                  :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
                  class="text-decoration-none"
                >
                  {{ item.instrumentName }}
                </router-link>
              </template>

              <template #item.performance.return1D="{ item }">
                <span :class="getTrendClass(item.performance?.return1D)">{{
                  formatPercentage(item.performance?.return1D)
                }}</span>
              </template>

              <template #item.performance.return1W="{ item }">
                <span :class="getTrendClass(item.performance?.return1W)">{{
                  formatPercentage(item.performance?.return1W)
                }}</span>
              </template>

              <template #item.performance.return1M="{ item }">
                <span :class="getTrendClass(item.performance?.return1M)">{{
                  formatPercentage(item.performance?.return1M)
                }}</span>
              </template>

              <template #item.performance.return3M="{ item }">
                <span :class="getTrendClass(item.performance?.return3M)">{{
                  formatPercentage(item.performance?.return3M)
                }}</span>
              </template>

              <template #item.performance.return6M="{ item }">
                <span :class="getTrendClass(item.performance?.return6M)">{{
                  formatPercentage(item.performance?.return6M)
                }}</span>
              </template>

              <template #item.performance.return1Y="{ item }">
                <span :class="getTrendClass(item.performance?.return1Y)">{{
                  formatPercentage(item.performance?.return1Y)
                }}</span>
              </template>

              <template #item.performance.returnYtd="{ item }">
                <span :class="getTrendClass(item.performance?.returnYtd)">{{
                  formatPercentage(item.performance?.returnYtd)
                }}</span>
              </template>

              <template #item.performance.returnAllTime="{ item }">
                <span :class="getTrendClass(item.performance?.returnAllTime)">{{
                  formatPercentage(item.performance?.returnAllTime)
                }}</span>
              </template>

              <template #item.performance.annualisedReturn="{ item }">
                <span :class="getTrendClass(item.performance?.annualisedReturn)">{{
                  formatPercentage(item.performance?.annualisedReturn)
                }}</span>
              </template>

              <template #no-data> No positions</template>
            </v-data-table>
          </v-window-item>
        </v-window>
      </v-col>
    </v-row>
  </v-container>
</template>
