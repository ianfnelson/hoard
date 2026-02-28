<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { usePageTitle } from '@/composables/usePageTitle'
import PortfolioValuationChart from '@/components/charts/PortfolioValuationChart.vue'
import type { DisaggregateBy } from '@/api/portfoliosApi'

const props = defineProps<{ id: string }>()
const route = useRoute()
const router = useRouter()
const portfolioStore = usePortfolioStore()

const pageTitle = computed(() => {
  const portfolioName = portfolioStore.portfolio?.name
  return portfolioName ? `${portfolioName} : History` : null
})
usePageTitle(pageTitle)

const disaggregateOptions: { value: DisaggregateBy; label: string }[] = [
  { value: 'none', label: 'Total' },
  { value: 'accounts', label: 'Accounts' },
  { value: 'assetClasses', label: 'Asset Classes' },
  { value: 'assetSubclasses', label: 'Asset Subclasses' },
  { value: 'instrumentTypes', label: 'Instrument Types' },
]

// Form state
const fromDate = ref<string>('')
const toDate = ref<string>('')
const disaggregateBy = ref<DisaggregateBy>('none')

// Controls when the chart remounts to fetch fresh data
const loadKey = ref(0)

function getCurrentDate(): string {
  const now = new Date()
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function getDefaultFromDate(): string {
  const now = new Date()
  now.setFullYear(now.getFullYear() - 1)
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function loadData() {
  loadKey.value++

  const query: Record<string, string> = {}
  if (fromDate.value) query.from = fromDate.value
  if (toDate.value) query.to = toDate.value
  if (disaggregateBy.value && disaggregateBy.value !== 'none')
    query.disaggregateBy = disaggregateBy.value

  router.replace({ query })
}

// Initialize from querystring or defaults, then load
onMounted(() => {
  fromDate.value = (route.query.from as string) || getDefaultFromDate()
  toDate.value = (route.query.to as string) || getCurrentDate()
  disaggregateBy.value = (route.query.disaggregateBy as DisaggregateBy) || 'none'

  loadData()
})
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-4">
      <v-col cols="auto" class="d-flex align-center">
        <v-btn-toggle
          v-model="disaggregateBy"
          mandatory
          color="primary"
          density="compact"
          @update:model-value="loadData"
        >
          <v-btn
            v-for="option in disaggregateOptions"
            :key="option.value"
            :value="option.value"
            size="small"
          >
            {{ option.label }}
          </v-btn>
        </v-btn-toggle>
      </v-col>
      <v-col cols="12" md="3">
        <v-text-field
          v-model="fromDate"
          label="From"
          type="date"
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
        />
      </v-col>
      <v-col cols="auto" class="d-flex align-center">
        <v-btn color="primary" @click="loadData">Load</v-btn>
      </v-col>
    </v-row>

    <v-row dense>
      <v-col>
        <PortfolioValuationChart
          v-if="loadKey > 0"
          :key="loadKey"
          :portfolio-id="parseInt(props.id)"
          :from="fromDate || undefined"
          :to="toDate || undefined"
          :disaggregate-by="disaggregateBy"
        />
      </v-col>
    </v-row>
  </v-container>
</template>
