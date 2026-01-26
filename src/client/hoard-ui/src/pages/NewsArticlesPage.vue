<script setup lang="ts">
import { ref, watch } from "vue";
import { useRouter } from "vue-router";
import { useNewsArticles } from "@/composables/useNewsArticles";
import { formatDateTime } from "@/utils/formatters";

const router = useRouter();
const { items, totalCount, isLoading, fetchNewsArticles } = useNewsArticles();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'publishedUtc', order: 'desc' }
]);

const headers = [
  { title: "Published", key: "publishedUtc", sortable: true, width: '160px' },
  { title: "Ticker", key: "instrumentTicker", sortable: true },
  { title: "Instrument Name", key: "instrumentName", sortable: true },
  { title: "Headline", key: "headline", sortable: true }
] as const;

async function loadItems() {
  const params = {
    pageNumber: page.value,
    pageSize: itemsPerPage.value === -1 ? undefined : itemsPerPage.value,
    sortBy: sortBy.value[0]?.key,
    sortDirection: sortBy.value[0]?.order
  };
  await fetchNewsArticles(params);
}

function viewArticle(item: any) {
  router.push({ name: 'news-article-detail', params: { id: item.id } });
}

// Watch for changes and reload
watch([page, itemsPerPage, sortBy], () => {
  loadItems();
}, { immediate: true });
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-data-table-server
          :headers="headers"
          :items="items"
          :items-length="totalCount"
          :loading="isLoading"
          v-model:page="page"
          v-model:items-per-page="itemsPerPage"
          v-model:sort-by="sortBy"
          :items-per-page-options="[
            { value: 10, title: '10' },
            { value: 15, title: '15' },
            { value: 25, title: '25' },
            { value: 50, title: '50' },
            { value: 100, title: '100' },
            { value: -1, title: 'All' }
          ]"
          density="compact"
        >
          <template #item.headline="{ item }">
            <a
              href="#"
              class="text-decoration-none"
              @click.prevent="viewArticle(item)"
            >
              {{ item.headline }}
            </a>
          </template>

          <template #item.publishedUtc="{ value }">
            <span style="white-space: nowrap;">{{ formatDateTime(value) }}</span>
          </template>

          <template #no-data>
            No news articles found
          </template>
        </v-data-table-server>
      </v-col>
    </v-row>
  </v-container>
</template>
