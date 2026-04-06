<script setup lang="ts">
defineProps<{
  modelValue: boolean
  title: string
  message: string
  loading: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
}>()
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="500"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card>
      <v-card-title>{{ title }}</v-card-title>
      <v-card-text>{{ message }}</v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="flat" prepend-icon="mdi-close" @click="emit('update:modelValue', false)">
          Cancel
        </v-btn>
        <v-btn
          color="error"
          variant="flat"
          prepend-icon="mdi-delete"
          :loading="loading"
          @click="emit('confirm')"
        >
          Delete
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
