<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTransactionDetail } from '@/composables/useTransactionDetail'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { useNavigationStore } from '@/stores/navigationStore'
import { usePageTitle } from '@/composables/usePageTitle'
import { formatCurrency, formatDate, formatUnits, getTrendClass } from '@/utils/formatters'
import {
  TransactionTypeIds,
  INSTRUMENT_REQUIRED_TYPES,
  INSTRUMENT_OPTIONAL_TYPES,
  TRADING_FIELD_TYPES,
} from '@/constants/transactionTypes'
import { uploadContractNote } from '@/api/transactionsApi'

const props = defineProps<{ id: string }>()
const route = useRoute()
const router = useRouter()

const pageTitle = ref('Transaction')
usePageTitle(pageTitle)

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

// Data
const {
  transaction,
  isLoading,
  isSaving,
  isDeleting,
  error,
  fetchTransaction,
  create,
  update,
  remove,
} = useTransactionDetail()
const refStore = useReferenceDataStore()
const navStore = useNavigationStore()

// Form state - always stores positive values for editing
const formData = reactive({
  accountId: null as number | null,
  instrumentId: null as number | null,
  transactionTypeId: null as number | null,
  date: '',
  notes: '',
  value: null as number | null,
  units: null as number | null, // Always positive in form
  price: null as number | null,
  contractNoteReference: '',
  dealingCharge: null as number | null,
  stampDuty: null as number | null,
  ptmLevy: null as number | null,
  fxCharge: null as number | null,
  contractNoteFile: null as File | null,
})

// When loading transaction for edit, convert to positive values (except for corporate actions)
watch([() => transaction.value, isEditMode], ([txn, editMode]) => {
  if (txn && editMode) {
    const isCorporateActionTxn = txn.transactionTypeId === TransactionTypeIds.CorporateAction

    formData.accountId = txn.accountId
    formData.instrumentId = txn.instrumentId ?? null
    formData.transactionTypeId = txn.transactionTypeId
    formData.date = txn.date
    formData.notes = txn.notes ?? ''
    // Convert to positive for editing, except for corporate actions
    formData.value = isCorporateActionTxn ? txn.value : Math.abs(txn.value)
    formData.units = txn.units ? (isCorporateActionTxn ? txn.units : Math.abs(txn.units)) : null
    formData.price = txn.price ?? null
    formData.contractNoteReference = txn.contractNoteReference ?? ''
    formData.dealingCharge = txn.dealingCharge ?? null
    formData.stampDuty = txn.stampDuty ?? null
    formData.ptmLevy = txn.ptmLevy ?? null
    formData.fxCharge = txn.fxCharge ?? null
  }
})

// Instruments with context for dropdown
const instrumentItems = computed(() => {
  return refStore.instruments.map((i) => ({
    title: i.context ? `${i.name} (${i.context})` : i.name,
    value: i.id,
  }))
})

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

const showContractNoteUpload = computed(() => {
  if (!formData.transactionTypeId) return false
  return [
    TransactionTypeIds.Buy,
    TransactionTypeIds.Sell,
    TransactionTypeIds.CorporateAction,
  ].includes(formData.transactionTypeId)
})

// Validation
const today = new Date().toISOString().split('T')[0]!
const isFormValid = computed(() => {
  return (
    formData.accountId !== null &&
    formData.transactionTypeId !== null &&
    formData.date !== '' &&
    formData.date <= today &&
    formData.value !== null &&
    (!instrumentRequired.value || formData.instrumentId !== null)
  )
})

// Delete dialog
const showDeleteDialog = ref(false)

// Contract note upload state
const isUploadingFile = ref(false)
const uploadError = ref<string | null>(null)

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

// Actions
async function handleSave() {
  if (!isFormValid.value) return

  // Convert empty strings to null for optional text fields
  const payload = {
    ...formData,
    notes: formData.notes.trim() || null,
    contractNoteReference: formData.contractNoteReference.trim() || null,
  }

  try {
    let transactionId: number

    if (isCreateMode.value) {
      const id = await create(payload)
      if (!id) return
      transactionId = id
    } else {
      transactionId = Number(props.id)
      const success = await update(transactionId, payload)
      if (!success) return
    }

    // Upload file if present
    if (formData.contractNoteFile) {
      isUploadingFile.value = true
      uploadError.value = null
      try {
        await uploadContractNote(transactionId, formData.contractNoteFile)
      } catch (err) {
        uploadError.value = err instanceof Error ? err.message : 'Failed to upload contract note'
        console.error('Error uploading contract note:', err)
      } finally {
        isUploadingFile.value = false
      }
    }

    if (isCreateMode.value) {
      router.push({ name: 'transactions' })
    } else {
      router.push({ query: {} })
    }
  } catch (err) {
    console.error('Error saving transaction:', err)
  }
}

function handleCancel() {
  if (isCreateMode.value) {
    router.push({ name: 'transactions' })
  } else {
    router.push({ query: {} })
  }
}

async function confirmDelete() {
  const transactionId = Number(props.id)
  const success = await remove(transactionId)
  if (success) {
    router.push({ name: 'transactions' })
  }
}

onMounted(async () => {
  await refStore.loadTransactionTypes()
  await refStore.loadInstruments()
  await navStore.loadAccounts()

  if (isCreateMode.value) {
    formData.date = today
  } else {
    const transactionId = Number(props.id)
    await fetchTransaction(transactionId)
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

    <v-row v-if="isLoading" dense>
      <v-col>
        <v-progress-linear indeterminate />
      </v-col>
    </v-row>

    <v-row dense>
      <v-col>
        <!-- View Mode -->
        <v-card v-if="isViewMode">
          <v-card-title>Transaction Details</v-card-title>
          <v-card-text v-if="transaction">
            <!-- Section 1: Core (Date, Account, Type) -->
            <v-row dense>
              <v-col cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Date</div>
                <div class="text-body-1">{{ formatDate(transaction.date) }}</div>
              </v-col>
              <v-col cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Account</div>
                <div class="text-body-1">{{ transaction.accountName }}</div>
              </v-col>
              <v-col cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Transaction Type</div>
                <div class="text-body-1">{{ transaction.transactionTypeName }}</div>
              </v-col>
            </v-row>

            <v-divider class="my-3" />

            <!-- Section 2: Value & Trade (Value, Units, Price) -->
            <v-row dense>
              <v-col cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Value</div>
                <div class="text-body-1" :class="getTrendClass(transaction.value)">
                  {{ formatCurrency(transaction.value) }}
                </div>
              </v-col>
              <v-col v-if="transaction.units !== null" cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Units</div>
                <div class="text-body-1" :class="getTrendClass(transaction.units)">
                  {{ formatUnits(transaction.units) }}
                </div>
              </v-col>
              <v-col v-if="transaction.price !== null" cols="12" sm="4">
                <div class="text-caption text-medium-emphasis">Price</div>
                <div class="text-body-1">{{ transaction.price }}</div>
              </v-col>
            </v-row>

            <!-- Section 3: Costs (conditional) -->
            <template
              v-if="
                transaction.dealingCharge ||
                transaction.stampDuty ||
                transaction.ptmLevy ||
                transaction.fxCharge
              "
            >
              <v-divider class="my-3" />
              <v-row dense>
                <v-col v-if="transaction.dealingCharge" cols="12" sm="6" md="3">
                  <div class="text-caption text-medium-emphasis">Dealing Charge</div>
                  <div class="text-body-1">{{ formatCurrency(transaction.dealingCharge) }}</div>
                </v-col>
                <v-col v-if="transaction.stampDuty" cols="12" sm="6" md="3">
                  <div class="text-caption text-medium-emphasis">Stamp Duty</div>
                  <div class="text-body-1">{{ formatCurrency(transaction.stampDuty) }}</div>
                </v-col>
                <v-col v-if="transaction.ptmLevy" cols="12" sm="6" md="3">
                  <div class="text-caption text-medium-emphasis">PTM Levy</div>
                  <div class="text-body-1">{{ formatCurrency(transaction.ptmLevy) }}</div>
                </v-col>
                <v-col v-if="transaction.fxCharge" cols="12" sm="6" md="3">
                  <div class="text-caption text-medium-emphasis">FX Charge</div>
                  <div class="text-body-1">{{ formatCurrency(transaction.fxCharge) }}</div>
                </v-col>
              </v-row>
            </template>

            <!-- Section 4: Instrument (conditional) -->
            <template v-if="transaction.instrumentName">
              <v-divider class="my-3" />
              <v-row dense>
                <v-col>
                  <div class="text-caption text-medium-emphasis">Instrument</div>
                  <div class="text-body-1">
                    {{ transaction.instrumentName }}
                    <span v-if="transaction.instrumentTicker" class="text-medium-emphasis">
                      ({{ transaction.instrumentTicker }})
                    </span>
                  </div>
                </v-col>
              </v-row>
            </template>

            <!-- Section 5: Notes (conditional) -->
            <template v-if="transaction.notes">
              <v-divider class="my-3" />
              <v-row dense>
                <v-col>
                  <div class="text-caption text-medium-emphasis">Notes</div>
                  <div class="text-body-1">{{ transaction.notes }}</div>
                </v-col>
              </v-row>
            </template>
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

        <!-- Form Mode -->
        <v-card v-if="isFormMode">
          <v-card-title>{{ isCreateMode ? 'New Transaction' : 'Edit Transaction' }}</v-card-title>
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
                    :rules="[
                      (v) => !!v || 'Date is required',
                      (v) => v <= today || 'Future dates not allowed',
                    ]"
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
                    :rules="[(v) => !!v || 'Account is required']"
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
                    :rules="[(v) => !!v || 'Transaction Type is required']"
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
                      (v) => (v !== null && v !== '') || 'Value is required',
                      (v) => isCorporateAction || v >= 0 || 'Value must be positive',
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
                    :rules="[(v) => !v || isCorporateAction || v >= 0 || 'Units must be positive']"
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
                    :rules="[(v) => !v || v >= 0 || 'Price must be positive']"
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
                    :rules="[(v) => !v || v >= 0 || 'Dealing Charge must be positive']"
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
                    :rules="[(v) => !v || v >= 0 || 'Stamp Duty must be positive']"
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
                    :rules="[(v) => !v || v >= 0 || 'PTM Levy must be positive']"
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
                    :rules="[(v) => !v || v >= 0 || 'FX Charge must be positive']"
                    variant="outlined"
                    density="compact"
                    prefix="£"
                  />
                </v-col>
              </v-row>

              <!-- Section 4: Instrument (conditional) -->
              <v-row v-if="showInstrumentField" dense>
                <v-col>
                  <v-select
                    v-model="formData.instrumentId"
                    :items="instrumentItems"
                    label="Instrument"
                    :rules="instrumentRequired ? [(v) => !!v || 'Instrument is required'] : []"
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
                    :rules="[(v) => !v || v.length <= 20 || 'Maximum 20 characters']"
                    hint="Auto-filled from filename, or enter manually"
                    persistent-hint
                  />
                </v-col>
              </v-row>

              <v-row v-if="uploadError" dense>
                <v-col>
                  <v-alert type="warning" dismissible @click:close="uploadError = null">
                    {{ uploadError }}
                  </v-alert>
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

    <!-- Delete Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="500">
      <v-card>
        <v-card-title>Delete Transaction?</v-card-title>
        <v-card-text>
          Are you sure you want to delete this transaction? This action cannot be undone.
        </v-card-text>
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
            @click="confirmDelete"
          >
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>
