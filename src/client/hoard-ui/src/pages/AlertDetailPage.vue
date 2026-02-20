<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAlertDetail } from '@/composables/useAlertDetail'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatDateTime, formatDate } from '@/utils/formatters'

const route = useRoute()
const { alert, isLoading, error, fetchAlert } = useAlertDetail()

const pageTitle = computed(() => alert.value?.title)
usePageTitle(pageTitle)

const id = Number(route.params.id)

onMounted(() => {
  fetchAlert(id)
})
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-card v-if="isLoading">
          <v-card-text>
            <v-progress-circular indeterminate></v-progress-circular>
            Loading alert...
          </v-card-text>
        </v-card>

        <v-alert v-else-if="error" type="error">
          {{ error }}
        </v-alert>

        <v-card v-else-if="alert">
          <v-card-title>
            {{ alert.title }}
          </v-card-title>
          <v-card-text>
            <v-row dense>
              <v-col cols="12" sm="6">
                <div class="text-caption text-medium-emphasis">Alert Type</div>
                <div class="text-body-1">{{ alert.alertTypeName }}</div>
              </v-col>
              <v-col cols="12" sm="6">
                <div class="text-caption text-medium-emphasis">Subject</div>
                <div class="text-body-1">
                  <router-link
                    v-if="alert.instrumentId"
                    :to="{ name: 'instrument-detail', params: { id: alert.instrumentId } }"
                    class="text-decoration-none"
                  >
                    {{ alert.instrumentName }}
                  </router-link>
                  <router-link
                    v-else-if="alert.portfolioId"
                    :to="{ name: 'portfolio-overview', params: { id: alert.portfolioId } }"
                    class="text-decoration-none"
                  >
                    {{ alert.portfolioName }}
                  </router-link>
                </div>
              </v-col>
              <v-col cols="12" sm="6">
                <div class="text-caption text-medium-emphasis">Triggered Date</div>
                <div class="text-body-1">{{ formatDate(alert.triggeredDate) }}</div>
              </v-col>
              <v-col cols="12" sm="6">
                <div class="text-caption text-medium-emphasis">Created</div>
                <div class="text-body-1">{{ formatDateTime(alert.createdUtc) }}</div>
              </v-col>
            </v-row>
            <v-divider class="my-3" />
            <v-row dense>
              <v-col cols="12">
                <div class="text-caption text-medium-emphasis mb-2">Message</div>
                <div class="text-body-1" style="white-space: pre-wrap">{{ alert.message }}</div>
              </v-col>
            </v-row>
            <v-row v-if="alert.url" dense>
              <v-col cols="12">
                <v-divider class="my-3" />
                <a :href="alert.url" target="_blank" rel="noopener noreferrer">
                  {{ alert.urlTitle || alert.url }}
                </a>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
