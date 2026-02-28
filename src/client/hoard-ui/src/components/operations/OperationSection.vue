<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <div class="mb-4">
    <h3 class="text-h6 mb-2">{{ title }}</h3>
    <div class="d-flex flex-wrap ga-2 mb-2">
      <slot />
    </div>
    <v-divider />
  </div>
</template>
