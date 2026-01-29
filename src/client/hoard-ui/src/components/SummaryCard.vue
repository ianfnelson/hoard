<template>
  <v-card density="compact" flat class="summary-card">
    <v-card-text class="py-3 text-center">
      <div class="text-caption-lg text-medium-emphasis">
        {{ title }}
      </div>

      <div class="text-h6 font-weight-medium mt-1" :class="trendClass">
        {{ formattedValue }}
      </div>

      <div class="text-caption-lg mt-1" :class="trendClass">
        {{ formattedPercentage || '\u00A0' }}
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { formatCurrency, formatPercentage, getTrendClass } from '@/utils/formatters'

const props = defineProps<{
  title: string
  value?: number
  percentage?: number
  trend?: number
}>()

const formattedValue = computed(() => formatCurrency(props.value))
const formattedPercentage = computed(() => formatPercentage(props.percentage))
const trendClass = computed(() => getTrendClass(props.trend))
</script>

<style scoped>
.summary-card {
  background-color: rgba(0, 0, 0, 0.02);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 4px;
}

.v-theme--dark .summary-card {
  background-color: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.1);
}
</style>
