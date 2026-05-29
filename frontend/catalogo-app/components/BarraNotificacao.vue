<script setup lang="ts">
defineProps<{
  modelValue: boolean
  mensagem: string
  tipo?: 'success' | 'error'
}>()

defineEmits<{
  'update:modelValue': [value: boolean]
}>()
</script>

<template>
  <v-snackbar
    :model-value="modelValue"
    :color="tipo === 'error' ? 'error' : 'success'"
    location="top"
    :timeout="tipo === 'error' ? 5000 : 3000"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <v-icon start>{{ tipo === 'error' ? 'mdi-alert-circle' : 'mdi-check-circle' }}</v-icon>
    {{ mensagem }}
  </v-snackbar>
</template>
