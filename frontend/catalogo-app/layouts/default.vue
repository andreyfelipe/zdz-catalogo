<template>
  <v-app theme="temaZDZCode">
    <v-app-bar color="primary" elevation="2">
      <v-app-bar-title>
        <span class="font-weight-bold">ZDZCode</span> — Gestão de Catálogo
      </v-app-bar-title>
      <template #append>
        <v-btn to="/categorias" variant="text" color="white">
          <v-icon start>mdi-shape</v-icon>Categorias
        </v-btn>
        <v-btn to="/produtos" variant="text" color="white">
          <v-icon start>mdi-package-variant</v-icon>Produtos
        </v-btn>
      </template>
    </v-app-bar>
    <v-main style="background:#F5F5F5">
      <v-container fluid class="pa-6"><slot /></v-container>
    </v-main>
  </v-app>
</template>