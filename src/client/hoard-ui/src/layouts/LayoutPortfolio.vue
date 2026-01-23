<script setup lang="ts">
import { onMounted, watch, computed } from "vue";
import { usePortfolioStore } from "@/stores/portfolioStore";
import PortfolioTabs from "@/components/navigation/PortfolioTabs.vue";

const props = defineProps<{ id: string }>();

const portfolioStore = usePortfolioStore();

const portfolioId = computed(() => Number(props.id));

onMounted(() => {
  portfolioStore.load(portfolioId.value);
});

watch(() => props.id, (newId) => {
  portfolioStore.load(Number(newId));
});
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <PortfolioTabs />
      </v-col>
    </v-row>

    <router-view :key="portfolioId" />
  </v-container>
</template>
