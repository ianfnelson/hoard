<script setup lang="ts">
import { onMounted } from "vue";
import AppNavBar from "@/components/navigation/AppNavBar.vue";
import { useNavigationStore } from "@/stores/navigationStore";
import { useReferenceDataStore } from "@/stores/referenceDataStore";

const navStore = useNavigationStore();
const refStore = useReferenceDataStore();

onMounted(() => {
  navStore.loadPortfolios();
  refStore.loadInstrumentTypes();
  refStore.loadAssetClasses();
});
</script>

<template>
  <v-app>
    <AppNavBar />
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>
