<script setup lang="ts">
defineProps<{
  label: string
  icon?: string
}>()

defineEmits<{
  click: []
}>()
</script>

<template>
  <v-btn variant="outlined" :prepend-icon="icon ?? 'mdi-play-circle'" @click="$emit('click')">
    {{ label }}
  </v-btn>
</template>
