<script setup lang="ts">
import { computed } from 'vue'
import apexchart from 'vue3-apexcharts/core'
import { useTheme } from 'vuetify'

export interface PieChartData {
  name: string
  colour: string
  percentage: number
}

const props = defineProps<{
  data: PieChartData[]
  isLoading?: boolean
}>()

const theme = useTheme()
const isDark = computed(() => theme.global.current.value.dark)

const chartOptions = computed(() => ({
  chart: {
    type: 'pie' as const,
    background: 'transparent',
    fontFamily: 'IBM Plex Sans, sans-serif',
    animations: { enabled: false },
  },
  theme: {
    mode: isDark.value ? ('dark' as const) : ('light' as const),
  },
  labels: props.data.map((d) => d.name),
  colors: props.data.map((d) => d.colour),
  stroke: {
    width: 1,
    colors: isDark.value ? ['#424242'] : ['#e0e0e0'],
  },
  dataLabels: {
    enabled: true,
    formatter: (val: number) => `${val.toFixed(1)}%`,
  },
  legend: {
    show: false,
  },
  tooltip: {
    y: {
      formatter: (val: number) => `${val.toFixed(2)}%`,
    },
  },
}))

const series = computed(() => props.data.map((d) => d.percentage))
</script>

<template>
  <div v-if="isLoading" class="d-flex justify-center align-center" style="min-height: 300px">
    <v-progress-circular indeterminate />
  </div>
  <div
    v-else-if="data.length === 0"
    class="d-flex justify-center align-center text-medium-emphasis"
    style="min-height: 100px"
  >
    No data available
  </div>
  <apexchart v-else type="pie" height="350" :options="chartOptions" :series="series" />
</template>
