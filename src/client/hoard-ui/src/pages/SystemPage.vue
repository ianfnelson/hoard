<script setup lang="ts">
import { ref } from 'vue'
import { usePageTitle } from '@/composables/usePageTitle'

const pageTitle = ref('System')
usePageTitle(pageTitle)
</script>

<template>
  <v-container fluid>
    <v-row>
      <v-col>
        <v-card>
          <v-card-title>System</v-card-title>
          <v-card-text>
            <p>Coming soon: System settings and metadata operations.</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
