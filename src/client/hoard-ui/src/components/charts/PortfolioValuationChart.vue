<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useTheme } from 'vuetify'
import { usePortfolioValuationSeries } from '@/composables/usePortfolioValuationSeries'
import type { DisaggregateBy } from '@/api/portfoliosApi'

const props = defineProps<{
  portfolioId: number
  from?: string
  to?: string
  disaggregateBy: DisaggregateBy
}>()

const theme = useTheme()
const isDark = computed(() => theme.global.current.value.dark)

const { series, isLoading, fetchSeries } = usePortfolioValuationSeries()

const totalDataPoints = computed(() => series.value.reduce((sum, s) => sum + s.data.length, 0))

const sortedSeries = computed(() =>
  [...series.value].sort((a, b) => {
    const aLast = a.data.length > 0 ? a.data[a.data.length - 1].y : 0
    const bLast = b.data.length > 0 ? b.data[b.data.length - 1].y : 0
    return bLast - aLast
  })
)

const chartColors = computed(() =>
  sortedSeries.value.map((s) => s.color ?? undefined).filter(Boolean)
)

const chartOptions = computed(() => ({
  chart: {
    type: 'area' as const,
    stacked: true,
    background: 'transparent',
    fontFamily: 'IBM Plex Sans, sans-serif',
  },
  theme: {
    mode: isDark.value ? ('dark' as const) : ('light' as const),
  },
  dataLabels: {
    enabled: false,
  },
  stroke: {
    curve: 'straight' as const,
  },
  xaxis: {
    type: 'datetime' as const,
    tickAmount: 6,
    labels: {
      datetimeUTC: false,
    },
  },
  yaxis: {
    min: 0,
    forceNiceScale: true,
  },
  fill: {
    opacity: 0.4,
  },
  legend: {
    position: 'right' as const,
  },
  ...(chartColors.value.length > 0 ? { colors: chartColors.value } : {}),
}))

const chartSeries = computed(() =>
  sortedSeries.value.map((s) => ({
    name: s.name,
    data: s.data,
  }))
)

function loadData() {
  fetchSeries(props.portfolioId, props.from, props.to, props.disaggregateBy)
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div v-if="isLoading" class="d-flex justify-center align-center" style="min-height: 300px">
    <v-progress-circular indeterminate />
  </div>
  <div
    v-else-if="totalDataPoints === 0"
    class="d-flex justify-center align-center text-medium-emphasis"
    style="min-height: 100px"
  >
    Not enough valuation data to display chart
  </div>
  <apexchart v-else type="area" height="550" :options="chartOptions" :series="chartSeries" />
</template>
