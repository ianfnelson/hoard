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
})

// When loading transaction for edit, convert to positive values
watch([() => transaction.value, isEditMode], ([txn, editMode]) => {
  if (txn && editMode) {
    formData.accountId = txn.accountId
    formData.instrumentId = txn.instrumentId ?? null
    formData.transactionTypeId = txn.transactionTypeId
    formData.date = txn.date
    formData.notes = txn.notes ?? ''
    formData.value = Math.abs(txn.value) // Convert to positive for editing
    formData.units = txn.units ? Math.abs(txn.units) : null // Convert to positive
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

// Actions
async function handleSave() {
  if (!isFormValid.value) return

  // Convert empty strings to null for optional text fields
  const payload = {
    ...formData,
    notes: formData.notes.trim() || null,
    contractNoteReference: formData.contractNoteReference.trim() || null,
  }

  if (isCreateMode.value) {
    const id = await create(payload)
    if (id) {
      router.push({ name: 'transactions' })
    }
  } else {
    const transactionId = Number(props.id)
    const success = await update(transactionId, payload)
    if (success) {
      router.push({ query: {} })
    }
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

  if (!isCreateMode.value) {
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
          <v-card-text>
            <v-list v-if="transaction" density="compact" lines="two">
              <v-list-item>
                <template #title>{{ transaction.accountName }}</template>
                <template #subtitle>Account</template>
              </v-list-item>
              <v-list-item>
                <template #title>{{ transaction.transactionTypeName }}</template>
                <template #subtitle>Transaction Type</template>
              </v-list-item>
              <v-list-item v-if="transaction.instrumentName">
                <template #title>{{ transaction.instrumentName }}</template>
                <template #subtitle>Instrument</template>
              </v-list-item>
              <v-list-item v-if="transaction.instrumentTicker">
                <template #title>{{ transaction.instrumentTicker }}</template>
                <template #subtitle>Ticker</template>
              </v-list-item>
              <v-list-item>
                <template #title>{{ formatDate(transaction.date) }}</template>
                <template #subtitle>Date</template>
              </v-list-item>
              <v-list-item>
                <template #title>
                  <span :class="getTrendClass(transaction.value)">{{
                    formatCurrency(transaction.value)
                  }}</span>
                </template>
                <template #subtitle>Value</template>
              </v-list-item>
              <v-list-item v-if="transaction.units !== null && transaction.units !== undefined">
                <template #title>
                  <span :class="getTrendClass(transaction.units)">{{
                    formatUnits(transaction.units)
                  }}</span>
                </template>
                <template #subtitle>Units</template>
              </v-list-item>
              <v-list-item v-if="transaction.price !== null && transaction.price !== undefined">
                <template #title>{{ transaction.price }}</template>
                <template #subtitle>Price</template>
              </v-list-item>
              <v-list-item v-if="transaction.dealingCharge">
                <template #title>{{ formatCurrency(transaction.dealingCharge) }}</template>
                <template #subtitle>Dealing Charge</template>
              </v-list-item>
              <v-list-item v-if="transaction.stampDuty">
                <template #title>{{ formatCurrency(transaction.stampDuty) }}</template>
                <template #subtitle>Stamp Duty</template>
              </v-list-item>
              <v-list-item v-if="transaction.ptmLevy">
                <template #title>{{ formatCurrency(transaction.ptmLevy) }}</template>
                <template #subtitle>PTM Levy</template>
              </v-list-item>
              <v-list-item v-if="transaction.fxCharge">
                <template #title>{{ formatCurrency(transaction.fxCharge) }}</template>
                <template #subtitle>FX Charge</template>
              </v-list-item>
              <v-list-item v-if="transaction.notes">
                <template #title>{{ transaction.notes }}</template>
                <template #subtitle>Notes</template>
              </v-list-item>
            </v-list>
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
              <!-- Account dropdown (required) -->
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

              <!-- Transaction Type dropdown (required) -->
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

              <!-- Instrument dropdown (conditional) -->
              <v-select
                v-if="showInstrumentField"
                v-model="formData.instrumentId"
                :items="instrumentItems"
                label="Instrument"
                :rules="instrumentRequired ? [(v) => !!v || 'Instrument is required'] : []"
                clearable
                variant="outlined"
                density="compact"
              />

              <!-- Date picker (required, no future dates) -->
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

              <!-- Trading fields (conditional on transaction type) -->
              <v-row v-if="showTradingFields" dense class="my-0">
                <v-col cols="12" md="6">
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
                <v-col cols="12" md="6">
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

              <!-- Fee fields (conditional) -->
              <v-row v-if="showTradingFields" dense class="my-0">
                <v-col cols="12" md="6">
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
                <v-col cols="12" md="6">
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
              </v-row>
              <v-row v-if="showTradingFields" dense class="my-0">
                <v-col cols="12" md="6">
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
                <v-col cols="12" md="6">
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

              <!-- Value (required, 2 decimals) -->
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

              <!-- Notes (optional) -->
              <v-textarea
                v-model="formData.notes"
                label="Notes"
                variant="outlined"
                density="compact"
                rows="3"
              />
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
