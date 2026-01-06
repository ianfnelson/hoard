<template>
  <v-card density="compact" flat>
    <v-card-text class="py-2">
      <div class="text-caption text-medium-emphasis">
        {{ title }}
      </div>

      <div
        class="text-h6 font-weight-medium"
        :class="trendClass"
      >
        {{ value }}
      </div>

      <div
        v-if="secondary"
        class="text-caption"
        :class="trendClass"
      >
        {{ secondary }}
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  title: string
  value: string
  secondary?: string
  trend?: number
}>()

const trendClass = computed(() => {
  if (props.trend == null) return undefined
  if (props.trend > 0) return 'text-success'
  if (props.trend < 0) return 'text-error'
  return undefined
})
</script>
