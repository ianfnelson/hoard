import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'vuetify/styles/main.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

import router from "./router";
import App from './App.vue'

const vuetify = createVuetify({
  components,
  directives,
})

createApp(App)
  .use(createPinia())
  .use(router)
  .use(vuetify)
  .mount('#app')
