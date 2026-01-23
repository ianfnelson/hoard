<script setup lang="ts">
import { onMounted } from "vue";
import AppNavBar from "@/components/navigation/AppNavBar.vue";
import { useNavigationStore } from "@/stores/navigationStore";

const navStore = useNavigationStore();

onMounted(() => {
  navStore.loadPortfolios();
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
