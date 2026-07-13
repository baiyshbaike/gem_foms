<script lang="ts" setup>
import { Building2Icon, ChevronsUpDownIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import { useSidebar } from '@/components/ui/sidebar'

import type { Team } from './types'

const { teams } = defineProps<{
  teams: Team[]
}>()

const { isMobile, open } = useSidebar()
const authStore = useAuthStore()
const { activeTenant, isLogin, tenants } = storeToRefs(authStore)

const fallbackTeam = computed(() => teams[0])
const activeLabel = computed(() => activeTenant.value?.name ?? fallbackTeam.value.name)
const activeSubLabel = computed(() => activeTenant.value?.code ?? fallbackTeam.value.plan)

async function switchTenant(tenantId: string) {
  try {
    await authStore.switchTenant(tenantId)
    toast.success('Tenant changed')
  }
  catch {
    toast.error('Tenant could not be changed')
  }
}

onMounted(() => {
  if (isLogin.value) {
    authStore.loadTenants().catch(() => undefined)
  }
})
</script>

<template>
  <UiSidebarMenu>
    <UiSidebarMenuItem>
      <UiDropdownMenu>
        <UiDropdownMenuTrigger as-child>
          <UiSidebarMenuButton
            size="lg"
            class="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
          >
            <div
              class="flex items-center justify-center rounded-lg aspect-square size-8 bg-sidebar-primary text-sidebar-primary-foreground"
            >
              <Building2Icon class="size-4" />
            </div>
            <div class="grid flex-1 text-sm leading-tight text-left">
              <span class="font-semibold truncate">{{ activeLabel }}</span>
              <span class="text-xs truncate">{{ activeSubLabel }}</span>
            </div>
            <ChevronsUpDownIcon class="ml-auto" />
          </UiSidebarMenuButton>
        </UiDropdownMenuTrigger>
        <UiDropdownMenuContent
          class="w-(--radix-dropdown-menu-trigger-width) min-w-56 rounded-lg"
          align="start"
          :side="(isMobile || open) ? 'bottom' : 'right'"
          :side-offset="4"
        >
          <UiDropdownMenuLabel class="text-xs text-muted-foreground">
            Tenants
          </UiDropdownMenuLabel>
          <UiDropdownMenuItem
            v-if="tenants.length === 0"
            disabled
            class="gap-2 p-2"
          >
            No tenant access
          </UiDropdownMenuItem>
          <UiDropdownMenuItem
            v-for="tenant in tenants"
            :key="tenant.id"
            class="gap-2 p-2"
            @click="switchTenant(tenant.id)"
          >
            <div class="flex items-center justify-center border rounded-sm size-6">
              <Building2Icon class="size-4 shrink-0" />
            </div>
            <div class="grid text-sm leading-tight">
              <span>{{ tenant.name }}</span>
              <span class="text-xs text-muted-foreground">{{ tenant.code }}</span>
            </div>
          </UiDropdownMenuItem>
        </UiDropdownMenuContent>
      </UiDropdownMenu>
    </UiSidebarMenuItem>
  </UiSidebarMenu>
</template>
