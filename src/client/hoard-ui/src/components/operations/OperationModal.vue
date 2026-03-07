<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { OperationDefinition } from '@/types/operations'
import { useNavigationStore } from '@/stores/navigationStore'
import InstrumentSelect from '@/components/InstrumentSelect.vue'

const props = defineProps<{
  modelValue: boolean
  operation: OperationDefinition | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: [params: Record<string, unknown>]
}>()

const navStore = useNavigationStore()

const formData = ref<Record<string, unknown>>({})
const isLoading = ref(false)
const error = ref<string | null>(null)

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

// Reset form when operation changes
watch(
  () => props.operation,
  (newOp) => {
    if (newOp) {
      formData.value = {}
      error.value = null
    }
  }
)

// Portfolio items
const portfolioItems = computed(() => {
  return navStore.portfolios.map((p) => ({
    title: p.name,
    value: p.id,
  }))
})

// Validation
const isValid = computed(() => {
  if (!props.operation) return false

  // Check required fields
  for (const field of props.operation.fields) {
    if (field.required && !formData.value[field.name]) {
      return false
    }
  }

  // Date range validation
  const startDate = formData.value.startDate as string | undefined
  const endDate = formData.value.endDate as string | undefined
  if (startDate && endDate && startDate > endDate) {
    return false
  }

  // Year range validation
  const startYear = formData.value.startYear as number | undefined
  const endYear = formData.value.endYear as number | undefined
  if (startYear && endYear && startYear > endYear) {
    return false
  }

  return true
})

function close() {
  dialogVisible.value = false
}

async function confirm() {
  if (!isValid.value || !props.operation) return

  isLoading.value = true
  error.value = null

  try {
    // Filter out empty values
    const params: Record<string, unknown> = {}
    for (const [key, value] of Object.entries(formData.value)) {
      if (value !== null && value !== undefined && value !== '') {
        params[key] = value
      }
    }

    emit('confirm', params)
    close()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'An error occurred'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <v-dialog v-model="dialogVisible" max-width="600">
    <v-card v-if="operation">
      <v-card-title>{{ operation.label }}</v-card-title>
      <v-card-text>
        <v-alert v-if="error" type="error" variant="tonal" class="mb-4">
          {{ error }}
        </v-alert>

        <div v-if="operation.fields.length === 0" class="text-body-1">
          Are you sure you want to trigger this operation?
        </div>

        <div v-else class="d-flex flex-column ga-4">
          <template v-for="field in operation.fields" :key="field.name">
            <!-- Date field -->
            <v-text-field
              v-if="field.type === 'date'"
              v-model="formData[field.name]"
              :label="field.label"
              type="date"
              variant="outlined"
              density="compact"
              hide-details
              hint="Optional - defaults to today"
              persistent-hint
            />

            <!-- Number field -->
            <v-text-field
              v-else-if="field.type === 'number'"
              v-model.number="formData[field.name]"
              :label="field.label"
              type="number"
              variant="outlined"
              density="compact"
              hide-details
              :min="field.min"
              :max="field.max"
              hint="Optional"
              persistent-hint
            />

            <!-- Instrument selector -->
            <InstrumentSelect
              v-else-if="field.type === 'instrument'"
              v-model="formData[field.name] as number | null"
              :label="field.label"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              hint="Optional - applies to all instruments if not specified"
              persistent-hint
            />

            <!-- Portfolio selector -->
            <v-select
              v-else-if="field.type === 'portfolio'"
              v-model="formData[field.name]"
              :label="field.label"
              :items="portfolioItems"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              hint="Optional - applies to all portfolios if not specified"
              persistent-hint
            />
          </template>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="close"> Cancel </v-btn>
        <v-btn color="primary" variant="flat" :loading="isLoading" @click="confirm">
          Trigger
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
