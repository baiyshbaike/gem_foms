<script setup lang="ts">
import { BasicPage, TwoColAside, TwoColLayout } from '@/components/global-layout'

const { settingsNavItems } = useSidebar()
</script>

<template>
  <BasicPage
    title="Settings"
    description="Manage tenant settings."
  >
    <TwoColLayout>
      <template #aside>
        <TwoColAside :nav="settingsNavItems" />
      </template>

      <slot />
    </TwoColLayout>
  </BasicPage>
</template>
