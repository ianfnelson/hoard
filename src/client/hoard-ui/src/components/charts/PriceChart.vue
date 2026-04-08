<script setup lang="ts">
import { computed, onMounted } from 'vue'
import apexchart from 'vue3-apexcharts'
import { useTheme } from 'vuetify'
import { usePriceSeries } from '@/composables/usePriceSeries'

const props = defineProps<{
  instrumentId: number
}>()

const theme = useTheme()
const isDark = computed(() => theme.global.current.value.dark)

const { candles, isLoading, fetchPriceSeries } = usePriceSeries()

const chartOptions = computed(() => ({
  chart: {
    type: 'candlestick' as const,
    background: 'transparent',
    fontFamily: 'IBM Plex Sans, sans-serif',
    animations: { enabled: false },
    zoom: {
      autoScaleYaxis: true,
    },
  },
  theme: {
    mode: isDark.value ? ('dark' as const) : ('light' as const),
  },
  xaxis: {
    type: 'datetime' as const,
    tickAmount: 6,
    labels: {
      datetimeUTC: false,
    },
  },
  yaxis: {
    tooltip: { enabled: true },
    forceNiceScale: true,
  },
  plotOptions: {
    candlestick: {
      colors: {
        upward: isDark.value ? '#64b5f6' : '#1976d2',
        downward: isDark.value ? '#ef5350' : '#b71c1c',
      },
    },
  },
}))

const series = computed(() => [
  {
    data: candles.value,
  },
])

onMounted(() => {
  const to = new Date()
  const from = new Date()
  from.setFullYear(from.getFullYear() - 1)

  fetchPriceSeries(
    props.instrumentId,
    from.toISOString().slice(0, 10),
    to.toISOString().slice(0, 10)
  )
})
</script>

<template>
  <div v-if="isLoading" class="d-flex justify-center align-center" style="min-height: 300px">
    <v-progress-circular indeterminate />
  </div>
  <div
    v-else-if="candles.length === 0"
    class="d-flex justify-center align-center text-medium-emphasis"
    style="min-height: 100px"
  >
    Not enough price data to display chart
  </div>
  <apexchart v-else type="candlestick" height="400" :options="chartOptions" :series="series" />
</template>
