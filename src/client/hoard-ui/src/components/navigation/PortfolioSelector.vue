<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useNavigationStore } from "@/stores/navigationStore";

const route = useRoute();
const navStore = useNavigationStore();

const currentPortfolioId = computed(() => {
  const id = route.params.id;
  return typeof id === "string" ? Number(id) : null;
});

const currentPortfolioName = computed(() => {
  if (!currentPortfolioId.value) return null;
  return navStore.portfolios.find(p => p.id === currentPortfolioId.value)?.name;
});
</script>

<template>
  <v-menu>
    <template #activator="{ props }">
      <v-btn text v-bind="props">
        {{ currentPortfolioName || "Select Portfolio" }}
        <v-icon end>mdi-chevron-down</v-icon>
      </v-btn>
    </template>
    <v-list>
      <v-list-item
        v-for="portfolio in navStore.portfolios"
        :key="portfolio.id"
        :to="{ name: 'portfolio-overview', params: { id: portfolio.id } }"
        :active="portfolio.id === currentPortfolioId"
      >
        <v-list-item-title>{{ portfolio.name }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>
