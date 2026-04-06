<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useNewsArticles } from '@/composables/useNewsArticles'
import { formatDateTime } from '@/utils/formatters'
import { TABLE_ITEMS_PER_PAGE_OPTIONS } from '@/utils/tableDefaults'
import type { NewsArticleSummaryDto } from '@/api/dtos/News/NewsArticleSummaryDto'

const props = defineProps<{
  instrumentId: number
}>()

const router = useRouter()

const { items, totalCount, isLoading, fetchNewsArticles } = useNewsArticles()

const page = ref(1)
const itemsPerPage = ref(10)
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'publishedUtc', order: 'desc' },
])

const headers = [
  { title: 'Published', key: 'publishedUtc', sortable: true },
  { title: 'Headline', key: 'headline', sortable: true },
] as const

function viewArticle(item: NewsArticleSummaryDto) {
  router.push({ name: 'news-article-detail', params: { id: item.id } })
}

watch(
  [page, itemsPerPage, sortBy, () => props.instrumentId],
  () => {
    fetchNewsArticles({
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
    <v-card-title>News</v-card-title>
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
</template>
