<script setup lang="ts">
defineProps<{
  error: string | null
  retryable?: boolean
}>()

defineEmits<{
  retry: []
}>()
</script>

<template>
  <v-alert v-if="error" type="error" variant="tonal" class="mb-4">
    {{ error }}
    <template v-if="retryable" #append>
      <v-btn variant="text" size="small" @click="$emit('retry')">Retry</v-btn>
    </template>
  </v-alert>
</template>
