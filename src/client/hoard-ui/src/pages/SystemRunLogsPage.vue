<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { usePageTitle } from '@/composables/usePageTitle'
import { getRunLogs, getRunTypes, type GetRunLogsParams } from '@/api/runLogsApi'
import type { RunLogDto } from '@/api/dtos/System/RunLogDto'
import type { RunTypeDto } from '@/api/dtos/System/RunTypeDto'
import { formatDateTime } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'

const pageTitle = ref('Run Logs')
usePageTitle(pageTitle)

const router = useRouter()
const route = useRoute()

const items = ref<RunLogDto[]>([])
const totalCount = ref(0)
const isLoading = ref(false)
const runTypes = ref<RunTypeDto[]>([])

const page = ref(1)
const itemsPerPage = ref(25)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'startedAtUtc', order: 'desc' },
])

const initialRunTypeId = route.query.runTypeId ? Number(route.query.runTypeId) : null
const selectedRunTypeId = ref<number | null>(initialRunTypeId)

const headers = [
  { title: 'Operation', key: 'runTypeName', sortable: true },
  { title: 'Started', key: 'startedAtUtc', sortable: true },
  { title: 'Completed', key: 'completedAtUtc', sortable: true },
  { title: 'Duration', key: 'duration', sortable: true, align: 'end' },
  { title: 'Status', key: 'status', sortable: false },
] as const

function durationLabel(item: RunLogDto): string {
  const start = new Date(item.startedAtUtc).getTime()
  const end = item.completedAtUtc ? new Date(item.completedAtUtc).getTime() : Date.now()
  const seconds = Math.max(0, Math.round((end - start) / 1000))
  if (seconds < 60) return `${seconds}s`
  const minutes = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${minutes}m ${secs}s`
}

function statusLabel(item: RunLogDto): string {
  return item.completedAtUtc ? 'Completed' : 'Running'
}

function statusColor(item: RunLogDto): string {
  return item.completedAtUtc ? 'success' : 'warning'
}

async function loadItems() {
  isLoading.value = true
  try {
    const params: GetRunLogsParams = {
      pageNumber: page.value,
      pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
      sortBy: sortBy.value[0]?.key,
      sortDirection: sortBy.value[0]?.order,
      runTypeId: selectedRunTypeId.value ?? undefined,
    }
    const result = await getRunLogs(params)
    items.value = result.items
    totalCount.value = result.totalCount
  } finally {
    isLoading.value = false
  }
}

watch([selectedRunTypeId], () => {
  page.value = 1
})

watch([selectedRunTypeId], () => {
  const query: Record<string, string> = {}
  if (selectedRunTypeId.value) query.runTypeId = String(selectedRunTypeId.value)
  router.replace({ query })
})

watch(
  [page, itemsPerPage, sortBy, selectedRunTypeId],
  () => {
    loadItems()
  },
  { immediate: true }
)

onMounted(async () => {
  runTypes.value = await getRunTypes()
})

// Auto-refresh every 5 seconds
const refreshInterval = setInterval(() => {
  loadItems()
}, 5000)

onUnmounted(() => {
  clearInterval(refreshInterval)
})
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="4">
        <v-select
          v-model="selectedRunTypeId"
          :items="runTypes"
          item-title="name"
          item-value="id"
          label="Operation"
          clearable
          density="compact"
          hide-details
          variant="outlined"
        />
      </v-col>
    </v-row>
    <v-row dense>
      <v-col>
        <v-card>
          <v-data-table-server
            v-model:page="page"
            v-model:items-per-page="itemsPerPage"
            v-model:sort-by="sortBy"
            :headers="headers"
            :items="items"
            :items-length="totalCount"
            :loading="isLoading"
            :items-per-page-options="TABLE_ITEMS_PER_PAGE_OPTIONS"
            item-value="runLogId"
            density="compact"
          >
            <template #item.startedAtUtc="{ item }">{{
              formatDateTime(item.startedAtUtc)
            }}</template>
            <template #item.completedAtUtc="{ item }">{{
              item.completedAtUtc ? formatDateTime(item.completedAtUtc) : '—'
            }}</template>
            <template #item.duration="{ item }">{{ durationLabel(item) }}</template>
            <template #item.status="{ item }">
              <v-chip :color="statusColor(item)" size="small" label>{{ statusLabel(item) }}</v-chip>
            </template>
          </v-data-table-server>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
