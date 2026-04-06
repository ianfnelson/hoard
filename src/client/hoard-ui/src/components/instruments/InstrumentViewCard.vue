<script setup lang="ts">
import type { InstrumentDetailDto } from '@/api/dtos/Instruments/InstrumentDetailDto'
import { formatPercentage, getTrendClass, formatUpdatedTime } from '@/utils/formatters'

defineProps<{
  instrument: InstrumentDetailDto
}>()

defineEmits<{
  edit: []
  delete: []
}>()
</script>

<template>
  <v-card class="h-100">
    <v-card-title>{{ instrument.name }}</v-card-title>
    <v-card-text>
      <v-row dense>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Ticker</div>
          <div class="text-body-1">{{ instrument.ticker }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">ISIN</div>
          <div class="text-body-1">{{ instrument.isin ?? '—' }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Currency</div>
          <div class="text-body-1">{{ instrument.currencyName }}</div>
        </v-col>
      </v-row>
      <v-divider class="my-3" />
      <v-row dense>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Type</div>
          <div class="text-body-1">{{ instrument.instrumentTypeName }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Class</div>
          <div class="text-body-1">{{ instrument.assetClassName }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Subclass</div>
          <div class="text-body-1">{{ instrument.assetSubclassName }}</div>
        </v-col>
      </v-row>
      <v-divider class="my-3" />

      <!-- Quote -->
      <v-row v-if="instrument.quote" dense>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Price</div>
          <div class="text-body-1">
            <span :class="getTrendClass(instrument.quote.regularMarketChange)">
              {{ instrument.quote.regularMarketPrice }}
            </span>
          </div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Change</div>
          <div class="text-body-1">
            <span :class="getTrendClass(instrument.quote.regularMarketChange)">
              {{ instrument.quote.regularMarketChange }}
            </span>
          </div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Change (%)</div>
          <div class="text-body-1">
            <span :class="getTrendClass(instrument.quote.regularMarketChange)">
              {{ formatPercentage(instrument.quote.regularMarketChangePercent) }}
            </span>
          </div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Retrieved</div>
          <div class="text-body-1">
            {{ formatUpdatedTime(instrument.quote.retrievedUtc) }}
          </div>
        </v-col>
      </v-row>
      <v-divider v-if="instrument.quote" class="my-3" />
      <v-row v-if="instrument.quote" dense>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Bid</div>
          <div class="text-body-1">{{ instrument.quote.bid }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">Ask</div>
          <div class="text-body-1">{{ instrument.quote.ask }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">52-week Low</div>
          <div class="text-body-1">{{ instrument.quote.fiftyTwoWeekLow }}</div>
        </v-col>
        <v-col cols="12" sm="3">
          <div class="text-caption text-medium-emphasis">52-week High</div>
          <div class="text-body-1">{{ instrument.quote.fiftyTwoWeekHigh }}</div>
        </v-col>
      </v-row>

      <!-- Data Feeds -->
      <div v-if="instrument.dataFeeds?.length">
        <v-divider class="my-3" />
        <v-row dense>
          <v-col cols="12">
            <div class="text-caption text-medium-emphasis mb-1">Active Data Feeds</div>
            <v-chip-group>
              <v-chip
                v-for="feed in instrument.dataFeeds"
                :key="feed.dataFeedId"
                size="small"
                variant="tonal"
                style="cursor: default"
              >
                <strong>{{ feed.dataFeedName }}:</strong> {{ feed.ticker }}
                <span v-if="feed.importStartUtc" class="ml-1">
                  (from {{ new Date(feed.importStartUtc).toLocaleDateString() }})
                </span>
              </v-chip>
            </v-chip-group>
          </v-col>
        </v-row>
      </div>
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
