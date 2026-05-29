import vuetify from 'vite-plugin-vuetify'

export default defineNuxtConfig({
  srcDir: '.',
  ssr: false,
  css: ['vuetify/styles', '@mdi/font/css/materialdesignicons.css'],
  build: { transpile: ['vuetify'] },
  vite: { plugins: [vuetify({ autoImport: true })] },
  plugins: ['~/plugins/vuetify.ts'],
  runtimeConfig: {
    public: { urlApi: 'http://localhost:5193' }
  }
})