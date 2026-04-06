<script setup lang="ts">
import { ref, watch } from 'vue'
import { useTransactions } from '@/composables/useTransactions'
import { useContractNoteDownload } from '@/composables/useContractNoteDownload'
import { formatDate, formatCurrency, getTrendClass } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'

const props = defineProps<{
  instrumentId: number
}>()

const { items, totalCount, isLoading, fetchTransactions } = useTransactions()
const { handleDownloadContractNote, isDownloading } = useContractNoteDownload()

const page = ref(1)
const itemsPerPage = ref(10)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([{ key: 'date', order: 'desc' }])

const headers = [
  { title: 'Date', key: 'date', sortable: true },
  { title: 'Account', key: 'accountName', sortable: false },
  { title: 'Type', key: 'transactionTypeName', sortable: false },
  { title: 'Units', key: 'units', sortable: false, align: 'end' },
  { title: 'Price', key: 'price', sortable: false, align: 'end' },
  { title: 'Value', key: 'value', sortable: true, align: 'end' },
  { title: '', key: 'contractNoteReference', sortable: false },
] as const

watch(
  [page, itemsPerPage, sortBy, () => props.instrumentId],
  () => {
    fetchTransactions({
      instrumentId: props.instrumentId,
      pageNumber: page.value,
      pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
      sortBy: sortBy.value[0]?.key,
      sortDirection: sortBy.value[0]?.order,
    })
  },
  { immediate: true }
)
</script>

<template>
  <v-card class="h-100">
    <v-card-title>Transactions</v-card-title>
    <v-card-text>
      <v-data-table-server
        v-model:page="page"
        v-model:items-per-page="itemsPerPage"
        v-model:sort-by="sortBy"
        :headers="headers"
        :items="items"
        :items-length="totalCount"
        :loading="isLoading"
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
            <v-tooltip activator="parent" location="top"> Download {{ value }}.pdf </v-tooltip>
          </v-btn>
        </template>

        <template #no-data> No transactions found </template>
      </v-data-table-server>
    </v-card-text>
  </v-card>
</template>
