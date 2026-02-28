<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const tabs = [
  { name: 'system-status', label: 'Status', to: { name: 'system-status' } },
  { name: 'system-operations', label: 'Operations', to: { name: 'system-operations' } },
]

const currentTab = computed(() => route.name)
</script>

<template>
  <v-tabs bg-color="transparent" density="compact" :model-value="currentTab">
    <v-tab v-for="tab in tabs" :key="tab.name" :value="tab.name" :to="tab.to">
      {{ tab.label }}
    </v-tab>
  </v-tabs>
</template>
