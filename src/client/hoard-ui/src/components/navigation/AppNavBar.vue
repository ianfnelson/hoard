<script setup lang="ts">
import PortfolioSelector from './PortfolioSelector.vue'
import { useTheme } from '@/composables/useTheme'

const { isDark, toggleTheme } = useTheme()
</script>

<template>
  <v-app-bar density="comfortable" elevation="1">
    <PortfolioSelector />

    <v-btn :to="{ name: 'instruments' }" text>Instruments</v-btn>
    <v-btn :to="{ name: 'transactions' }" text>Transactions</v-btn>
    <v-btn :to="{ name: 'news' }" text>News</v-btn>
    <v-btn :to="{ name: 'system' }" text>System</v-btn>

    <v-spacer />

    <v-btn
      icon
      :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      @click="toggleTheme"
    >
      <v-icon>{{ isDark ? 'mdi-lightbulb-on' : 'mdi-lightbulb-off-outline' }}</v-icon>
    </v-btn>
  </v-app-bar>
</template>
