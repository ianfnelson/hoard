<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useNewsArticles } from "@/composables/useNewsArticles";
import { useReferenceDataStore } from "@/stores/referenceDataStore";
import { formatDateTime } from "@/utils/formatters";

const router = useRouter();
const { items, totalCount, isLoading, fetchNewsArticles } = useNewsArticles();
const refStore = useReferenceDataStore();

const page = ref(1);
const itemsPerPage = ref(15);
const sortBy = ref<Array<{ key: string; order: 'asc' | 'desc' }>>([
  { key: 'publishedUtc', order: 'desc' }
]);

// Filter state
const searchText = ref("");
const debouncedSearch = ref("");
const selectedInstrumentId = ref<number | null>(null);
const fromDate = ref<string>("");
const toDate = ref<string>("");

// Debounce search
let debounceTimer: ReturnType<typeof setTimeout> | undefined;

watch(searchText, (val) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    debouncedSearch.value = val;
  }, 400);
});

// Computed properties
const instrumentItems = computed(() => {
  return refStore.instruments.map(i => ({
    title: i.context ? `${i.name} (${i.context})` : i.name,
    value: i.id
  }));
});

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
    sortDirection: sortBy.value[0]?.order,
    instrumentId: selectedInstrumentId.value ?? undefined,
    fromDate: fromDate.value || undefined,
    toDate: toDate.value || undefined,
    search: debouncedSearch.value || undefined,
  };
  await fetchNewsArticles(params);
}

function viewArticle(item: any) {
  router.push({ name: 'news-article-detail', params: { id: item.id } });
}

// Reset page when filters change
watch(
  [selectedInstrumentId, fromDate, toDate, debouncedSearch],
  () => {
    page.value = 1;
  }
);

// Watch for changes and reload
watch(
  [page, itemsPerPage, sortBy, selectedInstrumentId, fromDate, toDate, debouncedSearch],
  () => {
    loadItems();
  },
  { immediate: true }
);

onMounted(() => {
  refStore.loadInstruments();
});
</script>

<template>
  <v-container fluid>
    <v-row dense class="mb-2">
      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="searchText"
          label="Search"
          prepend-inner-icon="mdi-magnify"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-select
          v-model="selectedInstrumentId"
          :items="instrumentItems"
          label="Instrument"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="fromDate"
          label="From Date"
          type="date"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-text-field
          v-model="toDate"
          label="To Date"
          type="date"
          clearable
          hide-details
          density="compact"
          variant="outlined"
        />
      </v-col>
    </v-row>
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
