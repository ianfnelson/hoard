<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'
import { usePortfolioStore } from '@/stores/portfolioStore'
import { setTargetAllocations } from '@/api/portfoliosApi'
import { useTargetAllocations } from '@/composables/useTargetAllocations'
import { formatPercentage } from '@/utils/formatters'
import ApiErrorAlert from '@/components/ApiErrorAlert.vue'

const props = defineProps<{
  modelValue: boolean
  portfolioId: number
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: []
}>()

const refStore = useReferenceDataStore()
const portfolioStore = usePortfolioStore()

const { rows, total, cashRemainder, isValid, initialise, buildPayload } = useTargetAllocations()

const isSaving = ref(false)
const error = ref<string | null>(null)

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

const groupedRows = computed(() => {
  const groups = new Map<string, typeof rows.value>()
  for (const row of rows.value) {
    const existing = groups.get(row.assetClassName)
    if (existing) {
      existing.push(row)
    } else {
      groups.set(row.assetClassName, [row])
    }
  }
  return [...groups.entries()].map(([assetClassName, items]) => ({ assetClassName, items }))
})

watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return

    error.value = null
    await refStore.loadAssetClasses()

    const currentTargets = new Map<number, number>(
      (portfolioStore.exposure?.assetSubclasses ?? [])
        .filter((sc) => sc.targetPercentage != null)
        .map((sc) => [sc.id, sc.targetPercentage as number])
    )

    initialise(refStore.assetClasses, currentTargets)
  },
  { immediate: true }
)

function close() {
  dialogVisible.value = false
}

async function save() {
  if (!isValid.value) return

  isSaving.value = true
  error.value = null

  try {
    await setTargetAllocations(props.portfolioId, buildPayload())
    await portfolioStore.refresh()
    emit('saved')
    close()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to save target allocations'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <v-dialog v-model="dialogVisible" max-width="640" scrollable>
    <v-card>
      <v-card-title>Edit Target Allocations</v-card-title>
      <v-card-text>
        <ApiErrorAlert :error="error" />

        <template v-for="group in groupedRows" :key="group.assetClassName">
          <div class="text-subtitle-2 mt-2 mb-1">{{ group.assetClassName }}</div>
          <v-text-field
            v-for="row in group.items"
            :key="row.assetSubclassId"
            v-model.number="row.target"
            :label="row.name"
            type="number"
            min="0"
            max="100"
            step="0.01"
            suffix="%"
            variant="outlined"
            density="compact"
            hide-details
            class="mb-2"
          />
        </template>

        <v-divider class="my-3" />

        <div class="d-flex justify-space-between">
          <span class="text-subtitle-2">Cash (remainder)</span>
          <span class="text-subtitle-2">{{ formatPercentage(cashRemainder) }}</span>
        </div>
        <div class="d-flex justify-space-between mt-1">
          <span :class="isValid ? '' : 'text-error'">Total</span>
          <span :class="isValid ? '' : 'text-error'">{{ formatPercentage(total) }}</span>
        </div>
        <div v-if="!isValid" class="text-error text-caption mt-1">
          Total must be between 0% and 100%.
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="close">Cancel</v-btn>
        <v-btn
          color="primary"
          variant="flat"
          :loading="isSaving"
          :disabled="!isValid"
          @click="save"
        >
          Save
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
