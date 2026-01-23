<script setup lang="ts">
import PortfolioSelector from "./PortfolioSelector.vue";
</script>

<template>
  <v-app-bar density="comfortable" elevation="1">
    <v-app-bar-title>Hoard</v-app-bar-title>

    <v-spacer />

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
  </v-app-bar>
</template>
