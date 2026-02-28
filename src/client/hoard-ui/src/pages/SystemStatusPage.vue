<script setup lang="ts">
import { ref } from 'vue'
import { usePageTitle } from '@/composables/usePageTitle'

const pageTitle = ref('Status')
usePageTitle(pageTitle)
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-card>
          <v-card-text> Coming soon </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
