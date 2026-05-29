<script setup lang="ts">
defineProps<{
  modelValue: boolean
  titulo?: string
  mensagem?: string
  loading?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'confirmar': []
}>()
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="400"
    rounded="lg"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card elevation="2" rounded="lg">
      <v-card-title class="pa-4 d-flex align-center" style="color: #C62828;">
        <v-icon start color="error">mdi-alert-circle</v-icon>
        {{ titulo ?? 'Confirmar Exclusão' }}
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-4">
        {{ mensagem ?? 'Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.' }}
      </v-card-text>
      <v-card-actions class="pa-4">
        <v-spacer />
        <v-btn variant="text" @click="emit('update:modelValue', false)">Cancelar</v-btn>
        <v-btn
          color="error"
          variant="elevated"
          prepend-icon="mdi-delete"
          :loading="loading"
          @click="emit('confirmar')"
        >
          Excluir
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
