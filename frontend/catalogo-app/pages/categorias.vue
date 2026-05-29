<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const config = useRuntimeConfig()
const baseUrl = config.public.urlApi

interface Categoria {
  id: number
  nome: string
}

const categorias = ref<Categoria[]>([])
const carregando = ref(false)
const processando = ref(false)
const dialogoFormulario = ref(false)
const dialogoExclusao = ref(false)
const modoEdicao = ref(false)
const idEditando = ref<number | null>(null)
const idParaExcluir = ref<number | null>(null)
const form = ref({ nome: '' })
const snackbarSucesso = ref(false)
const snackbarErro = ref(false)
const mensagemSucesso = ref('')
const mensagemErro = ref('')

const cabecalhos = [
  { title: 'ID', key: 'id', width: '70px' },
  { title: 'Nome', key: 'nome' },
  { title: 'Ações', key: 'acoes', sortable: false, align: 'center' as const },
]

const formularioValido = computed(() => form.value.nome.trim().length >= 5)

async function carregarCategorias() {
  carregando.value = true
  try {
    const resposta = await axios.get<Categoria[]>(`${baseUrl}/api/categorias`)
    categorias.value = resposta.data
  } catch {
    mensagemErro.value = 'Erro ao carregar categorias.'
    snackbarErro.value = true
  } finally {
    carregando.value = false
  }
}

function abrirNovo() {
  modoEdicao.value = false
  idEditando.value = null
  form.value = { nome: '' }
  dialogoFormulario.value = true
}

function abrirEditar(categoria: Categoria) {
  modoEdicao.value = true
  idEditando.value = categoria.id
  form.value = { nome: categoria.nome }
  dialogoFormulario.value = true
}

async function salvar() {
  if (!formularioValido.value) return
  processando.value = true
  try {
    if (modoEdicao.value && idEditando.value !== null) {
      await axios.put(`${baseUrl}/api/categorias/${idEditando.value}`, form.value)
      const indice = categorias.value.findIndex(c => c.id === idEditando.value)
      if (indice !== -1) categorias.value[indice] = { ...categorias.value[indice], nome: form.value.nome }
      mensagemSucesso.value = 'Categoria atualizada com sucesso!'
    } else {
      const resposta = await axios.post<Categoria>(`${baseUrl}/api/categorias`, form.value)
      categorias.value.push(resposta.data)
      mensagemSucesso.value = 'Categoria criada com sucesso!'
    }
    snackbarSucesso.value = true
    dialogoFormulario.value = false
  } catch (e: any) {
    mensagemErro.value = e.response?.data?.mensagem ?? e.response?.data?.title ?? 'Erro ao salvar categoria.'
    snackbarErro.value = true
  } finally {
    processando.value = false
  }
}

function confirmarExclusao(id: number) {
  idParaExcluir.value = id
  dialogoExclusao.value = true
}

async function excluir() {
  if (idParaExcluir.value === null) return
  processando.value = true
  try {
    await axios.delete(`${baseUrl}/api/categorias/${idParaExcluir.value}`)
    categorias.value = categorias.value.filter(c => c.id !== idParaExcluir.value)
    mensagemSucesso.value = 'Categoria excluída com sucesso!'
    snackbarSucesso.value = true
    dialogoExclusao.value = false
  } catch (e: any) {
    mensagemErro.value = e.response?.data?.mensagem ?? e.response?.data?.title ?? 'Não foi possível excluir esta categoria.'
    snackbarErro.value = true
    dialogoExclusao.value = false
  } finally {
    processando.value = false
    idParaExcluir.value = null
  }
}

onMounted(carregarCategorias)
</script>

<template>
  <div>
    <v-card elevation="2" rounded="lg">
      <v-card-title class="d-flex align-center pa-4" style="color: #1565C0;">
        <v-icon start color="primary">mdi-shape</v-icon>
        Categorias
        <v-spacer />
        <v-btn color="primary" variant="elevated" prepend-icon="mdi-plus" @click="abrirNovo">
          Nova Categoria
        </v-btn>
      </v-card-title>
      <v-divider />
      <v-data-table
        :items="categorias"
        :headers="cabecalhos"
        :loading="carregando"
        density="comfortable"
        item-value="id"
      >
        <template #item.acoes="{ item }">
          <v-btn
            color="secondary"
            variant="text"
            icon
            density="compact"
            @click="abrirEditar(item)"
          >
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn
            color="error"
            variant="text"
            icon
            density="compact"
            @click="confirmarExclusao(item.id)"
          >
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </v-card>

    <!-- Diálogo Criar / Editar -->
    <v-dialog v-model="dialogoFormulario" max-width="600" rounded="lg">
      <v-card elevation="2" rounded="lg">
        <v-card-title class="pa-4 d-flex align-center" style="color: #1565C0;">
          <v-icon start color="primary">
            {{ modoEdicao ? 'mdi-pencil' : 'mdi-plus-circle' }}
          </v-icon>
          {{ modoEdicao ? 'Editar Categoria' : 'Nova Categoria' }}
        </v-card-title>
        <v-divider />
        <v-card-text class="pa-4">
          <v-text-field
            v-model="form.nome"
            label="Nome"
            variant="outlined"
            density="compact"
            :rules="[(v: string) => v.trim().length >= 5 || 'Mínimo 5 caracteres']"
            validate-on="input"
          />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="dialogoFormulario = false">Cancelar</v-btn>
          <v-btn
            color="primary"
            variant="elevated"
            prepend-icon="mdi-content-save"
            :disabled="!formularioValido"
            :loading="processando"
            @click="salvar"
          >
            Salvar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Diálogo Confirmação de Exclusão -->
    <DialogoConfirmacao
      v-model="dialogoExclusao"
      mensagem="Tem certeza que deseja excluir esta categoria? Esta ação não pode ser desfeita."
      :loading="processando"
      @confirmar="excluir"
    />

    <!-- Notificações -->
    <BarraNotificacao v-model="snackbarSucesso" :mensagem="mensagemSucesso" tipo="success" />
    <BarraNotificacao v-model="snackbarErro" :mensagem="mensagemErro" tipo="error" />
  </div>
</template>

<style scoped>
:deep(thead th) {
  background-color: #1565C0 !important;
  color: white !important;
}
</style>
