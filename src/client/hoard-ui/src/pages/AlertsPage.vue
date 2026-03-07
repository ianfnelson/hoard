<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAlerts } from '@/composables/useAlerts'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatDateTime } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'
import InstrumentSelect from '@/components/InstrumentSelect.vue'

const pageTitle = ref('Alerts')
usePageTitle(pageTitle)

const router = useRouter()
const route = useRoute()
const { items, totalCount, isLoading, fetchAlerts } = useAlerts()
const refStore = useReferenceDataStore()
const navStore = useNavigationStore()

const page = ref(1)
const itemsPerPage = ref(15)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'createdUtc', order: 'desc' },
])

// Filter state - initialize from query params
const initialAlertTypeId = route.query.alertTypeId ? Number(route.query.alertTypeId) : null
const initialInstrumentId = route.query.instrumentId ? Number(route.query.instrumentId) : null
const initialPortfolioId = route.query.portfolioId ? Number(route.query.portfolioId) : null

const selectedAlertTypeId = ref<number | null>(initialAlertTypeId)
const selectedInstrumentId = ref<number | null>(initialInstrumentId)
const selectedPortfolioId = ref<number | null>(initialPortfolioId)

// Computed properties for filter items
const portfolioItems = computed(() => {
  return navStore.portfolios.map((p) => ({
    title: p.name,
    value: p.id,
  }))
})

const alertTypeItems = computed(() => {
  return refStore.alertTypes.map((at) => ({
    title: at.name,
    value: at.id,
  }))
})

const headers = [
  { title: 'Date', key: 'createdUtc', sortable: true, width: '180px' },
  { title: 'Type', key: 'alertTypeName', sortable: true },
  { title: 'Subject', key: 'subject', sortable: false },
  { title: 'Title', key: 'title', sortable: true },
] as const

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order,
    alertTypeId: selectedAlertTypeId.value ?? undefined,
    instrumentId: selectedInstrumentId.value ?? undefined,
    portfolioId: selectedPortfolioId.value ?? undefined,
  }
  await fetchAlerts(params)
}

// Reset page when filters change
watch([selectedAlertTypeId, selectedInstrumentId, selectedPortfolioId], () => {
  page.value = 1
})

// Watch filter changes and update URL
watch([selectedAlertTypeId, selectedInstrumentId, selectedPortfolioId], () => {
  const query: Record<string, string> = {}
  if (selectedAlertTypeId.value) query.alertTypeId = String(selectedAlertTypeId.value)
  if (selectedInstrumentId.value) query.instrumentId = String(selectedInstrumentId.value)
  if (selectedPortfolioId.value) query.portfolioId = String(selectedPortfolioId.value)
  router.replace({ query })
})

// Watch for changes and reload
watch(
  [page, itemsPerPage, sortBy, selectedAlertTypeId, selectedInstrumentId, selectedPortfolioId],
  () => {
    loadItems()
  },
  { immediate: true }
)

onMounted(() => {
  refStore.loadAlertTypes()
  refStore.loadInstruments()
  navStore.loadPortfolios()
})
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="4">
        <v-select
          v-model="selectedAlertTypeId"
          :items="alertTypeItems"
          label="Alert Type"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="4">
        <v-select
          v-model="selectedPortfolioId"
          :items="portfolioItems"
          label="Portfolio"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="4">
        <InstrumentSelect
          v-model="selectedInstrumentId"
          label="Instrument"
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
          :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
          density="compact"
        >
          <template #item.createdUtc="{ item, value }">
            <router-link
              :to="{ name: 'alert-detail', params: { id: item.id } }"
              class="text-decoration-none"
              style="white-space: nowrap"
            >
              {{ formatDateTime(value) }}
            </router-link>
          </template>

          <template #item.subject="{ item }">
            <router-link
              v-if="item.instrumentId"
              :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
              class="text-decoration-none"
            >
              {{ item.instrumentName }}
            </router-link>
            <router-link
              v-else-if="item.portfolioId"
              :to="{ name: 'portfolio-overview', params: { id: item.portfolioId } }"
              class="text-decoration-none"
            >
              {{ item.portfolioName }}
            </router-link>
          </template>

          <template #item.title="{ item }">
            <router-link
              :to="{ name: 'alert-detail', params: { id: item.id } }"
              class="text-decoration-none"
            >
              {{ item.title }}
            </router-link>
          </template>

          <template #no-data> No alerts found </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
