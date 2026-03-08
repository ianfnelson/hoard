<script setup lang="ts">
import { ref } from 'vue'
import { useDisplay } from 'vuetify'
import PortfolioSelector from './PortfolioSelector.vue'
import { useTheme } from '@/composables/useTheme'

const { isDark, toggleTheme } = useTheme()
const { mobile } = useDisplay()
const drawer = ref(false)

const navigationItems = [
  { name: 'Instruments', to: { name: 'instruments' }, icon: 'mdi-chart-line' },
  { name: 'Transactions', to: { name: 'transactions' }, icon: 'mdi-swap-horizontal' },
  { name: 'News', to: { name: 'news' }, icon: 'mdi-newspaper' },
  { name: 'Alerts', to: { name: 'alerts' }, icon: 'mdi-bell' },
  { name: 'System', to: '/system', icon: 'mdi-cog' },
]
</script>

<template>
  <v-app-bar density="comfortable" elevation="1">
    <!-- Mobile: Hamburger menu -->
    <v-app-bar-nav-icon v-if="mobile" @click="drawer = !drawer" />

    <PortfolioSelector />

    <!-- Desktop: Show all nav buttons -->
    <template v-if="!mobile">
      <v-btn :to="{ name: 'instruments' }" text>Instruments</v-btn>
      <v-btn :to="{ name: 'transactions' }" text>Transactions</v-btn>
      <v-btn :to="{ name: 'news' }" text>News</v-btn>
      <v-btn :to="{ name: 'alerts' }" text>Alerts</v-btn>
      <v-btn to="/system" text>System</v-btn>
    </template>

    <v-spacer />

    <v-btn
      icon
      :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      @click="toggleTheme"
    >
      <v-icon>{{ isDark ? 'mdi-lightbulb-on' : 'mdi-lightbulb-off-outline' }}</v-icon>
    </v-btn>
  </v-app-bar>

  <!-- Mobile navigation drawer -->
  <v-navigation-drawer v-model="drawer" temporary>
    <v-list nav>
      <v-list-item
        v-for="item in navigationItems"
        :key="item.name"
        :to="item.to"
        :prepend-icon="item.icon"
        @click="drawer = false"
      >
        <v-list-item-title>{{ item.name }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>
