<script setup lang="ts">
import { onMounted } from "vue";
import { usePortfolioStore } from "@/stores/portfolios";

const store = usePortfolioStore();

onMounted(() => {
  store.loadPortfolios();
});
</script>

<template>
  <main>
    <h1>Hoard</h1>
    <div v-if="store.loading">
      Loading…
    </div>

    <table v-else>
      <thead>
      <tr>
        <th>Name</th>
        <th>Created</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="p in store.portfolios" :key="p.id">
        <td>{{ p.name }}</td>
        <td>{{ p.createdUtc }}</td>
      </tr>
      </tbody>
    </table>
  </main>
</template>
