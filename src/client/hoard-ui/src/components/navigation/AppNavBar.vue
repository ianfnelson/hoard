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

    <v-menu>
      <template #activator="{ props }">
        <v-btn text v-bind="props">
          System
          <v-icon end>mdi-chevron-down</v-icon>
        </v-btn>
      </template>
      <v-list>
        <v-list-item :to="{ name: 'system' }">
          <v-list-item-title>Settings</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-menu>

    <v-spacer />

    <v-btn
      icon
      :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      @click="toggleTheme"
    >
      <v-icon>{{ isDark ? 'mdi-lightbulb-off-outline' : 'mdi-lightbulb-on' }}</v-icon>
    </v-btn>
  </v-app-bar>
</template>
