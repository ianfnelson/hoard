<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTransactions } from '@/composables/useTransactions'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'
import { formatCurrency, formatDate, getTrendClass } from '@/utils/formatters'

const router = useRouter()
const route = useRoute()
const { items, totalCount, isLoading, fetchTransactions } = useTransactions()
const refStore = useReferenceDataStore()
const navStore = useNavigationStore()

const page = ref(1)
const itemsPerPage = ref(15)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([{ key: 'date', order: 'desc' }])

// Filter state - initialize from query params
const initialSearch = (route.query.search as string) || ''
const initialAccountId = route.query.accountId ? Number(route.query.accountId) : null
const initialTransactionTypeId = route.query.transactionTypeId
  ? Number(route.query.transactionTypeId)
  : null
const initialInstrumentId = route.query.instrumentId ? Number(route.query.instrumentId) : null
const initialFromDate = (route.query.fromDate as string) || ''
const initialToDate = (route.query.toDate as string) || ''

const selectedAccountId = ref<number | null>(initialAccountId)
const selectedInstrumentId = ref<number | null>(initialInstrumentId)
const selectedTransactionTypeId = ref<number | null>(initialTransactionTypeId)
const fromDate = ref<string>(initialFromDate)
const toDate = ref<string>(initialToDate)
const searchText = ref(initialSearch)

const debouncedSearch = ref(initialSearch)
let debounceTimer: ReturnType<typeof setTimeout> | undefined

watch(searchText, (val) => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = val
  }, 400)
})

const instrumentItems = computed(() => {
  return refStore.instruments.map((i) => ({
    title: i.context ? `${i.name} (${i.context})` : i.name,
    value: i.id,
  }))
})

const headers = [
  { title: 'Date', key: 'date', sortable: true, width: '160px' },
  { title: 'Account', key: 'accountName', sortable: false },
  { title: 'Type', key: 'transactionTypeName', sortable: false },
  { title: 'Units', key: 'units', sortable: false, align: 'end' },
  { title: 'Ticker', key: 'instrumentTicker', sortable: true },
  { title: 'Price', key: 'price', sortable: false, align: 'end' },
  { title: 'Value', key: 'value', sortable: true, align: 'end' },
  { title: 'Contract Note', key: 'contractNoteReference', sortable: false },
] as const

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order,
    accountId: selectedAccountId.value ?? undefined,
    instrumentId: selectedInstrumentId.value ?? undefined,
    transactionTypeId: selectedTransactionTypeId.value ?? undefined,
    fromDate: fromDate.value || undefined,
    toDate: toDate.value || undefined,
    search: debouncedSearch.value || undefined,
  }
  await fetchTransactions(params)
}

// Reset page when filters change
watch(
  [
    selectedAccountId,
    selectedInstrumentId,
    selectedTransactionTypeId,
    fromDate,
    toDate,
    debouncedSearch,
  ],
  () => {
    page.value = 1
  }
)

// Watch filter changes and update URL
watch(
  [
    searchText,
    selectedAccountId,
    selectedTransactionTypeId,
    selectedInstrumentId,
    fromDate,
    toDate,
  ],
  () => {
    const query: Record<string, string> = {}
    if (searchText.value) query.search = searchText.value
    if (selectedAccountId.value) query.accountId = String(selectedAccountId.value)
    if (selectedTransactionTypeId.value)
      query.transactionTypeId = String(selectedTransactionTypeId.value)
    if (selectedInstrumentId.value) query.instrumentId = String(selectedInstrumentId.value)
    if (fromDate.value) query.fromDate = fromDate.value
    if (toDate.value) query.toDate = toDate.value

    router.replace({ query })
  }
)

// Watch for changes and reload
watch(
  [
    page,
    itemsPerPage,
    sortBy,
    selectedAccountId,
    selectedInstrumentId,
    selectedTransactionTypeId,
    fromDate,
    toDate,
    debouncedSearch,
  ],
  () => {
    loadItems()
  },
  { immediate: true }
)

onMounted(() => {
  refStore.loadTransactionTypes()
  refStore.loadInstruments()
  navStore.loadAccounts()
})
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="6" md="2">
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

      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedAccountId"
          :items="navStore.accounts"
          item-title="name"
          item-value="id"
          label="Account"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedTransactionTypeId"
          :items="refStore.transactionTypes"
          item-title="name"
          item-value="id"
          label="Transaction Type"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedInstrumentId"
          :items="instrumentItems"
          label="Instrument"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="2">
        <v-text-field
          v-model="fromDate"
          label="From Date"
          type="date"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="2">
        <v-text-field
          v-model="toDate"
          label="To Date"
          type="date"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
    </v-row>
    <v-row dense>
      <v-col>
        <v-data-table-server
          v-model:page="page"
          v-model:items-per-page="itemsPerPage"
          v-model:sort-by="sortBy"
          :headers="headers"
          :items="items"
          :items-length="totalCount"
          :loading="isLoading"
          :items-per-page-options="[
            { value: 10, title: '10' },
            { value: 15, title: '15' },
            { value: 25, title: '25' },
            { value: 50, title: '50' },
            { value: 100, title: '100' },
            { value: -1, title: 'All' },
          ]"
          density="compact"
        >
          <template #item.date="{ value }">
            <span style="white-space: nowrap">{{ formatDate(value) }}</span>
          </template>

          <template #item.value="{ value }">
            <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
          </template>

          <template #item.instrumentTicker="{ item }">
            <router-link
              v-if="item.instrumentId"
              :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
              class="text-decoration-none"
            >
              {{ item.instrumentTicker }}
            </router-link>
          </template>

          <template #item.instrumentName="{ item }">
            <router-link
              v-if="item.instrumentId"
              :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
              class="text-decoration-none"
            >
              {{ item.instrumentName }}
            </router-link>
          </template>

          <template #no-data> No transactions found </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
