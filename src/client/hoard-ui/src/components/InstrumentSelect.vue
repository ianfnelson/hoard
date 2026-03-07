<script setup lang="ts">
import { computed } from 'vue'
import { useReferenceDataStore } from '@/stores/referenceDataStore'

interface Props {
  modelValue: number | null
  label?: string
  rules?: Array<(v: unknown) => boolean | string>
  required?: boolean
  clearable?: boolean
  variant?: 'outlined' | 'filled' | 'underlined' | 'solo' | 'plain'
  density?: 'default' | 'comfortable' | 'compact'
  hideDetails?: boolean
  hint?: string
  persistentHint?: boolean
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  label: 'Instrument',
  rules: () => [],
  required: false,
  clearable: true,
  variant: 'outlined',
  density: 'compact',
  hideDetails: false,
  hint: undefined,
  persistentHint: false,
  disabled: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const refStore = useReferenceDataStore()

// Transform instruments into autocomplete items
const instrumentItems = computed(() => {
  return refStore.instruments.map((i) => ({
    title: i.context ? `${i.name} (${i.context})` : i.name,
    value: i.id,
    // Store searchable text for custom filtering
    searchText: `${i.name} ${i.context || ''}`.toLowerCase(),
  }))
})

// Custom filter function that searches both name and context
function customFilter(
  _itemTitle: string,
  queryText: string,
  item?: { raw: { searchText: string } }
) {
  if (!item?.raw?.searchText) return false
  const query = queryText.toLowerCase()
  return item.raw.searchText.includes(query)
}

const selectedValue = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})
</script>

<template>
  <v-autocomplete
    v-model="selectedValue"
    :items="instrumentItems"
    :label="label"
    :rules="rules"
    :required="required"
    :clearable="clearable"
    :variant="variant"
    :density="density"
    :hide-details="hideDetails"
    :hint="hint"
    :persistent-hint="persistentHint"
    :disabled="disabled"
    :custom-filter="customFilter"
    auto-select-first
  />
</template>
