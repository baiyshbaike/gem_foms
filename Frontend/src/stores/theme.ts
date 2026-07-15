import { defineStore } from 'pinia'

import type { ContentLayout, Radius, Theme } from '@/constants/themes'

export const useThemeStore = defineStore('system-config', () => {
  const radius = ref(1)
  function setRadius(newRadius: Radius) {
    radius.value = newRadius
  }
  const theme = ref<Theme>('green')
  function setTheme(newTheme: Theme) {
    theme.value = newTheme
  }

  const contentLayout = ref<ContentLayout>('full')
  function setContentLayout(newContentLayout: ContentLayout) {
    contentLayout.value = newContentLayout
  }
  return {
    radius,
    setRadius,

    theme,
    setTheme,

    contentLayout,
    setContentLayout,
  }
}, {
  persist: true,
})
