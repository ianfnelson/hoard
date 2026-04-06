<script setup lang="ts">
import { ref, computed, watch, defineAsyncComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useInstrumentStore } from '@/stores/instrumentStore'
import { useInstrumentDetail } from '@/composables/useInstrumentDetail'
import { usePageTitle } from '@/composables/usePageTitle'
import type { InstrumentWriteDto } from '@/api/dtos/Instruments/InstrumentWriteDto'
import InstrumentViewCard from '@/components/instruments/InstrumentViewCard.vue'
import InstrumentForm from '@/components/instruments/InstrumentForm.vue'
import InstrumentTransactionsCard from '@/components/instruments/InstrumentTransactionsCard.vue'
import InstrumentPricesCard from '@/components/instruments/InstrumentPricesCard.vue'
import InstrumentNewsCard from '@/components/instruments/InstrumentNewsCard.vue'
import DeleteConfirmDialog from '@/components/DeleteConfirmDialog.vue'

const PriceChart = defineAsyncComponent(() => import('@/components/charts/PriceChart.vue'))
const InstrumentValuationChart = defineAsyncComponent(
  () => import('@/components/charts/InstrumentValuationChart.vue')
)

const route = useRoute()
const router = useRouter()

const props = defineProps<{ id: string }>()
const instrumentId = computed(() => (props.id === 'new' ? null : Number(props.id)))

const instrumentStore = useInstrumentStore()
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

const isLoading = computed(() => instrumentStore.isLoading)

// Delete dialog
const showDeleteDialog = ref(false)

// Actions
async function handleSave(payload: InstrumentWriteDto) {
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

// Load instrument when ID changes
watch(
  instrumentId,
  async (newId) => {
    if (newId) {
      await instrumentStore.load(newId)
    }
  },
  { immediate: true }
)
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

    <v-row v-if="isViewMode && instrumentStore.instrument && instrumentId" align="stretch" dense>
      <v-col cols="12" lg="6">
        <InstrumentViewCard
          :instrument="instrumentStore.instrument"
          @edit="router.push({ query: { mode: 'edit' } })"
          @delete="showDeleteDialog = true"
        />
      </v-col>
      <v-col cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>Price History</v-card-title>
          <v-card-text>
            <PriceChart :instrument-id="instrumentId" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="isViewMode && instrumentId" align="stretch" dense>
      <v-col cols="12" lg="6">
        <InstrumentTransactionsCard :instrument-id="instrumentId" />
      </v-col>
      <v-col cols="12" lg="6">
        <v-card class="h-100">
          <v-card-title>Valuation History</v-card-title>
          <v-card-text>
            <InstrumentValuationChart :instrument-id="instrumentId" />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-if="isViewMode && instrumentId" align="stretch" dense>
      <v-col cols="12" lg="6">
        <InstrumentPricesCard :instrument-id="instrumentId" />
      </v-col>
      <v-col cols="12" lg="6">
        <InstrumentNewsCard :instrument-id="instrumentId" />
      </v-col>
    </v-row>

    <v-row v-if="isFormMode" dense>
      <v-col cols="12" lg="8">
        <InstrumentForm
          :instrument="isEditMode ? (instrumentStore.instrument ?? null) : null"
          :is-saving="isSaving"
          @save="handleSave"
          @cancel="handleCancel"
        />
      </v-col>
    </v-row>

    <DeleteConfirmDialog
      v-model="showDeleteDialog"
      title="Delete Instrument?"
      message="Are you sure you want to delete this instrument? This action cannot be undone."
      :loading="isDeleting"
      @confirm="confirmDelete"
    />
  </v-container>
</template>
