<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const config = useRuntimeConfig()
const baseUrl = config.public.urlApi

interface Categoria {
  id: number
  nome: string
}

interface Produto {
  id: number
  nome: string
  descricao: string
  preco: number
  categoriaId: number
  categoria: Categoria
}

const produtos = ref<Produto[]>([])
const categorias = ref<Categoria[]>([])
const carregando = ref(false)
const carregandoCategorias = ref(false)
const processando = ref(false)
const dialogoFormulario = ref(false)
const dialogoExclusao = ref(false)
const modoEdicao = ref(false)
const idEditando = ref<number | null>(null)
const idParaExcluir = ref<number | null>(null)
const form = ref({ nome: '', descricao: '', preco: 0, categoriaId: null as number | null })
const snackbarSucesso = ref(false)
const snackbarErro = ref(false)
const mensagemSucesso = ref('')
const mensagemErro = ref('')

const cabecalhosTabelaProduto = [
  { title: 'ID', key: 'id', width: '70px' },
  { title: 'Nome', key: 'nome' },
  { title: 'Descrição', key: 'descricao' },
  { title: 'Preço', key: 'preco' },
  { title: 'Categoria', key: 'categoria.nome' },
  { title: 'Ações', key: 'acoes', sortable: false, align: 'center' as const },
]

const formularioValido = computed(
  () => form.value.nome.trim().length >= 5 && form.value.categoriaId !== null,
)

function formatarPreco(valor: number) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

async function carregarProdutos() {
  carregando.value = true
  try {
    const resposta = await axios.get<Produto[]>(`${baseUrl}/api/produtos`)
    produtos.value = resposta.data
  } catch {
    mensagemErro.value = 'Erro ao carregar produtos.'
    snackbarErro.value = true
  } finally {
    carregando.value = false
  }
}

async function carregarCategorias() {
  carregandoCategorias.value = true
  try {
    const resposta = await axios.get<Categoria[]>(`${baseUrl}/api/categorias`)
    categorias.value = resposta.data
  } catch {
    mensagemErro.value = 'Erro ao carregar categorias.'
    snackbarErro.value = true
  } finally {
    carregandoCategorias.value = false
  }
}

function abrirNovo() {
  modoEdicao.value = false
  idEditando.value = null
  form.value = { nome: '', descricao: '', preco: 0, categoriaId: null }
  dialogoFormulario.value = true
}

function abrirEditar(produto: Produto) {
  modoEdicao.value = true
  idEditando.value = produto.id
  form.value = {
    nome: produto.nome,
    descricao: produto.descricao,
    preco: produto.preco,
    categoriaId: produto.categoriaId,
  }
  dialogoFormulario.value = true
}

async function salvar() {
  if (!formularioValido.value) return
  processando.value = true
  try {
    if (modoEdicao.value && idEditando.value !== null) {
      await axios.put(`${baseUrl}/api/produtos/${idEditando.value}`, form.value)
      const indice = produtos.value.findIndex(p => p.id === idEditando.value)
      if (indice !== -1) {
        const categoria = categorias.value.find(c => c.id === form.value.categoriaId)
        produtos.value[indice] = {
          ...produtos.value[indice],
          nome: form.value.nome,
          descricao: form.value.descricao,
          preco: form.value.preco,
          categoriaId: form.value.categoriaId!,
          categoria: categoria ?? produtos.value[indice].categoria,
        }
      }
      mensagemSucesso.value = 'Produto atualizado com sucesso!'
    } else {
      const resposta = await axios.post<Produto>(`${baseUrl}/api/produtos`, form.value)
      produtos.value.push(resposta.data)
      mensagemSucesso.value = 'Produto criado com sucesso!'
    }
    snackbarSucesso.value = true
    dialogoFormulario.value = false
  } catch (e: any) {
    mensagemErro.value = e.response?.data?.mensagem ?? e.response?.data?.title ?? 'Erro ao salvar produto.'
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
    await axios.delete(`${baseUrl}/api/produtos/${idParaExcluir.value}`)
    produtos.value = produtos.value.filter(p => p.id !== idParaExcluir.value)
    mensagemSucesso.value = 'Produto excluído com sucesso!'
    snackbarSucesso.value = true
    dialogoExclusao.value = false
  } catch (e: any) {
    mensagemErro.value = e.response?.data?.mensagem ?? e.response?.data?.title ?? 'Não foi possível excluir este produto.'
    snackbarErro.value = true
    dialogoExclusao.value = false
  } finally {
    processando.value = false
    idParaExcluir.value = null
  }
}

onMounted(async () => {
  await Promise.all([carregarProdutos(), carregarCategorias()])
})
</script>

<template>
  <div>
    <v-card elevation="2" rounded="lg">
      <v-card-title class="d-flex align-center pa-4" style="color: #1565C0;">
        <v-icon start color="primary">mdi-package-variant</v-icon>
        Produtos
        <v-spacer />
        <v-btn color="primary" variant="elevated" prepend-icon="mdi-plus" @click="abrirNovo">
          Novo Produto
        </v-btn>
      </v-card-title>
      <v-divider />
      <v-data-table
        :items="produtos"
        :headers="cabecalhosTabelaProduto"
        :loading="carregando"
        density="comfortable"
        item-value="id"
      >
        <template #item.preco="{ item }">
          {{ formatarPreco(item.preco) }}
        </template>
        <template #[`item.categoria.nome`]="{ item }">
          {{ item.categoria?.nome ?? '—' }}
        </template>
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
          {{ modoEdicao ? 'Editar Produto' : 'Novo Produto' }}
        </v-card-title>
        <v-divider />
        <v-card-text class="pa-4 d-flex flex-column ga-3">
          <v-text-field
            v-model="form.nome"
            label="Nome"
            variant="outlined"
            density="compact"
            :rules="[(v: string) => v.trim().length >= 5 || 'Mínimo 5 caracteres']"
            validate-on="input"
          />
          <v-text-field
            v-model="form.descricao"
            label="Descrição"
            variant="outlined"
            density="compact"
          />
          <v-text-field
            v-model.number="form.preco"
            label="Preço"
            variant="outlined"
            density="compact"
            type="number"
            prefix="R$"
            min="0"
            step="0.01"
          />
          <v-select
            v-model="form.categoriaId"
            :items="categorias"
            item-title="nome"
            item-value="id"
            label="Categoria"
            variant="outlined"
            density="compact"
            :loading="carregandoCategorias"
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
      mensagem="Tem certeza que deseja excluir este produto? Esta ação não pode ser desfeita."
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
