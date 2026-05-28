import { createVuetify } from 'vuetify'
import * as componentes from 'vuetify/components'
import * as diretivas from 'vuetify/directives'
import { pt } from 'vuetify/locale'

export default defineNuxtPlugin((app) => {
  const vuetify = createVuetify({
    components: componentes,
    directives: diretivas,
    locale: { locale: 'pt', messages: { pt } },
    theme: {
      defaultTheme: 'temaZDZCode',
      themes: {
        temaZDZCode: {
          dark: false,
          colors: {
            primary: '#1565C0', secondary: '#1976D2',
            accent: '#42A5F5', success: '#2E7D32',
            warning: '#F57F17', error: '#C62828',
            surface: '#FFFFFF', background: '#F5F5F5',
          }
        }
      }
    }
  })
  app.vueApp.use(vuetify)
})