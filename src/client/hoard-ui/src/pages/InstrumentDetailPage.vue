<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useInstrumentStore } from '@/stores/instrumentStore'
import { useInstrumentDetail } from '@/composables/useInstrumentDetail'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNewsArticles } from '@/composables/useNewsArticles'
import { useInstrumentPrices } from '@/composables/useInstrumentPrices'
import { useTransactions } from '@/composables/useTransactions'
import { usePageTitle } from '@/composables/usePageTitle'
import {
  formatDateTime,
  formatDate,
  formatCurrency,
  formatPercentage,
  getTrendClass,
  formatUpdatedTime,
} from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'
import type { PriceSummaryDto } from '@/api/dtos/Instruments/PriceSummaryDto'
import type { InstrumentDataFeedWriteDto } from '@/api/dtos/Instruments/InstrumentDataFeedWriteDto'
import { useContractNoteDownload } from '@/composables/useContractNoteDownload'
import PriceChart from '@/components/PriceChart.vue'

// Data feed constants (must match backend DataFeed.cs)
const DATA_FEED_NEWS = 1
const DATA_FEED_PRICES = 2
const DATA_FEED_QUOTES = 3
const TICKER_MAX_LENGTH = 20

const route = useRoute()
const router = useRouter()

const props = defineProps<{ id: string }>()
const instrumentId = computed(() => (props.id === 'new' ? null : Number(props.id)))

// Stores and composables
const instrumentStore = useInstrumentStore()
const refStore = useReferenceDataStore()
const { isSaving, isDeleting, error, create, update, remove } = useInstrumentDetail()

// Mode detection
const mode = computed(() => {
  const queryMode = route.query.mode as string
  if (queryMode === 'edit' || queryMode === 'create') return queryMode
  return 'view'
})
const isCreateMode = computed(() => mode.value === 'create')
const isEditMode = computed(() => mode.value === 'edit')
const isViewMode = computed(() => mode.value === 'view')
const isFormMode = computed(() => isEditMode.value || isCreateMode.value)

// Page title
const pageTitle = computed(() => {
  if (isCreateMode.value) return 'New Instrument'
  if (isEditMode.value) return `Edit ${instrumentStore.instrument?.name ?? 'Instrument'}`
  return instrumentStore.instrument?.name ?? 'Instrument'
})
usePageTitle(pageTitle)

// Loading state from store for view mode
const isLoading = computed(() => instrumentStore.isLoading)

// Form state
const formData = reactive({
  name: '',
  instrumentTypeId: null as number | null,
  assetClassId: null as number | null,
  assetSubclassId: null as number | null,
  currencyId: null as string | null,
  tickerDisplay: '',
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

// Validation rules for data feed ticker (required when feed is enabled)
const dataFeedTickerRules = [
  (v: string) => !!v?.trim() || 'Ticker is required',
  (v: string) => !v || v.length <= TICKER_MAX_LENGTH || `Maximum ${TICKER_MAX_LENGTH} characters`,
]

// Computed accessors for each feed (for v-model binding)
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

// Load form data when editing
watch(
  [() => instrumentStore.instrument, isEditMode],
  ([inst, editMode]) => {
    if (inst && editMode) {
      formData.name = inst.name
      formData.instrumentTypeId = inst.instrumentTypeId
      formData.assetClassId = inst.assetClassId
      formData.assetSubclassId = inst.assetSubclassId
      formData.currencyId = inst.currencyId
      formData.tickerDisplay = inst.tickerDisplay
      formData.isin = inst.isin ?? ''

      // Initialize data feeds
      initializeDataFeeds()

      // Load existing feed configurations
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
watch(isCreateMode, (creating) => {
  if (creating) {
    initializeDataFeeds()
  }
})

// Field length constraints (matching server validation)
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
  // Validate basic fields
  const basicFieldsValid =
    formData.name.trim() !== '' &&
    formData.name.length <= NAME_MAX_LENGTH &&
    formData.instrumentTypeId !== null &&
    formData.assetSubclassId !== null &&
    formData.currencyId !== null &&
    formData.tickerDisplay.trim() !== '' &&
    formData.tickerDisplay.length <= TICKER_MAX_LENGTH &&
    (!formData.isin || formData.isin.length <= ISIN_MAX_LENGTH)

  // Validate data feeds - if enabled, ticker is required
  const dataFeedsValid = formData.dataFeeds.every((feed) => {
    if (!feed.enabled) return true
    return feed.ticker && feed.ticker.trim() !== '' && feed.ticker.length <= TICKER_MAX_LENGTH
  })

  return basicFieldsValid && dataFeedsValid
})

// Delete dialog
const showDeleteDialog = ref(false)

// Actions
async function handleSave() {
  if (!isFormValid.value) return

  const payload = {
    name: formData.name.trim(),
    instrumentTypeId: formData.instrumentTypeId,
    assetSubclassId: formData.assetSubclassId,
    currencyId: formData.currencyId,
    tickerDisplay: formData.tickerDisplay.trim(),
    isin: formData.isin.trim() || null,
    dataFeeds: formData.dataFeeds.map((feed) => ({
      dataFeedId: feed.dataFeedId,
      enabled: feed.enabled,
      ticker: feed.ticker?.trim() || null,
      importStartUtc: feed.importStartUtc || null,
    })),
  }

  if (isCreateMode.value) {
    const id = await create(payload)
    if (id) {
      router.push({ name: 'instrument-detail', params: { id }, query: {} })
    }
  } else if (instrumentId.value) {
    const success = await update(instrumentId.value, payload)
    if (success) {
      await instrumentStore.load(instrumentId.value)
      router.push({ query: {} })
    }
  }
}

function handleCancel() {
  if (isCreateMode.value) {
    router.push({ name: 'instruments' })
  } else {
    router.push({ query: {} })
  }
}

async function confirmDelete() {
  if (!instrumentId.value) return
  const success = await remove(instrumentId.value)
  if (success) {
    router.push({ name: 'instruments' })
  }
}

// News
const {
  items: newsItems,
  totalCount: newsTotalCount,
  isLoading: newsIsLoading,
  fetchNewsArticles,
} = useNewsArticles()
const newsPage = ref(1)
const newsItemsPerPage = ref(10)
const newsSortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'publishedUtc', order: 'desc' },
])

const newsHeaders = [
  { title: 'Published', key: 'publishedUtc', sortable: true },
  { title: 'Headline', key: 'headline', sortable: true },
] as const

function viewArticle(item: NewsArticleSummaryDto) {
  router.push({ name: 'news-article-detail', params: { id: item.id } })
}

function viewPrice(item: PriceSummaryDto) {
  router.push({ name: 'price-detail', params: { id: item.id } })
}

function createNewPrice() {
  if (!instrumentId.value) return
  router.push({
    name: 'price-detail',
    params: { id: 'new' },
    query: { mode: 'create', instrumentId: instrumentId.value.toString() },
  })
}

watch(
  [newsPage, newsItemsPerPage, newsSortBy, instrumentId],
  () => {
    if (instrumentId.value && isViewMode.value) {
      fetchNewsArticles({
        instrumentId: instrumentId.value,
        pageNumber: newsPage.value,
        pageSize: newsItemsPerPage.value === -1 ? undefined : newsItemsPerPage.value,
        sortBy: newsSortBy.value[0]?.key,
        sortDirection: newsSortBy.value[0]?.order,
      })
    }
  },
  { immediate: true }
)

// Prices
const {
  items: priceItems,
  totalCount: priceTotalCount,
  isLoading: priceIsLoading,
  fetchPrices,
} = useInstrumentPrices()
const pricePage = ref(1)
const priceItemsPerPage = ref(10)

const priceHeaders = [
  { title: 'Date', key: 'asOfDate', sortable: false },
  { title: 'Open', key: 'open', sortable: false, align: 'end' },
  { title: 'High', key: 'high', sortable: false, align: 'end' },
  { title: 'Low', key: 'low', sortable: false, align: 'end' },
  { title: 'Close', key: 'close', sortable: false, align: 'end' },
  { title: 'Adjusted Close', key: 'adjustedClose', sortable: false, align: 'end' },
  { title: 'Volume', key: 'volume', sortable: false, align: 'end' },
] as const

watch(
  [pricePage, priceItemsPerPage, instrumentId],
  () => {
    if (instrumentId.value && isViewMode.value) {
      fetchPrices(instrumentId.value, {
        pageNumber: pricePage.value,
        pageSize: priceItemsPerPage.value === -1 ? undefined : priceItemsPerPage.value,
      })
    }
  },
  { immediate: true }
)

// Transactions
const {
  items: txnItems,
  totalCount: txnTotalCount,
  isLoading: txnIsLoading,
  fetchTransactions,
} = useTransactions()
const txnPage = ref(1)
const txnItemsPerPage = ref(10)
const txnSortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'date', order: 'desc' },
])

const txnHeaders = [
  { title: 'Date', key: 'date', sortable: true },
  { title: 'Account', key: 'accountName', sortable: false },
  { title: 'Type', key: 'transactionTypeName', sortable: false },
  { title: 'Units', key: 'units', sortable: false, align: 'end' },
  { title: 'Price', key: 'price', sortable: false, align: 'end' },
  { title: 'Value', key: 'value', sortable: true, align: 'end' },
  { title: '', key: 'contractNoteReference', sortable: false },
] as const

watch(
  [txnPage, txnItemsPerPage, txnSortBy, instrumentId],
  () => {
    if (instrumentId.value && isViewMode.value) {
      fetchTransactions({
        instrumentId: instrumentId.value,
        pageNumber: txnPage.value,
        pageSize: txnItemsPerPage.value === -1 ? undefined : txnItemsPerPage.value,
        sortBy: txnSortBy.value[0]?.key,
        sortDirection: txnSortBy.value[0]?.order,
      })
    }
  },
  { immediate: true }
)

// Load instrument when ID changes (handles navigation from create to view)
watch(
  instrumentId,
  async (newId) => {
    if (newId) {
      await instrumentStore.load(newId)
    }
  },
  { immediate: true }
)

// Contract note download
const { handleDownloadContractNote, isDownloading } = useContractNoteDownload()

// Initialize reference data
onMounted(() => {
  refStore.loadInstrumentTypes()
  refStore.loadAssetClasses()
  refStore.loadCurrencies()
})
</script>

<template>
  <v-container fluid>
    <v-row v-if="error" dense>
      <v-col>
        <v-alert type="error" dismissible @click:close="error = null">
          {{ error }}
        </v-alert>
      </v-col>
    </v-row>

    <v-row v-if="isLoading && !isCreateMode" dense>
      <v-col>
        <v-progress-linear indeterminate />
      </v-col>
    </v-row>

    <v-row v-if="isViewMode && instrumentId" align="stretch" dense>
      <!-- Instrument Details Card -->
      <v-col cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>{{ instrumentStore.instrument?.name }}</v-card-title>
          <v-card-text>
            <v-row dense>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Ticker</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.tickerDisplay }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">ISIN</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.isin ?? '—' }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Currency</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.currencyName }}</div>
              </v-col>
            </v-row>
            <v-divider class="my-3" />
            <v-row dense>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Type</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.instrumentTypeName }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Class</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.assetClassName }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Subclass</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.assetSubclassName }}</div>
              </v-col>
            </v-row>
            <v-divider class="my-3" />
            <v-row v-if="instrumentStore.instrument?.quote" dense>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Price</div>
                <div class="text-body-1">
                  <span
                    :class="getTrendClass(instrumentStore.instrument.quote.regularMarketChange)"
                  >
                    {{ instrumentStore.instrument.quote.regularMarketPrice }}
                  </span>
                </div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Change</div>
                <div class="text-body-1">
                  <span
                    :class="getTrendClass(instrumentStore.instrument.quote.regularMarketChange)"
                  >
                    {{ instrumentStore.instrument.quote.regularMarketChange }}
                  </span>
                </div>
              </v-col>

              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Change (%)</div>
                <div class="text-body-1">
                  <span
                    :class="getTrendClass(instrumentStore.instrument.quote.regularMarketChange)"
                  >
                    {{
                      formatPercentage(instrumentStore.instrument.quote.regularMarketChangePercent)
                    }}
                  </span>
                </div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Retrieved</div>
                <div class="text-body-1">
                  {{ formatUpdatedTime(instrumentStore.instrument?.quote.retrievedUtc) }}
                </div>
              </v-col>
            </v-row>
            <v-divider v-if="instrumentStore.instrument?.quote" class="my-3" />
            <v-row v-if="instrumentStore.instrument?.quote" dense>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Bid</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.quote.bid }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Ask</div>
                <div class="text-body-1">{{ instrumentStore.instrument?.quote.ask }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">52-week Low</div>
                <div class="text-body-1">
                  {{ instrumentStore.instrument?.quote.fiftyTwoWeekLow }}
                </div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">52-week High</div>
                <div class="text-body-1">
                  {{ instrumentStore.instrument?.quote.fiftyTwoWeekHigh }}
                </div>
              </v-col>
            </v-row>
            <!-- Active Data Feeds -->
            <div v-if="instrumentStore.instrument?.dataFeeds?.length">
              <v-divider class="my-3" />
              <v-row dense>
                <v-col cols="12">
                  <div class="text-caption text-medium-emphasis mb-1">Active Data Feeds</div>
                  <v-chip-group>
                    <v-chip
                      v-for="feed in instrumentStore.instrument.dataFeeds"
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
            <v-btn
              color="primary"
              variant="flat"
              prepend-icon="mdi-pencil"
              @click="router.push({ query: { mode: 'edit' } })"
            >
              Edit
            </v-btn>
            <v-btn
              color="error"
              variant="flat"
              prepend-icon="mdi-delete"
              @click="showDeleteDialog = true"
            >
              Delete
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <!-- Price Chart Card -->
      <v-col cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>Price History</v-card-title>
          <v-card-text>
            <PriceChart :instrument-id="instrumentId" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-row v-if="isViewMode" align="stretch" dense>
      <!-- Prices Card -->
      <v-col v-if="priceIsLoading || priceTotalCount > 0" cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>Prices</v-card-title>
          <v-card-text>
            <v-data-table-server
              v-model:page="pricePage"
              v-model:items-per-page="priceItemsPerPage"
              :headers="priceHeaders"
              :items="priceItems"
              :items-length="priceTotalCount"
              :loading="priceIsLoading"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              density="compact"
            >
              <template #item.asOfDate="{ item, value }">
                <a href="#" class="text-decoration-none" @click.prevent="viewPrice(item)">
                  <span style="white-space: nowrap">{{ formatDate(value) }}</span>
                </a>
              </template>

              <template #item.open="{ value }">
                <span v-if="value !== null">{{ value }}</span>
              </template>

              <template #item.high="{ value }">
                <span v-if="value !== null">{{ value }}</span>
              </template>

              <template #item.low="{ value }">
                <span v-if="value !== null">{{ value }}</span>
              </template>

              <template #item.volume="{ value }">
                <span v-if="value !== null">{{ value }}</span>
              </template>

              <template #footer.prepend>
                <div class="d-flex" style="flex: 1">
                  <v-btn
                    v-if="isViewMode"
                    color="primary"
                    variant="flat"
                    size="small"
                    prepend-icon="mdi-plus"
                    class="ml-2"
                    @click="createNewPrice"
                  >
                    New Price
                  </v-btn>
                </div>
              </template>

              <template #no-data> No prices found </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
      <!-- Transactions Card -->
      <v-col v-if="txnIsLoading || txnTotalCount > 0" cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>Transactions</v-card-title>
          <v-card-text>
            <v-data-table-server
              v-model:page="txnPage"
              v-model:items-per-page="txnItemsPerPage"
              v-model:sort-by="txnSortBy"
              :headers="txnHeaders"
              :items="txnItems"
              :items-length="txnTotalCount"
              :loading="txnIsLoading"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              density="compact"
            >
              <template #item.date="{ item, value }">
                <router-link
                  :to="{ name: 'transaction-detail', params: { id: item.id } }"
                  class="text-decoration-none"
                >
                  <span style="white-space: nowrap">{{ formatDate(value) }}</span>
                </router-link>
              </template>

              <template #item.value="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #item.contractNoteReference="{ item, value }">
                <v-btn
                  v-if="value"
                  icon
                  size="small"
                  variant="text"
                  :loading="isDownloading(value)"
                  @click.stop="handleDownloadContractNote(item.id, value)"
                >
                  <v-icon class="doc-link-icon">mdi-file-document</v-icon>
                  <v-tooltip activator="parent" location="top">
                    Download {{ value }}.pdf
                  </v-tooltip>
                </v-btn>
              </template>

              <template #no-data> No transactions found </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-row v-if="isViewMode" align="stretch" dense>
      <!-- News Card -->
      <v-col v-if="newsIsLoading || newsTotalCount > 0" cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>News</v-card-title>
          <v-card-text>
            <v-data-table-server
              v-model:page="newsPage"
              v-model:items-per-page="newsItemsPerPage"
              v-model:sort-by="newsSortBy"
              :headers="newsHeaders"
              :items="newsItems"
              :items-length="newsTotalCount"
              :loading="newsIsLoading"
              :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
              density="compact"
            >
              <template #item.publishedUtc="{ item, value }">
                <router-link
                  :to="{ name: 'news-article-detail', params: { id: item.id } }"
                  class="text-decoration-none"
                >
                  <span style="white-space: nowrap">{{ formatDateTime(value) }}</span>
                </router-link>
              </template>

              <template #item.headline="{ item }">
                <a href="#" class="text-decoration-none" @click.prevent="viewArticle(item)">
                  {{ item.headline }}
                </a>
              </template>

              <template #no-data> No news articles found </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Form Mode -->
    <v-row v-if="isFormMode" dense>
      <v-col cols="12" lg="8">
        <v-card>
          <v-card-title>{{ isCreateMode ? 'New Instrument' : 'Edit Instrument' }}</v-card-title>
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
                    v-model="formData.tickerDisplay"
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
            <v-btn variant="flat" prepend-icon="mdi-close" @click="handleCancel">Cancel</v-btn>
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
      </v-col>
    </v-row>

    <!-- Delete Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="500">
      <v-card>
        <v-card-title>Delete Instrument?</v-card-title>
        <v-card-text>
          Are you sure you want to delete this instrument? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="flat" prepend-icon="mdi-close" @click="showDeleteDialog = false">
            Cancel
          </v-btn>
          <v-btn
            color="error"
            variant="flat"
            prepend-icon="mdi-delete"
            :loading="isDeleting"
            @click="confirmDelete"
          >
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>
