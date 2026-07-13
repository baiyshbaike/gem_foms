<script setup lang="ts">
import { BadgeCheckIcon, BellIcon, ChevronsUpDownIcon, CreditCardIcon, LogOutIcon, SparklesIcon, UserRoundCogIcon } from '@lucide/vue'
import { storeToRefs } from 'pinia'

import { useSidebar } from '@/components/ui/sidebar'

import type { User } from './types'

const { user } = defineProps<
  { user: User }
>()

const { logout } = useAuth()
const { isMobile, open } = useSidebar()
const authStore = useAuthStore()
const { user: authUser } = storeToRefs(authStore)

const displayName = computed(() => {
  const fullName = [authUser.value?.firstName, authUser.value?.lastName].filter(Boolean).join(' ')
  return fullName || user.name
})

const displayEmail = computed(() => authUser.value?.username ?? user.email)
const displayInitials = computed(() => {
  const source = displayName.value || displayEmail.value
  return source.slice(0, 2).toUpperCase()
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
            <UiAvatar class="size-8 rounded-lg">
              <UiAvatarImage :src="user.avatar" :alt="displayName" />
              <UiAvatarFallback class="rounded-lg">
                {{ displayInitials }}
              </UiAvatarFallback>
            </UiAvatar>
            <div class="grid flex-1 text-sm leading-tight text-left">
              <span class="font-semibold truncate">{{ displayName }}</span>
              <span class="text-xs truncate">{{ displayEmail }}</span>
            </div>
            <ChevronsUpDownIcon class="ml-auto size-4" />
          </UiSidebarMenuButton>
        </UiDropdownMenuTrigger>
        <UiDropdownMenuContent
          class="w-(--radix-dropdown-menu-trigger-width) min-w-56 rounded-lg"
          :side="(isMobile || open) ? 'bottom' : 'right'"
          align="start"
          :side-offset="4"
        >
          <UiDropdownMenuLabel class="p-0 font-normal">
            <div class="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
              <UiAvatar class="size-8 rounded-lg">
                <UiAvatarImage :src="user.avatar" :alt="displayName" />
                <UiAvatarFallback class="rounded-lg">
                  {{ displayInitials }}
                </UiAvatarFallback>
              </UiAvatar>
              <div class="grid flex-1 text-sm leading-tight text-left">
                <span class="font-semibold truncate">{{ displayName }}</span>
                <span class="text-xs truncate">{{ displayEmail }}</span>
              </div>
            </div>
          </UiDropdownMenuLabel>

          <UiDropdownMenuSeparator />
          <UiDropdownMenuGroup>
            <UiDropdownMenuItem @click="$router.push('/billing/')">
              <SparklesIcon />
              Upgrade to Pro
            </UiDropdownMenuItem>
          </UiDropdownMenuGroup>

          <UiDropdownMenuSeparator />
          <UiDropdownMenuGroup>
            <UiDropdownMenuItem @click="$router.push('/billing?type=billing')">
              <CreditCardIcon />
              Billing
            </UiDropdownMenuItem>
          </UiDropdownMenuGroup>

          <UiDropdownMenuSeparator />
          <UiDropdownMenuGroup>
            <UiDropdownMenuItem @click="$router.push('/settings/')">
              <UserRoundCogIcon />
              Profile
            </UiDropdownMenuItem>
            <UiDropdownMenuItem @click="$router.push('/settings/account')">
              <BadgeCheckIcon />
              Account
            </UiDropdownMenuItem>
            <UiDropdownMenuItem @click="$router.push('/settings/notifications')">
              <BellIcon />
              Notifications
            </UiDropdownMenuItem>
          </UiDropdownMenuGroup>

          <UiDropdownMenuSeparator />
          <UiDropdownMenuItem @click="logout">
            <LogOutIcon />
            {{ $t('logout') }}
          </UiDropdownMenuItem>
        </UiDropdownMenuContent>
      </UiDropdownMenu>
    </UiSidebarMenuItem>
  </UiSidebarMenu>
</template>
