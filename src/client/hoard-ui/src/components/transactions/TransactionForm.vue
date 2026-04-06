<script setup lang="ts">
import { reactive, computed, watch, onMounted } from 'vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'
import type { TransactionDetailDto } from '@/api/dtos/Transactions/TransactionDetailDto'
import {
  TransactionTypeIds,
  INSTRUMENT_REQUIRED_TYPES,
  INSTRUMENT_OPTIONAL_TYPES,
  TRADING_FIELD_TYPES,
  UNITS_REQUIRED_TYPES,
} from '@/constants/transactionTypes'
import InstrumentSelect from '@/components/InstrumentSelect.vue'

const props = defineProps<{
  transaction: TransactionDetailDto | null
  isSaving: boolean
}>()

const emit = defineEmits<{
  save: [payload: Record<string, unknown>]
  cancel: []
}>()

const refStore = useReferenceDataStore()
const navStore = useNavigationStore()

const isEditMode = computed(() => props.transaction !== null)

// Form state - always stores positive values for editing
const formData = reactive({
  accountId: null as number | null,
  instrumentId: null as number | null,
  transactionTypeId: null as number | null,
  date: '',
  notes: '',
  value: null as number | null,
  units: null as number | null,
  price: null as number | null,
  contractNoteReference: '',
  dealingCharge: null as number | null,
  stampDuty: null as number | null,
  ptmLevy: null as number | null,
  fxCharge: null as number | null,
  contractNoteFile: null as File | null,
})

// When loading transaction for edit, convert to positive values (except for corporate actions)
watch(
  () => props.transaction,
  (txn) => {
    if (txn) {
      const isCorporateActionTxn = txn.transactionTypeId === TransactionTypeIds.CorporateAction

      formData.accountId = txn.accountId
      formData.instrumentId = txn.instrumentId ?? null
      formData.transactionTypeId = txn.transactionTypeId
      formData.date = txn.date
      formData.notes = txn.notes ?? ''
      formData.value = isCorporateActionTxn ? txn.value : Math.abs(txn.value)
      formData.units = txn.units ? (isCorporateActionTxn ? txn.units : Math.abs(txn.units)) : null
      formData.price = txn.price ?? null
      formData.contractNoteReference = txn.contractNoteReference ?? ''
      formData.dealingCharge = txn.dealingCharge ?? null
      formData.stampDuty = txn.stampDuty ?? null
      formData.ptmLevy = txn.ptmLevy ?? null
      formData.fxCharge = txn.fxCharge ?? null
    }
  },
  { immediate: true }
)

// In create mode, set today's date
const today = new Date().toISOString().split('T')[0]!
if (!props.transaction) {
  formData.date = today
}

// Conditional field visibility
const showInstrumentField = computed(() => {
  if (!formData.transactionTypeId) return false
  return (
    INSTRUMENT_REQUIRED_TYPES.includes(formData.transactionTypeId) ||
    INSTRUMENT_OPTIONAL_TYPES.includes(formData.transactionTypeId)
  )
})

const instrumentRequired = computed(() => {
  if (!formData.transactionTypeId) return false
  return INSTRUMENT_REQUIRED_TYPES.includes(formData.transactionTypeId)
})

const showTradingFields = computed(() => {
  if (!formData.transactionTypeId) return false
  return TRADING_FIELD_TYPES.includes(formData.transactionTypeId)
})

const isCorporateAction = computed(() => {
  return formData.transactionTypeId === TransactionTypeIds.CorporateAction
})

const unitsRequired = computed(() => {
  if (!formData.transactionTypeId) return false
  return UNITS_REQUIRED_TYPES.includes(formData.transactionTypeId)
})

const showContractNoteUpload = computed(() => {
  if (!formData.transactionTypeId) return false
  return TRADING_FIELD_TYPES.includes(formData.transactionTypeId)
})

// Validation
const requiredRule = (v: unknown) => !!v || 'Required'
const requiredNumberRule = (v: unknown) => (v !== null && v !== '') || 'Required'
const nonNegativeRule = (v: unknown) => !v || (v as number) >= 0 || 'Must be non-negative'
const notFutureRule = (v: string) => v <= today || 'Cannot be in the future'
const maxLength20Rule = (v: string) => !v || v.length <= 20 || 'Cannot exceed 20 characters'

const isFormValid = computed(() => {
  return (
    formData.accountId !== null &&
    formData.transactionTypeId !== null &&
    formData.date !== '' &&
    formData.date <= today &&
    formData.value !== null &&
    (!instrumentRequired.value || formData.instrumentId !== null) &&
    (!unitsRequired.value || formData.units !== null)
  )
})

// Auto-populate contract note reference from filename
function extractReferenceFromFilename(filename: string): string {
  const nameWithoutExtension = filename.substring(0, filename.lastIndexOf('.'))
  const underscoreIndex = nameWithoutExtension.indexOf('_')
  return underscoreIndex > 0
    ? nameWithoutExtension.substring(0, underscoreIndex)
    : nameWithoutExtension
}

watch(
  () => formData.contractNoteFile,
  (file) => {
    if (file) {
      const extractedRef = extractReferenceFromFilename(file.name)
      if (extractedRef && extractedRef.length <= 20) {
        formData.contractNoteReference = extractedRef
      }
    }
  }
)

function handleSave() {
  if (!isFormValid.value) return

  emit('save', {
    ...formData,
    notes: formData.notes.trim() || null,
    contractNoteReference: formData.contractNoteReference.trim() || null,
  })
}

onMounted(async () => {
  await refStore.loadTransactionTypes()
  await refStore.loadInstruments()
  await navStore.loadAccounts()
})
</script>

<template>
  <v-card>
    <v-card-title>{{ isEditMode ? 'Edit Transaction' : 'New Transaction' }}</v-card-title>
    <v-card-text>
      <v-form>
        <!-- Section 1: Core (Date, Account, Type) -->
        <v-row dense>
          <v-col cols="12" sm="4">
            <v-text-field
              v-model="formData.date"
              label="Date"
              type="date"
              :max="today"
              :rules="[requiredRule, notFutureRule]"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col cols="12" sm="4">
            <v-select
              v-model="formData.accountId"
              :items="navStore.accounts"
              item-title="name"
              item-value="id"
              label="Account"
              :rules="[requiredRule]"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col cols="12" sm="4">
            <v-select
              v-model="formData.transactionTypeId"
              :items="refStore.transactionTypes"
              item-title="name"
              item-value="id"
              label="Transaction Type"
              :rules="[requiredRule]"
              required
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Section 2: Value & Trade (Value, Units, Price) -->
        <v-row dense>
          <v-col cols="12" sm="4">
            <v-text-field
              v-model.number="formData.value"
              label="Value"
              type="number"
              step="0.01"
              :rules="[
                requiredNumberRule,
                (v) => isCorporateAction || v >= 0 || 'Must be non-negative',
              ]"
              required
              variant="outlined"
              density="compact"
              prefix="£"
            />
          </v-col>
          <v-col v-if="showTradingFields" cols="12" sm="4">
            <v-text-field
              v-model.number="formData.units"
              label="Units"
              type="number"
              step="0.000001"
              :rules="[
                ...(unitsRequired ? [requiredNumberRule] : []),
                (v) => !v || isCorporateAction || v >= 0 || 'Must be non-negative',
              ]"
              :required="unitsRequired"
              variant="outlined"
              density="compact"
            />
          </v-col>
          <v-col v-if="showTradingFields" cols="12" sm="4">
            <v-text-field
              v-model.number="formData.price"
              label="Price"
              type="number"
              step="0.0001"
              min="0"
              :rules="[nonNegativeRule]"
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Section 3: Costs (conditional) -->
        <v-row v-if="showTradingFields" dense>
          <v-col cols="12" sm="6" md="3">
            <v-text-field
              v-model.number="formData.dealingCharge"
              label="Dealing Charge"
              type="number"
              step="0.01"
              min="0"
              :rules="[nonNegativeRule]"
              variant="outlined"
              density="compact"
              prefix="£"
            />
          </v-col>
          <v-col cols="12" sm="6" md="3">
            <v-text-field
              v-model.number="formData.stampDuty"
              label="Stamp Duty"
              type="number"
              step="0.01"
              min="0"
              :rules="[nonNegativeRule]"
              variant="outlined"
              density="compact"
              prefix="£"
            />
          </v-col>
          <v-col cols="12" sm="6" md="3">
            <v-text-field
              v-model.number="formData.ptmLevy"
              label="PTM Levy"
              type="number"
              step="0.01"
              min="0"
              :rules="[nonNegativeRule]"
              variant="outlined"
              density="compact"
              prefix="£"
            />
          </v-col>
          <v-col cols="12" sm="6" md="3">
            <v-text-field
              v-model.number="formData.fxCharge"
              label="FX Charge"
              type="number"
              step="0.01"
              min="0"
              :rules="[nonNegativeRule]"
              variant="outlined"
              density="compact"
              prefix="£"
            />
          </v-col>
        </v-row>

        <!-- Section 4: Instrument (conditional) -->
        <v-row v-if="showInstrumentField" dense>
          <v-col>
            <InstrumentSelect
              v-model="formData.instrumentId"
              label="Instrument"
              :rules="instrumentRequired ? [requiredRule] : []"
              :required="instrumentRequired"
              clearable
              variant="outlined"
              density="compact"
            />
          </v-col>
        </v-row>

        <!-- Section 5: Notes -->
        <v-row dense>
          <v-col>
            <v-textarea
              v-model="formData.notes"
              label="Notes"
              variant="outlined"
              density="compact"
              rows="3"
            />
          </v-col>
        </v-row>

        <!-- Section 6: Contract Note (conditional) -->
        <v-row v-if="showContractNoteUpload" dense>
          <v-col cols="12" md="6">
            <v-file-input
              v-model="formData.contractNoteFile"
              label="Contract Note PDF"
              accept="application/pdf"
              prepend-icon="mdi-file-document"
              variant="outlined"
              density="compact"
              :rules="[
                (v) => !v || v.size < 10485760 || 'File size must be less than 10MB',
                (v) => !v || v.type === 'application/pdf' || 'Only PDF files allowed',
              ]"
              hint="Select PDF file (e.g., B293875534_BOUGHT_Parkmead_Group.pdf)"
              persistent-hint
              clearable
            />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="formData.contractNoteReference"
              label="Contract Note Reference"
              variant="outlined"
              density="compact"
              :rules="[maxLength20Rule]"
              hint="Auto-filled from filename, or enter manually"
              persistent-hint
            />
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
