<script setup lang="ts">
import SystemTabs from '@/components/navigation/SystemTabs.vue'
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <SystemTabs />
      </v-col>
    </v-row>

    <router-view />
  </v-container>
</template>
