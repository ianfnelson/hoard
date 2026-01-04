import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'vuetify/lib/styles/main.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { aliases, mdi } from 'vuetify/iconsets/mdi'

import router from "./router";
import App from './App.vue'

const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets : {
      mdi
    }
  }
})

createApp(App)
  .use(createPinia())
  .use(router)
  .use(vuetify)
  .mount('#app')
