<script setup lang="ts">
import type { PriceDetailDto } from '@/api/dtos/Prices/PriceDetailDto'
import { formatDateTime, formatDate } from '@/utils/formatters'

defineProps<{
  price: PriceDetailDto
}>()

defineEmits<{
  edit: []
  delete: []
}>()
</script>

<template>
  <v-card>
    <v-card-title>Price Detail</v-card-title>

    <v-card-text>
      <v-row>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">As Of Date</div>
          <div class="text-body-1">{{ formatDate(price.asOfDate) }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Source</div>
          <div class="text-body-1">{{ price.source }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Is Locked</div>
          <div class="text-body-1">{{ price.isLocked ? 'Yes' : 'No' }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Retrieved UTC</div>
          <div class="text-body-1">{{ formatDateTime(price.retrievedUtc) }}</div>
        </v-col>
      </v-row>

      <v-divider class="my-3" />

      <v-row>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Open</div>
          <div class="text-body-1">{{ price.open ?? '—' }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">High</div>
          <div class="text-body-1">{{ price.high ?? '—' }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Low</div>
          <div class="text-body-1">{{ price.low ?? '—' }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Close</div>
          <div class="text-body-1">{{ price.close }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Adjusted Close</div>
          <div class="text-body-1">{{ price.adjustedClose }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Volume</div>
          <div class="text-body-1">{{ price.volume ?? '—' }}</div>
        </v-col>
      </v-row>
      <v-divider class="my-3" />
      <v-row>
        <v-col cols="12" md="6">
          <div class="text-caption text-medium-emphasis">Instrument</div>
          <div class="text-body-1">
            <router-link
              :to="{ name: 'instrument-detail', params: { id: price.instrumentId } }"
              class="text-decoration-none"
            >
              {{ price.instrumentName }} ({{ price.instrumentTicker }})
            </router-link>
          </div>
        </v-col>
      </v-row>
    </v-card-text>

    <v-card-actions>
      <v-btn color="primary" variant="flat" prepend-icon="mdi-pencil" @click="$emit('edit')">
        Edit
      </v-btn>
      <v-btn color="error" variant="flat" prepend-icon="mdi-delete" @click="$emit('delete')">
        Delete
      </v-btn>
    </v-card-actions>
  </v-card>
</template>
