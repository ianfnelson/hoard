<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useInstrumentStore } from '@/stores/instrumentStore'
import { useNewsArticles } from '@/composables/useNewsArticles'
import { useInstrumentPrices } from '@/composables/useInstrumentPrices'
import { useTransactions } from '@/composables/useTransactions'
import {
  formatDateTime,
  formatDate,
  formatCurrency,
  formatPercentage,
  getTrendClass,
} from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'

const router = useRouter()

const props = defineProps<{ id: string }>()
const instrumentId = Number(props.id)

// Instrument detail
const instrumentStore = useInstrumentStore()
instrumentStore.load(instrumentId)

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

watch(
  [newsPage, newsItemsPerPage, newsSortBy],
  () => {
    fetchNewsArticles({
      instrumentId,
      pageNumber: newsPage.value,
      pageSize: newsItemsPerPage.value === -1 ? undefined : newsItemsPerPage.value,
      sortBy: newsSortBy.value[0]?.key,
      sortDirection: newsSortBy.value[0]?.order,
    })
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
  [pricePage, priceItemsPerPage],
  () => {
    fetchPrices(instrumentId, {
      pageNumber: pricePage.value,
      pageSize: priceItemsPerPage.value === -1 ? undefined : priceItemsPerPage.value,
    })
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
  { title: 'Contract Note', key: 'contractNoteReference', sortable: false },
] as const

watch(
  [txnPage, txnItemsPerPage, txnSortBy],
  () => {
    fetchTransactions({
      instrumentId,
      pageNumber: txnPage.value,
      pageSize: txnItemsPerPage.value === -1 ? undefined : txnItemsPerPage.value,
      sortBy: txnSortBy.value[0]?.key,
      sortDirection: txnSortBy.value[0]?.order,
    })
  },
  { immediate: true }
)
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <!-- Instrument Details Card -->
      <v-col cols="12" md="6" lg="3">
        <v-card>
          <v-card-title>{{ instrumentStore.instrument?.name }}</v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="6">
                <v-list density="compact" lines="two">
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.tickerDisplay }}</template>
                    <template #subtitle>Ticker</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.isin ?? '—' }}</template>
                    <template #subtitle>ISIN</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.currencyName }}</template>
                    <template #subtitle>Currency</template>
                  </v-list-item>
                </v-list>
              </v-col>
              <v-col cols="6">
                <v-list density="compact" lines="two">
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.instrumentTypeName }}</template>
                    <template #subtitle>Type</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.assetClassName }}</template>
                    <template #subtitle>Class</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument?.assetSubclassName }}</template>
                    <template #subtitle>Subclass</template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
      <!-- Quote Card -->
      <v-col v-if="instrumentStore.instrument?.quote" cols="12" md="6" lg="3">
        <v-card>
          <v-card-title>
            <span :class="getTrendClass(instrumentStore.instrument.quote.regularMarketChange)">
              {{ instrumentStore.instrument.quote.regularMarketPrice }}
              {{ instrumentStore.instrument.quote.regularMarketChange }}
              {{
                formatPercentage(instrumentStore.instrument.quote.regularMarketChangePercent)
              }}</span
            ></v-card-title
          >
          <v-card-text>
            <v-row>
              <v-col cols="6">
                <v-list density="compact" lines="two">
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument.quote.ask }}</template>
                    <template #subtitle>Ask</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{ instrumentStore.instrument.quote.bid }}</template>
                    <template #subtitle>Bid</template>
                  </v-list-item>
                </v-list>
              </v-col>
              <v-col cols="6">
                <v-list density="compact" lines="two">
                  <v-list-item>
                    <template #title>{{
                      instrumentStore.instrument.quote.fiftyTwoWeekHigh
                    }}</template>
                    <template #subtitle>52-week high</template>
                  </v-list-item>
                  <v-list-item>
                    <template #title>{{
                      instrumentStore.instrument.quote.fiftyTwoWeekLow
                    }}</template>
                    <template #subtitle>52-week low</template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
      <!-- Transactions Card -->
      <v-col v-if="txnIsLoading || txnTotalCount > 0" cols="12" md="6">
        <v-card>
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
              <template #item.date="{ value }">
                <span style="white-space: nowrap">{{ formatDate(value) }}</span>
              </template>

              <template #item.value="{ value }">
                <span :class="getTrendClass(value)">{{ formatCurrency(value) }}</span>
              </template>

              <template #no-data> No transactions found </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-row dense>
      <!-- News Card -->
      <v-col v-if="newsIsLoading || newsTotalCount > 0" cols="12" md="6">
        <v-card>
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
              <template #item.publishedUtc="{ value }">
                <span style="white-space: nowrap">{{ formatDateTime(value) }}</span>
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
      <!-- Prices Card -->
      <v-col v-if="priceIsLoading || priceTotalCount > 0" cols="12" md="6">
        <v-card>
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
              <template #item.asOfDate="{ value }">
                <span style="white-space: nowrap">{{ formatDate(value) }}</span>
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

              <template #no-data> No prices found </template>
            </v-data-table-server>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
