<script setup lang="ts">
import { computed } from 'vue'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { usePageTitle } from '@/composables/usePageTitle'

const store = usePortfolioStore()

const pageTitle = computed(() => {
  const portfolioName = store.portfolio?.name
  return portfolioName ? `${portfolioName} : History` : null
})
usePageTitle(pageTitle)
</script>

<template>
  <v-container fluid>
    <v-row dense class="mt-4">
      <v-col>
        <v-card>
          <v-card-title>Portfolio History</v-card-title>
          <v-card-text>
            <p>Coming soon: Compare portfolio state at any two dates in history.</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
