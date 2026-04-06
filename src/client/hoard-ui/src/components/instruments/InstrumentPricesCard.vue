<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useInstrumentPrices } from '@/composables/useInstrumentPrices'
import { formatDate } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'
import type { PriceSummaryDto } from '@/api/dtos/Instruments/PriceSummaryDto'

const props = defineProps<{
  instrumentId: number
}>()

const router = useRouter()

const { items, totalCount, isLoading, fetchPrices } = useInstrumentPrices()

const page = ref(1)
const itemsPerPage = ref(10)

const headers = [
  { title: 'Date', key: 'asOfDate', sortable: false },
  { title: 'Open', key: 'open', sortable: false, align: 'end' },
  { title: 'High', key: 'high', sortable: false, align: 'end' },
  { title: 'Low', key: 'low', sortable: false, align: 'end' },
  { title: 'Close', key: 'close', sortable: false, align: 'end' },
  { title: 'Adjusted Close', key: 'adjustedClose', sortable: false, align: 'end' },
  { title: 'Volume', key: 'volume', sortable: false, align: 'end' },
] as const

function viewPrice(item: PriceSummaryDto) {
  router.push({ name: 'price-detail', params: { id: item.id } })
}

function createNewPrice() {
  router.push({
    name: 'price-detail',
    params: { id: 'new' },
    query: { mode: 'create', instrumentId: props.instrumentId.toString() },
  })
}

watch(
  [page, itemsPerPage, () => props.instrumentId],
  () => {
    fetchPrices(props.instrumentId, {
      pageNumber: page.value,
      pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    })
  },
  { immediate: true }
)
</script>

<template>
  <v-card class="h-100">
    <v-card-title>Prices</v-card-title>
    <v-card-text>
      <v-data-table-server
        v-model:page="page"
        v-model:items-per-page="itemsPerPage"
        :headers="headers"
        :items="items"
        :items-length="totalCount"
        :loading="isLoading"
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
</template>
