<script setup lang="ts">
import { computed, onMounted } from 'vue'
import apexchart from 'vue3-apexcharts'
import { useTheme } from 'vuetify'
import { useHoldingValuationSeries } from '@/composables/useHoldingValuationSeries'

const props = defineProps<{
  instrumentId: number
}>()

const theme = useTheme()
const isDark = computed(() => theme.global.current.value.dark)

const { series, isLoading, fetchHoldingValuationSeries } = useHoldingValuationSeries()

const totalDataPoints = computed(() => series.value.reduce((sum, s) => sum + s.data.length, 0))

const chartColors = computed(() => series.value.map((s) => s.color ?? undefined).filter(Boolean))

const chartOptions = computed(() => ({
  chart: {
    type: 'area' as const,
    stacked: true,
    background: 'transparent',
    fontFamily: 'IBM Plex Sans, sans-serif',
    animations: { enabled: false },
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
  ...(chartColors.value.length > 0 ? { colors: chartColors.value } : {}),
}))

const chartSeries = computed(() =>
  series.value.map((s) => ({
    name: s.name,
    data: s.data,
  }))
)

onMounted(() => {
  fetchHoldingValuationSeries(props.instrumentId)
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
  <apexchart v-else type="area" height="450" :options="chartOptions" :series="chartSeries" />
</template>
