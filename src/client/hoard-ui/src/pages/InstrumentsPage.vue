<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useInstruments } from '@/composables/useInstruments'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'

const router = useRouter()
const route = useRoute()
const { items, totalCount, isLoading, fetchInstruments } = useInstruments()
const refStore = useReferenceDataStore()

const page = ref(1)
const itemsPerPage = ref(15)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([{ key: 'name', order: 'asc' }])

// Filter state - initialize from query params
const initialSearch = (route.query.search as string) || ''
const initialInstrumentTypeId = route.query.instrumentTypeId
  ? Number(route.query.instrumentTypeId)
  : null
const initialAssetClassId = route.query.assetClassId ? Number(route.query.assetClassId) : null
const initialAssetSubclassId = route.query.assetSubclassId
  ? Number(route.query.assetSubclassId)
  : null

const selectedInstrumentTypeId = ref<number | null>(initialInstrumentTypeId)
const selectedAssetClassId = ref<number | null>(initialAssetClassId)
const selectedAssetSubclassId = ref<number | null>(initialAssetSubclassId)
const searchText = ref(initialSearch)
const debouncedSearch = ref(initialSearch)

const availableSubclasses = computed(() => {
  if (!selectedAssetClassId.value) return []
  const ac = refStore.assetClasses.find((c) => c.id === selectedAssetClassId.value)
  return ac?.subclasses ?? []
})

// Helper to find parent asset class from subclass ID
function findParentAssetClassId(subclassId: number): number | null {
  const parent = refStore.assetClasses.find((ac) => ac.subclasses.some((s) => s.id === subclassId))
  return parent?.id ?? null
}

// Auto-resolve parent asset class when subclass is provided in URL without asset class
watch(
  () => refStore.assetClasses,
  (classes) => {
    if (classes.length > 0 && initialAssetSubclassId && !selectedAssetClassId.value) {
      const parentId = findParentAssetClassId(initialAssetSubclassId)
      if (parentId) {
        selectedAssetClassId.value = parentId
        // Re-set the subclass after asset class is set (since the watcher below clears it)
        selectedAssetSubclassId.value = initialAssetSubclassId
      }
    }
  },
  { immediate: true }
)

watch(selectedAssetClassId, (_newVal, oldVal) => {
  // Only clear subclass when user manually changes asset class, not on initial auto-resolve
  if (oldVal !== null) {
    selectedAssetSubclassId.value = null
  }
})

let debounceTimer: ReturnType<typeof setTimeout> | undefined
watch(searchText, (val) => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = val
  }, 400)
})

const headers = [
  { title: 'Ticker', key: 'tickerDisplay', sortable: true, width: '160px' },
  { title: 'Name', key: 'name', sortable: true },
  { title: 'Instrument Type', key: 'instrumentTypeName', sortable: false },
  { title: 'Asset Class', key: 'assetClassName', sortable: true },
  { title: 'Asset Subclass', key: 'assetSubclassName', sortable: false },
] as const

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order,
    instrumentTypeId: selectedInstrumentTypeId.value ?? undefined,
    assetClassId: selectedAssetClassId.value ?? undefined,
    assetSubclassId: selectedAssetSubclassId.value ?? undefined,
    search: debouncedSearch.value || undefined,
  }
  await fetchInstruments(params)
}

// Reset page when filters change
watch(
  [selectedInstrumentTypeId, selectedAssetClassId, selectedAssetSubclassId, debouncedSearch],
  () => {
    page.value = 1
  }
)

// Watch filter changes and update URL
watch([searchText, selectedInstrumentTypeId, selectedAssetClassId, selectedAssetSubclassId], () => {
  const query: Record<string, string> = {}
  if (searchText.value) query.search = searchText.value
  if (selectedInstrumentTypeId.value)
    query.instrumentTypeId = String(selectedInstrumentTypeId.value)
  if (selectedAssetClassId.value) query.assetClassId = String(selectedAssetClassId.value)
  if (selectedAssetSubclassId.value) query.assetSubclassId = String(selectedAssetSubclassId.value)

  router.replace({ query })
})

// Watch for changes and reload
watch(
  [
    page,
    itemsPerPage,
    sortBy,
    selectedInstrumentTypeId,
    selectedAssetClassId,
    selectedAssetSubclassId,
    debouncedSearch,
  ],
  () => {
    loadItems()
  },
  { immediate: true }
)

onMounted(() => {
  refStore.loadInstrumentTypes()
  refStore.loadAssetClasses()
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
          v-model="selectedInstrumentTypeId"
          :items="refStore.instrumentTypes"
          item-title="name"
          item-value="id"
          label="Instrument Type"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedAssetClassId"
          :items="refStore.assetClasses"
          item-title="name"
          item-value="id"
          label="Asset Class"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" sm="6" md="2">
        <v-select
          v-model="selectedAssetSubclassId"
          :items="availableSubclasses"
          item-title="name"
          item-value="id"
          label="Asset Subclass"
          clearable
          hide-details
          density="compact"
          variant="outlined"
          :disabled="!selectedAssetClassId"
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
          <template #item.tickerDisplay="{ item }">
            <router-link :to="{ name: 'instrument-detail', params: { id: item.id } }">
              {{ item.tickerDisplay }}</router-link
            >
          </template>

          <template #item.name="{ item }">
            <router-link :to="{ name: 'instrument-detail', params: { id: item.id } }">
              {{ item.name }}</router-link
            >
          </template>

          <template #no-data> No instruments found </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
