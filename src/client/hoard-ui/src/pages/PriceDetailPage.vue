<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePriceDetail } from '@/composables/usePriceDetail'
import { usePageTitle } from '@/composables/usePageTitle'
import PriceViewCard from '@/components/prices/PriceViewCard.vue'
import PriceForm from '@/components/prices/PriceForm.vue'
import DeleteConfirmDialog from '@/components/DeleteConfirmDialog.vue'

const route = useRoute()
const router = useRouter()

const props = defineProps<{ id: string }>()
const priceId = computed(() => (props.id === 'new' ? null : Number(props.id)))

const { price, isLoading, isSaving, isDeleting, error, fetchPrice, create, update, remove } =
  usePriceDetail()

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
  if (isCreateMode.value) return 'New Price'
  if (isEditMode.value) return 'Edit Price'
  return 'Price Detail'
})
usePageTitle(pageTitle)

// Delete dialog
const showDeleteDialog = ref(false)

// Instrument ID for create mode (from query param)
const createInstrumentId = computed(() => {
  const id = route.query.instrumentId
  return id ? Number(id) : null
})

// Actions
async function handleSave(payload: Record<string, unknown>) {
  if (isCreateMode.value) {
    const id = await create(payload)
    if (id) {
      router.push({ name: 'price-detail', params: { id }, query: {} })
    }
  } else if (priceId.value) {
    const success = await update(priceId.value, payload)
    if (success) {
      await fetchPrice(priceId.value)
      router.push({ query: {} })
    }
  }
}

function handleCancel() {
  if (isCreateMode.value && createInstrumentId.value) {
    router.push({ name: 'instrument-detail', params: { id: createInstrumentId.value } })
  } else if (price.value) {
    if (isEditMode.value) {
      router.push({ query: {} })
    } else {
      router.push({ name: 'instrument-detail', params: { id: price.value.instrumentId } })
    }
  } else {
    router.back()
  }
}

async function confirmDelete() {
  if (!priceId.value || !price.value) return
  const success = await remove(priceId.value)
  if (success) {
    router.push({ name: 'instrument-detail', params: { id: price.value.instrumentId } })
  }
}

onMounted(async () => {
  if (!isCreateMode.value && priceId.value) {
    await fetchPrice(priceId.value)
  }
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

    <v-row dense>
      <v-col>
        <PriceViewCard
          v-if="isViewMode && price"
          :price="price"
          :loading="isLoading"
          @edit="router.push({ query: { mode: 'edit' } })"
          @delete="showDeleteDialog = true"
        />

        <PriceForm
          v-if="isFormMode"
          :price="isEditMode ? (price ?? null) : null"
          :instrument-id="createInstrumentId"
          :is-saving="isSaving"
          @save="handleSave"
          @cancel="handleCancel"
        />
      </v-col>
    </v-row>

    <DeleteConfirmDialog
      v-model="showDeleteDialog"
      title="Delete Price?"
      message="Are you sure you want to delete this price entry?"
      :loading="isDeleting"
      @confirm="confirmDelete"
    />
  </v-container>
</template>
