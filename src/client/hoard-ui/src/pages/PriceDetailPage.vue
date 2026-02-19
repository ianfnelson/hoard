<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePriceDetail } from '@/composables/usePriceDetail'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatDateTime, formatDate } from '@/utils/formatters'

const route = useRoute()
const router = useRouter()

const props = defineProps<{ id: string }>()
const priceId = computed(() => (props.id === 'new' ? null : Number(props.id)))

// Composables
const refStore = useReferenceDataStore()
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

// Form state
const formData = reactive({
  instrumentId: null as number | null,
  asOfDate: '',
  source: 'Manual',
  isLocked: false,
  open: null as number | null,
  high: null as number | null,
  low: null as number | null,
  close: null as number | null,
  volume: null as number | null,
  adjustedClose: null as number | null,
})

// Get instrument name for display
const instrumentName = computed(() => {
  if (!formData.instrumentId) return ''
  const instrument = refStore.instruments.find((i) => i.id === formData.instrumentId)
  return instrument?.name ?? ''
})

// Load form data when editing or switching to edit mode
watch(
  [() => price.value, isEditMode],
  ([p, editMode]) => {
    if (p && editMode) {
      formData.instrumentId = p.instrumentId
      formData.asOfDate = p.asOfDate
      formData.source = p.source
      formData.isLocked = p.isLocked
      formData.open = p.open
      formData.high = p.high
      formData.low = p.low
      formData.close = p.close
      formData.volume = p.volume
      formData.adjustedClose = p.adjustedClose
    }
  },
  { immediate: true }
)

// Field length constraints
const SOURCE_MAX_LENGTH = 50

// Validation
const today = new Date().toISOString().split('T')[0]!
const requiredRule = (v: unknown) => !!v || 'Required'
const requiredNumberRule = (v: unknown) => (v !== null && v !== '') || 'Required'
const nonNegativeRule = (v: unknown) => !v || (v as number) >= 0 || 'Must be non-negative'
const notFutureRule = (v: string) => v <= today || 'Cannot be in the future'
const sourceMaxLengthRule = (v: string) =>
  !v || v.length <= SOURCE_MAX_LENGTH || `Cannot exceed ${SOURCE_MAX_LENGTH} characters`

const isFormValid = computed(() => {
  return (
    formData.instrumentId !== null &&
    formData.asOfDate !== '' &&
    formData.asOfDate <= today &&
    formData.source.trim() !== '' &&
    formData.source.length <= SOURCE_MAX_LENGTH &&
    formData.close !== null &&
    formData.close >= 0 &&
    formData.adjustedClose !== null &&
    formData.adjustedClose >= 0 &&
    (formData.open === null || formData.open >= 0) &&
    (formData.high === null || formData.high >= 0) &&
    (formData.low === null || formData.low >= 0) &&
    (formData.volume === null || formData.volume >= 0)
  )
})

// Delete dialog
const showDeleteDialog = ref(false)

// Actions
async function handleSave() {
  if (!isFormValid.value) return

  const payload = {
    instrumentId: formData.instrumentId,
    asOfDate: formData.asOfDate,
    source: formData.source.trim(),
    isLocked: formData.isLocked,
    open: formData.open,
    high: formData.high,
    low: formData.low,
    close: formData.close,
    volume: formData.volume,
    adjustedClose: formData.adjustedClose,
  }

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
  if (isCreateMode.value && formData.instrumentId) {
    router.push({ name: 'instrument-detail', params: { id: formData.instrumentId } })
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

async function handleDeleteConfirm() {
  await confirmDelete()
  showDeleteDialog.value = false
}

// Load data on mount
onMounted(async () => {
  refStore.loadInstruments()

  if (isCreateMode.value) {
    // Get instrumentId from query param
    const instrumentId = route.query.instrumentId
    if (instrumentId) {
      formData.instrumentId = Number(instrumentId)
    }
  } else if (priceId.value) {
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
        <!-- View Mode -->
        <v-card v-if="isViewMode && price" :loading="isLoading">
          <v-card-title>Price Detail</v-card-title>

          <v-card-text>
            <v-row>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">As Of Date</div>
                <div class="text-body-1">{{ formatDate(price.asOfDate) }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Source</div>
                <div class="text-body-1">{{ price.source }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Is Locked</div>
                <div class="text-body-1">{{ price.isLocked ? 'Yes' : 'No' }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Retrieved UTC</div>
                <div class="text-body-1">{{ formatDateTime(price.retrievedUtc) }}</div>
              </v-col>
            </v-row>

            <v-divider class="my-3" />

            <v-row>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Open</div>
                <div class="text-body-1">{{ price.open ?? '—' }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">High</div>
                <div class="text-body-1">{{ price.high ?? '—' }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Low</div>
                <div class="text-body-1">{{ price.low ?? '—' }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Close</div>
                <div class="text-body-1">{{ price.close }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Adjusted Close</div>
                <div class="text-body-1">{{ price.adjustedClose }}</div>
              </v-col>
              <v-col cols="12" sm="3">
                <div class="text-caption text-medium-emphasis">Volume</div>
                <div class="text-body-1">{{ price.volume ?? '—' }}</div>
              </v-col>
            </v-row>
            <v-divider class="my-3" />
            <v-row>
              <v-col cols="12" md="6">
                <div class="text-caption text-medium-emphasis">Instrument</div>
                <div class="text-body-1">
                  <router-link
                    :to="{ name: 'instrument-detail', params: { id: price.instrumentId } }"
                    class="text-decoration-none"
                  >
                    {{ price.instrumentName }} ({{ price.instrumentTicker }})
                  </router-link>
                </div>
              </v-col>
            </v-row>
          </v-card-text>

          <v-card-actions>
            <v-btn
              color="primary"
              variant="flat"
              prepend-icon="mdi-pencil"
              @click="router.push({ query: { mode: 'edit' } })"
            >
              Edit
            </v-btn>
            <v-btn
              color="error"
              variant="flat"
              prepend-icon="mdi-delete"
              @click="showDeleteDialog = true"
            >
              Delete
            </v-btn>
          </v-card-actions>
        </v-card>

        <!-- Form Mode (Create/Edit) -->
        <v-card v-if="isFormMode" :loading="isLoading">
          <v-card-title>
            {{ isCreateMode ? 'New Price' : 'Edit Price' }}
          </v-card-title>

          <v-card-text>
            <v-form>
              <v-row dense>
                <v-col cols="12" sm="3">
                  <v-text-field
                    v-model="formData.asOfDate"
                    label="As Of Date"
                    type="date"
                    :max="today"
                    :rules="[requiredRule, notFutureRule]"
                    variant="outlined"
                    density="compact"
                    required
                  />
                </v-col>
                <v-col cols="12" sm="3">
                  <v-text-field
                    v-model="formData.source"
                    label="Source"
                    :rules="[requiredRule, sourceMaxLengthRule]"
                    :maxlength="SOURCE_MAX_LENGTH"
                    variant="outlined"
                    density="compact"
                    required
                  />
                </v-col>
                <v-col cols="12" sm="3">
                  <v-checkbox
                    v-model="formData.isLocked"
                    label="Is Locked"
                    density="compact"
                    hide-details
                  />
                </v-col>
              </v-row>

              <v-row dense>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.open"
                    label="Open"
                    type="number"
                    step="0.0001"
                    min="0"
                    :rules="[nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    clearable
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.high"
                    label="High"
                    type="number"
                    step="0.0001"
                    min="0"
                    :rules="[nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    clearable
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.low"
                    label="Low"
                    type="number"
                    step="0.0001"
                    min="0"
                    :rules="[nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    clearable
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.close"
                    label="Close"
                    type="number"
                    step="0.0001"
                    min="0"
                    :rules="[requiredNumberRule, nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    required
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.adjustedClose"
                    label="Adjusted Close"
                    type="number"
                    step="0.0001"
                    min="0"
                    :rules="[requiredNumberRule, nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    required
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-text-field
                    v-model.number="formData.volume"
                    label="Volume"
                    type="number"
                    step="1"
                    min="0"
                    :rules="[nonNegativeRule]"
                    variant="outlined"
                    density="compact"
                    clearable
                  />
                </v-col>
              </v-row>

              <v-row dense>
                <v-col cols="12" md="6">
                  <div class="text-caption text-medium-emphasis">Instrument</div>
                  <div class="text-body-1 mb-4">{{ instrumentName }}</div>
                </v-col>
              </v-row>
            </v-form>
          </v-card-text>

          <v-card-actions>
            <v-btn variant="flat" prepend-icon="mdi-close" @click="handleCancel">Cancel</v-btn>
            <v-btn
              color="primary"
              variant="flat"
              prepend-icon="mdi-content-save"
              :disabled="!isFormValid"
              :loading="isSaving"
              @click="handleSave"
            >
              Save
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="400">
      <v-card>
        <v-card-title>Delete Price?</v-card-title>
        <v-card-text> Are you sure you want to delete this price entry? </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="flat" prepend-icon="mdi-close" @click="showDeleteDialog = false">
            Cancel
          </v-btn>
          <v-btn
            color="error"
            variant="flat"
            prepend-icon="mdi-delete"
            :loading="isDeleting"
            @click="handleDeleteConfirm"
          >
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>
