<script lang="ts" setup>
import { storeToRefs } from 'pinia'

import { filterNavigationByPermissions } from '@/composables/use-authorized-navigation'
import { useAuthStore } from '@/stores/auth'

import { sidebarData } from './data/sidebar-data'
import NavFooter from './nav-footer.vue'
import NavTeam from './nav-team.vue'
import TeamSwitcher from './team-switcher.vue'

const authStore = useAuthStore()
const { permissions } = storeToRefs(authStore)
const authorizedNavMain = computed(() =>
  filterNavigationByPermissions(sidebarData.navMain, permissions.value),
)
</script>

<template>
  <UiSidebar collapsible="icon" class="z-50">
    <UiSidebarHeader>
      <TeamSwitcher :teams="sidebarData.teams" />
    </UiSidebarHeader>

    <UiSidebarContent>
      <NavTeam :nav-main="authorizedNavMain" />
    </UiSidebarContent>

    <UiSidebarFooter>
      <NavFooter :user="sidebarData.user" />
    </UiSidebarFooter>

    <UiSidebarRail />
  </UiSidebar>
</template>
