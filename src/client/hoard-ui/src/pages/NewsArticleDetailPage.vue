<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useNewsArticleDetail } from '@/composables/useNewsArticleDetail'
import { formatDateTime } from '@/utils/formatters'

const route = useRoute()
const { article, isLoading, error, fetchNewsArticle } = useNewsArticleDetail()

const id = Number(route.params.id)

onMounted(() => {
  fetchNewsArticle(id)
})
</script>

<template>
  <v-container fluid>
    <v-row dense>
      <v-col>
        <v-card v-if="isLoading">
          <v-card-text>
            <v-progress-circular indeterminate></v-progress-circular>
            Loading article...
          </v-card-text>
        </v-card>

        <v-alert v-else-if="error" type="error">
          {{ error }}
        </v-alert>

        <v-card v-else-if="article">
          <v-card-title>
            {{ article.headline }}
          </v-card-title>
          <v-card-subtitle>
            <div class="d-flex flex-column">
              <div>{{ article.instrumentName }}</div>
              <div>Published {{ formatDateTime(article.publishedUtc) }}</div>
              <div>Retrieved {{ formatDateTime(article.retrievedUtc) }}</div>
              <div v-if="article.url">
                <a :href="article.url" target="_blank" rel="noopener noreferrer">
                  {{ article.url }}
                </a>
              </div>
            </div>
          </v-card-subtitle>
          <v-divider></v-divider>
          <v-card-text>
            <div v-html="article.bodyHtml"></div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
