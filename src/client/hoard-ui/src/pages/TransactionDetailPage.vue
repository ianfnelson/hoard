<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTransactionDetail } from '@/composables/useTransactionDetail'
import { usePageTitle } from '@/composables/usePageTitle'
import { uploadContractNote } from '@/api/transactionsApi'
import type { TransactionWriteDto } from '@/api/dtos/Transactions/TransactionWriteDto'
import TransactionViewCard from '@/components/transactions/TransactionViewCard.vue'
import TransactionForm from '@/components/transactions/TransactionForm.vue'
import DeleteConfirmDialog from '@/components/DeleteConfirmDialog.vue'

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

// Delete dialog
const showDeleteDialog = ref(false)

// Contract note upload state
const isUploadingFile = ref(false)
const uploadError = ref<string | null>(null)

// Actions
async function handleSave(payload: TransactionWriteDto & { contractNoteFile: File | null }) {
  try {
    let transactionId: number
    const { contractNoteFile, ...writeData } = payload

    if (isCreateMode.value) {
      const id = await create(writeData)
      if (!id) return
      transactionId = id
    } else {
      transactionId = Number(props.id)
      const success = await update(transactionId, writeData)
      if (!success) return
    }

    // Upload file if present
    if (contractNoteFile) {
      isUploadingFile.value = true
      uploadError.value = null
      try {
        await uploadContractNote(transactionId, contractNoteFile)
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

    <v-row v-if="uploadError" dense>
      <v-col>
        <v-alert type="warning" dismissible @click:close="uploadError = null">
          {{ uploadError }}
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
        <TransactionViewCard
          v-if="isViewMode && transaction"
          :transaction="transaction"
          @edit="router.push({ query: { mode: 'edit' } })"
          @delete="showDeleteDialog = true"
        />

        <TransactionForm
          v-if="isFormMode"
          :transaction="isEditMode ? (transaction ?? null) : null"
          :is-saving="isSaving || isUploadingFile"
          @save="handleSave"
          @cancel="handleCancel"
        />
      </v-col>
    </v-row>

    <DeleteConfirmDialog
      v-model="showDeleteDialog"
      title="Delete Transaction?"
      message="Are you sure you want to delete this transaction? This action cannot be undone."
      :loading="isDeleting"
      @confirm="confirmDelete"
    />
  </v-container>
</template>
