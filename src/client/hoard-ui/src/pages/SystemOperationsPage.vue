<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/composables/usePageTitle'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'
import OperationSection from '@/components/operations/OperationSection.vue'
import OperationButton from '@/components/operations/OperationButton.vue'
import OperationModal from '@/components/operations/OperationModal.vue'
import type { OperationDefinition } from '@/types/operations'
import * as operationsApi from '@/api/operationsApi'

const pageTitle = ref('Operations')
usePageTitle(pageTitle)

const refStore = useReferenceDataStore()
const navStore = useNavigationStore()

const showModal = ref(false)
const selectedOperation = ref<OperationDefinition | null>(null)
const successMessage = ref<string | null>(null)
const isProcessing = ref(false)

// Define all 15 operations
const operations: OperationDefinition[] = [
  // Analysis
  {
    key: 'analyse-instruments',
    label: 'Analyse Instruments',
    section: 'Analysis',
    endpoint: '/operations/analysis/analyse-instruments',
    fields: [{ name: 'asOfDate', label: 'As Of Date', type: 'date', required: false }],
  },

  // Chrono
  {
    key: 'trigger-close-of-day',
    label: 'Trigger Close of Day',
    section: 'Chrono',
    endpoint: '/operations/chrono/trigger-close-of-day',
    fields: [{ name: 'asOfDate', label: 'As Of Date', type: 'date', required: false }],
  },

  // Holdings
  {
    key: 'backfill-holdings',
    label: 'Backfill Holdings',
    section: 'Holdings',
    endpoint: '/operations/holdings/backfill',
    fields: [
      { name: 'startDate', label: 'Start Date', type: 'date', required: false },
      { name: 'endDate', label: 'End Date', type: 'date', required: false },
    ],
  },
  {
    key: 'calculate-holdings',
    label: 'Calculate Holdings',
    section: 'Holdings',
    endpoint: '/operations/holdings/calculate',
    fields: [{ name: 'asOfDate', label: 'As Of Date', type: 'date', required: false }],
  },

  // News
  {
    key: 'refresh-news',
    label: 'Refresh News',
    section: 'News',
    endpoint: '/operations/news/refresh',
    fields: [{ name: 'instrumentId', label: 'Instrument', type: 'instrument', required: false }],
  },

  // Performance
  {
    key: 'calculate-performance',
    label: 'Calculate Performance',
    section: 'Performance',
    endpoint: '/operations/performance/calculate',
    fields: [{ name: 'instrumentId', label: 'Instrument', type: 'instrument', required: false }],
  },

  // Positions
  {
    key: 'calculate-positions',
    label: 'Calculate Positions',
    section: 'Positions',
    endpoint: '/operations/positions/calculate',
    fields: [],
  },

  // Prices
  {
    key: 'backfill-prices',
    label: 'Backfill Prices',
    section: 'Prices',
    endpoint: '/operations/prices/backfill',
    fields: [
      { name: 'instrumentId', label: 'Instrument', type: 'instrument', required: false },
      { name: 'startDate', label: 'Start Date', type: 'date', required: false },
      { name: 'endDate', label: 'End Date', type: 'date', required: false },
    ],
  },
  {
    key: 'refresh-prices',
    label: 'Refresh Prices',
    section: 'Prices',
    endpoint: '/operations/prices/refresh',
    fields: [
      { name: 'instrumentId', label: 'Instrument', type: 'instrument', required: false },
      { name: 'startDate', label: 'Start Date', type: 'date', required: false },
      { name: 'endDate', label: 'End Date', type: 'date', required: false },
    ],
  },

  // Quotes
  {
    key: 'refresh-quotes',
    label: 'Refresh Quotes',
    section: 'Quotes',
    endpoint: '/operations/quotes/refresh',
    fields: [],
  },

  // Snapshots
  {
    key: 'backfill-snapshots',
    label: 'Backfill Snapshots',
    section: 'Snapshots',
    endpoint: '/operations/snapshots/backfill',
    fields: [
      { name: 'portfolioId', label: 'Portfolio', type: 'portfolio', required: false },
      { name: 'startYear', label: 'Start Year', type: 'number', required: false },
      { name: 'endYear', label: 'End Year', type: 'number', required: false },
    ],
  },
  {
    key: 'calculate-snapshots',
    label: 'Calculate Snapshots',
    section: 'Snapshots',
    endpoint: '/operations/snapshots/calculate',
    fields: [{ name: 'year', label: 'Year', type: 'number', required: false }],
  },

  // Valuations
  {
    key: 'backfill-valuations',
    label: 'Backfill Valuations',
    section: 'Valuations',
    endpoint: '/operations/valuations/backfill',
    fields: [
      { name: 'instrumentId', label: 'Instrument', type: 'instrument', required: false },
      { name: 'startDate', label: 'Start Date', type: 'date', required: false },
      { name: 'endDate', label: 'End Date', type: 'date', required: false },
    ],
  },
  {
    key: 'calculate-valuations',
    label: 'Calculate Valuations',
    section: 'Valuations',
    endpoint: '/operations/valuations/calculate',
    fields: [{ name: 'asOfDate', label: 'As Of Date', type: 'date', required: false }],
  },
]

// Group operations by section
const sections = [
  'Analysis',
  'Chrono',
  'Holdings',
  'News',
  'Performance',
  'Positions',
  'Prices',
  'Quotes',
  'Snapshots',
  'Valuations',
]

function getOperationsForSection(section: string): OperationDefinition[] {
  return operations.filter((op) => op.section === section)
}

function openOperation(key: string) {
  selectedOperation.value = operations.find((op) => op.key === key) ?? null
  showModal.value = true
}

async function handleOperationConfirm(params: Record<string, unknown>) {
  if (!selectedOperation.value) return

  isProcessing.value = true
  successMessage.value = null

  try {
    let message: string

    // Call the appropriate API function based on operation key
    switch (selectedOperation.value.key) {
      // Analysis
      case 'analyse-instruments':
        message = await operationsApi.analyseInstruments({
          asOfDate: params.asOfDate as string | undefined,
        })
        break

      // Chrono
      case 'trigger-close-of-day':
        message = await operationsApi.triggerCloseOfDay({
          asOfDate: params.asOfDate as string | undefined,
        })
        break

      // Holdings
      case 'backfill-holdings':
        message = await operationsApi.backfillHoldings({
          startDate: params.startDate as string | undefined,
          endDate: params.endDate as string | undefined,
        })
        break
      case 'calculate-holdings':
        message = await operationsApi.calculateHoldings({
          asOfDate: params.asOfDate as string | undefined,
        })
        break

      // News
      case 'refresh-news':
        message = await operationsApi.refreshNews({
          instrumentId: params.instrumentId as number | undefined,
        })
        break

      // Performance
      case 'calculate-performance':
        message = await operationsApi.calculatePerformance({
          instrumentId: params.instrumentId as number | undefined,
        })
        break

      // Positions
      case 'calculate-positions':
        message = await operationsApi.calculatePositions()
        break

      // Prices
      case 'backfill-prices':
        message = await operationsApi.backfillPrices({
          instrumentId: params.instrumentId as number | undefined,
          startDate: params.startDate as string | undefined,
          endDate: params.endDate as string | undefined,
        })
        break
      case 'refresh-prices':
        message = await operationsApi.refreshPrices({
          instrumentId: params.instrumentId as number | undefined,
          startDate: params.startDate as string | undefined,
          endDate: params.endDate as string | undefined,
        })
        break

      // Quotes
      case 'refresh-quotes':
        message = await operationsApi.refreshQuotes()
        break

      // Snapshots
      case 'backfill-snapshots':
        message = await operationsApi.backfillSnapshots({
          portfolioId: params.portfolioId as number | undefined,
          startYear: params.startYear as number | undefined,
          endYear: params.endYear as number | undefined,
        })
        break
      case 'calculate-snapshots':
        message = await operationsApi.calculateSnapshots({
          year: params.year as number | undefined,
        })
        break

      // Valuations
      case 'backfill-valuations':
        message = await operationsApi.backfillValuations({
          instrumentId: params.instrumentId as number | undefined,
          startDate: params.startDate as string | undefined,
          endDate: params.endDate as string | undefined,
        })
        break
      case 'calculate-valuations':
        message = await operationsApi.calculateValuations({
          asOfDate: params.asOfDate as string | undefined,
        })
        break

      default:
        throw new Error(`Unknown operation: ${selectedOperation.value.key}`)
    }

    successMessage.value = message
    showModal.value = false
  } catch (e) {
    // Error is handled in the modal
    console.error('Operation failed:', e)
  } finally {
    isProcessing.value = false
  }
}

onMounted(() => {
  refStore.loadInstruments()
  navStore.loadPortfolios()
})
</script>

<template>
  <v-container fluid>
    <!-- Success alert -->
    <v-row v-if="successMessage" dense>
      <v-col>
        <v-alert type="success" variant="tonal" closable @click:close="successMessage = null">
          {{ successMessage }}
        </v-alert>
      </v-col>
    </v-row>

    <!-- Operations sections -->
    <v-row dense>
      <v-col>
        <template v-for="section in sections" :key="section">
          <OperationSection :title="section">
            <OperationButton
              v-for="operation in getOperationsForSection(section)"
              :key="operation.key"
              :label="operation.label"
              @click="openOperation(operation.key)"
            />
          </OperationSection>
        </template>
      </v-col>
    </v-row>

    <!-- Operation modal -->
    <OperationModal
      v-model="showModal"
      :operation="selectedOperation"
      @confirm="handleOperationConfirm"
    />
  </v-container>
</template>
