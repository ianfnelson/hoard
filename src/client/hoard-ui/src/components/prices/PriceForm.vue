<script setup lang="ts">
import { reactive, computed, watch, onMounted } from 'vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import type { PriceDetailDto } from '@/api/dtos/Prices/PriceDetailDto'
import type { PriceWriteDto } from '@/api/dtos/Prices/PriceWriteDto'

const props = defineProps<{
  price: PriceDetailDto | null
  instrumentId: number | null
  isSaving: boolean
}>()

const emit = defineEmits<{
  save: [payload: PriceWriteDto]
  cancel: []
}>()

const refStore = useReferenceDataStore()

const isEditMode = computed(() => props.price !== null)

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

// Load form data when editing
watch(
  () => props.price,
  (p) => {
    if (p) {
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

// In create mode, set instrumentId from prop
if (!props.price && props.instrumentId) {
  formData.instrumentId = props.instrumentId
}

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

function handleSave() {
  if (!isFormValid.value) return

  emit('save', {
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
  })
}

onMounted(() => {
  refStore.loadInstruments()
})
</script>

<template>
  <v-card>
    <v-card-title>
      {{ isEditMode ? 'Edit Price' : 'New Price' }}
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
      <v-btn variant="flat" prepend-icon="mdi-close" @click="emit('cancel')">Cancel</v-btn>
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
</template>
