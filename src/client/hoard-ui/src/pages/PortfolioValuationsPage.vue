<script setup lang="ts">
import { computed, ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatCurrency, formatPercentage, getTrendClass } from '@/utils/formatters'
import { getPortfolioValuations } from '@/api/portfoliosApi'
import type { PortfolioValuationsDto } from '@/api/dtos/Portfolios/PortfolioValuationsDto'

const props = defineProps<{ id: string }>()
const route = useRoute()
const router = useRouter()
const portfolioStore = usePortfolioStore()

const pageTitle = computed(() => {
  const portfolioName = portfolioStore.portfolio?.name
  return portfolioName ? `${portfolioName} : Valuations` : null
})
usePageTitle(pageTitle)

// Date state - bound to querystring
const fromDate = ref<string>('')
const toDate = ref<string>('')

// Validation errors
const validationError = ref<string | null>(null)

// Data state
const valuations = ref<PortfolioValuationsDto | null>(null)
const isLoading = ref(false)

// Initialize dates from querystring or defaults
onMounted(() => {
  fromDate.value = (route.query.from as string) || ''
  toDate.value = (route.query.to as string) || getCurrentDate()

  // Load data
  loadValuations()
})

// Watch for date changes and update querystring
watch([fromDate, toDate], ([newFrom, newTo]) => {
  const query: Record<string, string> = {}
  if (newFrom) query.from = newFrom
  if (newTo) query.to = newTo

  router.replace({ query })
})

// Watch querystring changes (e.g., from browser back/forward)
watch(
  () => route.query,
  (newQuery) => {
    fromDate.value = (newQuery.from as string) || ''
    toDate.value = (newQuery.to as string) || getCurrentDate()
    loadValuations()
  }
)

function getCurrentDate(): string {
  const now = new Date()
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function validateDates(): boolean {
  validationError.value = null

  if (!toDate.value) {
    validationError.value = 'To date is required'
    return false
  }

  const today = getCurrentDate()
  if (toDate.value > today) {
    validationError.value = 'To date cannot be in the future'
    return false
  }

  if (fromDate.value && fromDate.value >= toDate.value) {
    validationError.value = 'From date must be before to date'
    return false
  }

  return true
}

async function loadValuations() {
  if (!validateDates()) {
    valuations.value = null
    return
  }

  isLoading.value = true
  try {
    valuations.value = await getPortfolioValuations(parseInt(props.id), {
      from: fromDate.value || undefined,
      to: toDate.value || undefined,
    })
  } catch {
    validationError.value = 'Failed to load valuations'
    valuations.value = null
  } finally {
    isLoading.value = false
  }
}

// Computed headers based on whether fromDate is set
const headers = computed(() => {
  const baseHeaders = [{ title: 'Instrument', key: 'instrumentName', sortable: true }] as const

  if (fromDate.value) {
    return [
      ...baseHeaders,
      { title: 'From Value', key: 'fromValue', sortable: true, align: 'end' },
      { title: 'From %', key: 'fromPercentage', sortable: true, align: 'end' },
      { title: 'To Value', key: 'toValue', sortable: true, align: 'end' },
      { title: 'To %', key: 'toPercentage', sortable: true, align: 'end' },
      { title: 'Change Value', key: 'changeValue', sortable: true, align: 'end' },
      { title: 'Change %', key: 'changePercentage', sortable: true, align: 'end' },
    ] as const
  }

  return [
    ...baseHeaders,
    { title: 'To Value', key: 'toValue', sortable: true, align: 'end' },
    { title: 'To %', key: 'toPercentage', sortable: true, align: 'end' },
  ] as const
})

// Sort state
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'toValue', order: 'desc' },
])
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-4">
      <v-col cols="12" md="3">
        <v-text-field
          v-model="fromDate"
          label="From (Optional)"
          type="date"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="toDate"
          label="To"
          type="date"
          hide-details
          density="compact"
          variant="outlined"
          required
        />
      </v-col>
      <v-col cols="12" md="3" class="d-flex align-center">
        <v-btn color="primary" :loading="isLoading" @click="loadValuations">Load</v-btn>
      </v-col>
    </v-row>

    <v-row v-if="validationError" dense class="mb-2">
      <v-col>
        <v-alert type="error" density="compact">{{ validationError }}</v-alert>
      </v-col>
    </v-row>

    <v-row dense>
      <v-col>
        <v-data-table
          v-model:sort-by="sortBy"
          :headers="headers"
          :items="valuations?.items ?? []"
          :loading="isLoading"
          :items-per-page="-1"
          density="compact"
          hide-default-footer
        >
          <template #item.instrumentName="{ item }">
            <router-link
              :to="{ name: 'instrument-detail', params: { id: item.instrumentId } }"
              class="text-decoration-none"
            >
              {{ item.instrumentName }}
            </router-link>
          </template>

          <template #item.toValue="{ value }">
            <span>{{ value !== null ? formatCurrency(value) : '-' }}</span>
          </template>

          <template #item.toPercentage="{ value }">
            <span>{{ value !== null ? formatPercentage(value) : '-' }}</span>
          </template>

          <template #item.fromValue="{ value }">
            <span>{{ value !== null ? formatCurrency(value) : '-' }}</span>
          </template>

          <template #item.fromPercentage="{ value }">
            <span>{{ value !== null ? formatPercentage(value) : '-' }}</span>
          </template>

          <template #item.changeValue="{ value }">
            <span v-if="value !== null" :class="getTrendClass(value)">{{
              formatCurrency(value)
            }}</span>
            <span v-else>-</span>
          </template>

          <template #item.changePercentage="{ value }">
            <span v-if="value !== null" :class="getTrendClass(value)">{{
              formatPercentage(value)
            }}</span>
            <span v-else>-</span>
          </template>

          <template #no-data> No valuations available for the selected date(s) </template>
        </v-data-table>
      </v-col>
    </v-row>
  </v-container>
</template>
