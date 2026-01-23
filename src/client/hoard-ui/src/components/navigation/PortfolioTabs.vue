<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import { usePortfolioStore } from "@/stores/portfolioStore";
import { formatUpdatedTime } from "@/utils/formatters";

const route = useRoute();
const portfolioStore = usePortfolioStore();

const tabs = [
  { name: "portfolio-overview", label: "Overview", to: { name: "portfolio-overview" } },
  { name: "portfolio-exposure", label: "Exposure", to: { name: "portfolio-exposure" } },
  { name: "portfolio-positions", label: "Positions", to: { name: "portfolio-positions" } },
  { name: "portfolio-snapshots", label: "Snapshots", to: { name: "portfolio-snapshots" } },
  { name: "portfolio-history", label: "History", to: { name: "portfolio-history" } }
];

const currentTab = computed(() => route.name);
</script>

<template>
  <div class="d-flex align-center">
    <v-tabs
      bg-color="transparent"
      density="compact"
      :model-value="currentTab"
      class="flex-grow-1"
    >
      <v-tab
        v-for="tab in tabs"
        :key="tab.name"
        :value="tab.name"
        :to="tab.to"
      >
        {{ tab.label }}
      </v-tab>
    </v-tabs>
    <div class="text-button text-medium-emphasis ml-4">
      {{ formatUpdatedTime(portfolioStore.portfolio?.performance?.updatedUtc) }}
    </div>
  </div>
</template>
