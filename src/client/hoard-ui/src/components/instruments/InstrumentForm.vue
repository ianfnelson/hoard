<script setup lang="ts">
import { reactive, computed, watch, onMounted } from 'vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import type { InstrumentDetailDto } from '@/api/dtos/Instruments/InstrumentDetailDto'
import type { InstrumentDataFeedWriteDto } from '@/api/dtos/Instruments/InstrumentDataFeedWriteDto'

const props = defineProps<{
  instrument: InstrumentDetailDto | null
  isSaving: boolean
}>()

const emit = defineEmits<{
  save: [payload: Record<string, unknown>]
  cancel: []
}>()

const refStore = useReferenceDataStore()

const isEditMode = computed(() => props.instrument !== null)

// Data feed constants
const DATA_FEED_NEWS = 1
const DATA_FEED_PRICES = 2
const DATA_FEED_QUOTES = 3
const TICKER_MAX_LENGTH = 20

// Form state
const formData = reactive({
  name: '',
  instrumentTypeId: null as number | null,
  assetClassId: null as number | null,
  assetSubclassId: null as number | null,
  currencyId: null as string | null,
  ticker: '',
  isin: '',
  dataFeeds: [] as InstrumentDataFeedWriteDto[],
})

// Asset class/subclass interdependency
const availableSubclasses = computed(() => {
  if (!formData.assetClassId) return []
  const ac = refStore.assetClasses.find((c) => c.id === formData.assetClassId)
  return ac?.subclasses ?? []
})

// Data feed helpers
function initializeDataFeeds() {
  formData.dataFeeds = [
    { dataFeedId: DATA_FEED_NEWS, enabled: false, ticker: null, importStartUtc: null },
    { dataFeedId: DATA_FEED_PRICES, enabled: false, ticker: null, importStartUtc: null },
    { dataFeedId: DATA_FEED_QUOTES, enabled: false, ticker: null, importStartUtc: null },
  ]
}

function getFeed(feedId: number) {
  return formData.dataFeeds.find((f) => f.dataFeedId === feedId)
}

const dataFeedTickerRules = [
  (v: string) => !!v?.trim() || 'Ticker is required',
  (v: string) => !v || v.length <= TICKER_MAX_LENGTH || `Maximum ${TICKER_MAX_LENGTH} characters`,
]

// Computed accessors for each feed
const newsEnabled = computed({
  get: () => getFeed(DATA_FEED_NEWS)?.enabled ?? false,
  set: (val) => {
    const feed = getFeed(DATA_FEED_NEWS)
    if (feed) feed.enabled = val
  },
})

const newsTicker = computed({
  get: () => getFeed(DATA_FEED_NEWS)?.ticker ?? '',
  set: (val) => {
    const feed = getFeed(DATA_FEED_NEWS)
    if (feed) feed.ticker = val
  },
})

const newsImportStart = computed({
  get: () => getFeed(DATA_FEED_NEWS)?.importStartUtc ?? null,
  set: (val) => {
    const feed = getFeed(DATA_FEED_NEWS)
    if (feed) feed.importStartUtc = val
  },
})

const pricesEnabled = computed({
  get: () => getFeed(DATA_FEED_PRICES)?.enabled ?? false,
  set: (val) => {
    const feed = getFeed(DATA_FEED_PRICES)
    if (feed) feed.enabled = val
  },
})

const pricesTicker = computed({
  get: () => getFeed(DATA_FEED_PRICES)?.ticker ?? '',
  set: (val) => {
    const feed = getFeed(DATA_FEED_PRICES)
    if (feed) feed.ticker = val
  },
})

const quotesEnabled = computed({
  get: () => getFeed(DATA_FEED_QUOTES)?.enabled ?? false,
  set: (val) => {
    const feed = getFeed(DATA_FEED_QUOTES)
    if (feed) feed.enabled = val
  },
})

const quotesTicker = computed({
  get: () => getFeed(DATA_FEED_QUOTES)?.ticker ?? '',
  set: (val) => {
    const feed = getFeed(DATA_FEED_QUOTES)
    if (feed) feed.ticker = val
  },
})

// When asset class changes, clear subclass if it's not in the new list
watch(
  () => formData.assetClassId,
  (newVal, oldVal) => {
    if (oldVal !== null && newVal !== oldVal) {
      const stillValid = availableSubclasses.value.some((s) => s.id === formData.assetSubclassId)
      if (!stillValid) {
        formData.assetSubclassId = null
      }
    }
  }
)

// Load form data when instrument is provided (edit mode)
watch(
  () => props.instrument,
  (inst) => {
    if (inst) {
      formData.name = inst.name
      formData.instrumentTypeId = inst.instrumentTypeId
      formData.assetClassId = inst.assetClassId
      formData.assetSubclassId = inst.assetSubclassId
      formData.currencyId = inst.currencyId
      formData.ticker = inst.ticker
      formData.isin = inst.isin ?? ''

      initializeDataFeeds()

      inst.dataFeeds?.forEach((df) => {
        const feed = getFeed(df.dataFeedId)
        if (feed) {
          feed.enabled = true
          feed.ticker = df.ticker
          if (df.dataFeedId === DATA_FEED_NEWS) {
            feed.importStartUtc = df.importStartUtc ?? null
          }
        }
      })
    }
  },
  { immediate: true }
)

// In create mode, initialize empty feeds
if (!props.instrument) {
  initializeDataFeeds()
}

// Field length constraints
const NAME_MAX_LENGTH = 100
const ISIN_MAX_LENGTH = 12

// Validation rules
const nameRules = [
  (v: string) => !!v.trim() || 'Name is required',
  (v: string) => v.length <= NAME_MAX_LENGTH || `Name cannot exceed ${NAME_MAX_LENGTH} characters`,
]

const tickerRules = [
  (v: string) => !!v.trim() || 'Ticker is required',
  (v: string) =>
    v.length <= TICKER_MAX_LENGTH || `Ticker cannot exceed ${TICKER_MAX_LENGTH} characters`,
]

const isinRules = [
  (v: string) =>
    !v || v.length <= ISIN_MAX_LENGTH || `ISIN cannot exceed ${ISIN_MAX_LENGTH} characters`,
]

const isFormValid = computed(() => {
  const basicFieldsValid =
    formData.name.trim() !== '' &&
    formData.name.length <= NAME_MAX_LENGTH &&
    formData.instrumentTypeId !== null &&
    formData.assetSubclassId !== null &&
    formData.currencyId !== null &&
    formData.ticker.trim() !== '' &&
    formData.ticker.length <= TICKER_MAX_LENGTH &&
    (!formData.isin || formData.isin.length <= ISIN_MAX_LENGTH)

  const dataFeedsValid = formData.dataFeeds.every((feed) => {
    if (!feed.enabled) return true
    return feed.ticker && feed.ticker.trim() !== '' && feed.ticker.length <= TICKER_MAX_LENGTH
  })

  return basicFieldsValid && dataFeedsValid
})

function handleSave() {
  if (!isFormValid.value) return

  emit('save', {
    name: formData.name.trim(),
    instrumentTypeId: formData.instrumentTypeId,
    assetSubclassId: formData.assetSubclassId,
    currencyId: formData.currencyId,
    ticker: formData.ticker.trim(),
    isin: formData.isin.trim() || null,
    dataFeeds: formData.dataFeeds.map((feed) => ({
      dataFeedId: feed.dataFeedId,
      enabled: feed.enabled,
      ticker: feed.ticker?.trim() || null,
      importStartUtc: feed.importStartUtc || null,
    })),
  })
}

onMounted(() => {
  refStore.loadInstrumentTypes()
  refStore.loadAssetClasses()
  refStore.loadCurrencies()
})
</script>

<template>
  <v-card>
    <v-card-title>{{ isEditMode ? 'Edit Instrument' : 'New Instrument' }}</v-card-title>
    <v-card-text>
      <v-form>
        <!-- Row 1: Name and Ticker -->
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-text-field
              v-model="formData.name"
              label="Name"
              :rules="nameRules"
              :counter="NAME_MAX_LENGTH"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col cols="12" sm="6">
            <v-text-field
              v-model="formData.ticker"
              label="Ticker"
              :rules="tickerRules"
              :counter="TICKER_MAX_LENGTH"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Row 2: Instrument Type and Currency -->
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-select
              v-model="formData.instrumentTypeId"
              :items="refStore.instrumentTypes"
              item-title="name"
              item-value="id"
              label="Instrument Type"
              :rules="[(v) => !!v || 'Instrument Type is required']"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col cols="12" sm="6">
            <v-select
              v-model="formData.currencyId"
              :items="refStore.currencies"
              item-title="name"
              item-value="id"
              label="Currency"
              :rules="[(v) => !!v || 'Currency is required']"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Row 3: Asset Class and Subclass -->
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-select
              v-model="formData.assetClassId"
              :items="refStore.assetClasses"
              item-title="name"
              item-value="id"
              label="Asset Class"
              :rules="[(v) => !!v || 'Asset Class is required']"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col cols="12" sm="6">
            <v-select
              v-model="formData.assetSubclassId"
              :items="availableSubclasses"
              item-title="name"
              item-value="id"
              label="Asset Subclass"
              :rules="[(v) => !!v || 'Asset Subclass is required']"
              :disabled="!formData.assetClassId"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Row 4: ISIN -->
        <v-row dense>
          <v-col cols="12" sm="6">
            <v-text-field
              v-model="formData.isin"
              label="ISIN"
              :rules="isinRules"
              :counter="ISIN_MAX_LENGTH"
              variant="outlined"
              density="compact"
              hint="Optional - International Securities Identification Number"
              persistent-hint
            />
          </v-col>
        </v-row>

        <!-- Data Feeds Configuration -->
        <v-row dense>
          <v-col cols="12">
            <v-card variant="outlined" class="mt-4">
              <v-card-title class="text-h6">Data Feeds</v-card-title>
              <v-card-subtitle class="text-caption">
                Configure automatic data refresh from external sources
              </v-card-subtitle>
              <v-card-text>
                <v-row dense>
                  <!-- News Feed -->
                  <v-col cols="12" md="4">
                    <v-card variant="tonal" class="pa-3">
                      <v-checkbox
                        v-model="newsEnabled"
                        label="Enable News Updates"
                        density="compact"
                        hide-details
                      />
                      <v-text-field
                        v-if="newsEnabled"
                        v-model="newsTicker"
                        label="News Ticker"
                        :rules="dataFeedTickerRules"
                        :counter="TICKER_MAX_LENGTH"
                        density="compact"
                        variant="outlined"
                        class="mt-2"
                        required
                      />
                      <v-text-field
                        v-if="newsEnabled"
                        v-model="newsImportStart"
                        label="Import Start Date (UTC)"
                        type="datetime-local"
                        density="compact"
                        variant="outlined"
                        hint="Optional - only import news from this date onwards"
                        persistent-hint
                        class="mt-2"
                      />
                    </v-card>
                  </v-col>

                  <!-- Prices Feed -->
                  <v-col cols="12" md="4">
                    <v-card variant="tonal" class="pa-3">
                      <v-checkbox
                        v-model="pricesEnabled"
                        label="Enable Price Updates"
                        density="compact"
                        hide-details
                      />
                      <v-text-field
                        v-if="pricesEnabled"
                        v-model="pricesTicker"
                        label="Prices Ticker"
                        :rules="dataFeedTickerRules"
                        :counter="TICKER_MAX_LENGTH"
                        density="compact"
                        variant="outlined"
                        class="mt-2"
                        required
                      />
                    </v-card>
                  </v-col>

                  <!-- Quotes Feed -->
                  <v-col cols="12" md="4">
                    <v-card variant="tonal" class="pa-3">
                      <v-checkbox
                        v-model="quotesEnabled"
                        label="Enable Quote Updates"
                        density="compact"
                        hide-details
                      />
                      <v-text-field
                        v-if="quotesEnabled"
                        v-model="quotesTicker"
                        label="Quotes Ticker"
                        :rules="dataFeedTickerRules"
                        :counter="TICKER_MAX_LENGTH"
                        density="compact"
                        variant="outlined"
                        class="mt-2"
                        required
                      />
                    </v-card>
                  </v-col>
                </v-row>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-btn variant="flat" prepend-icon="mdi-close" @click="emit('cancel')">Cancel</v-btn>
      <v-btn
        color="primary"
        variant="flat"
        prepend-icon="mdi-content-save"
        :disabled="!isFormValid"
        :loading="isSaving"
        @click="handleSave"
      >
        Save
      </v-btn>
    </v-card-actions>
  </v-card>
</template>
