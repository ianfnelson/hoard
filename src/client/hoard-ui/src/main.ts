import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'vuetify/lib/styles/main.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { aliases, mdi } from 'vuetify/iconsets/mdi'

import router from './router'
import App from './App.vue'
import './styles/global.css'

const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: {
      mdi,
    },
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          // Can add custom colors here if needed
        },
      },
      dark: {
        colors: {
          // Can add custom dark theme colors here if needed
        },
      },
    },
  },
  defaults: {
    VApp: {
      style: {
        fontFamily: '"IBM Plex Sans", sans-serif',
      },
    },
  },
})

createApp(App).use(createPinia()).use(router).use(vuetify).mount('#app')
