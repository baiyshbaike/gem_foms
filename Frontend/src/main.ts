import config from 'devextreme/core/config'
import { createApp } from 'vue'

import { licenseKey } from '../devextreme-license'
import App from './App.vue'
import { setupDevExtremeTheme } from './lib/devextreme-theme'
import { setupPlugins } from './plugins'

import '@/assets/index.css'
import '@/assets/scrollbar.css'
import '@/assets/themes.css'
import '@/assets/chart-theme.css'
import 'vue-sonner/style.css' // vue sonner style

import '@/utils/env'

config({ licenseKey })

async function bootstrap() {
  await setupDevExtremeTheme()

  const app = createApp(App)

  setupPlugins(app)

  app.mount('#app')
}

void bootstrap()
