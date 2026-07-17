import { defineStore } from 'pinia'

import type { AuthSession, AuthUser, Tenant } from '@/services/types/dialysis'

import { authApi, tenantApi } from '@/services/api/dialysis.api'
import {
  clearStoredAuthSession,
  getStoredAuthSession,
  setStoredAuthSession,
} from '@/services/auth-session'
import { resolveTenantSelection } from '@/services/tenant-selection'

export const useAuthStore = defineStore('auth', () => {
  const stored = getStoredAuthSession()

  const accessToken = ref(stored?.accessToken ?? '')
  const refreshToken = ref(stored?.refreshToken ?? '')
  const expiresAt = ref(stored?.expiresAt ?? '')
  const user = ref<AuthUser | null>(stored?.user ?? null)
  const activeTenant = ref<Tenant | null>(stored?.activeTenant ?? null)
  const tenants = ref<Tenant[]>([])
  const loading = ref(false)

  const isLogin = computed(() => Boolean(accessToken.value && refreshToken.value && user.value))
  const permissions = computed(() => user.value?.permissions ?? [])

  function setSession(session: AuthSession) {
    accessToken.value = session.accessToken
    refreshToken.value = session.refreshToken
    expiresAt.value = session.expiresAt
    user.value = session.user
    activeTenant.value = session.activeTenant ?? activeTenant.value

    setStoredAuthSession({
      accessToken: accessToken.value,
      refreshToken: refreshToken.value,
      expiresAt: expiresAt.value,
      user: session.user,
      activeTenant: activeTenant.value ?? undefined,
    })
  }

  function clearSession() {
    accessToken.value = ''
    refreshToken.value = ''
    expiresAt.value = ''
    user.value = null
    activeTenant.value = null
    tenants.value = []
    clearStoredAuthSession()
  }

  async function login(username: string, password: string) {
    loading.value = true
    try {
      const response = await authApi.login(username, password)
      setSession(response)
      await loadMe()
      await loadTenants()

      if (tenants.value.length > 0) {
        await switchTenant(tenants.value[0].id)
      }
    }
    finally {
      loading.value = false
    }
  }

  async function loadMe() {
    if (!accessToken.value)
      return

    const me = await authApi.me()
    user.value = me

    setSession({
      accessToken: accessToken.value,
      refreshToken: refreshToken.value,
      expiresAt: expiresAt.value,
      user: me,
      activeTenant: activeTenant.value ?? undefined,
    })
  }

  async function loadTenants() {
    if (!accessToken.value)
      return

    tenants.value = await tenantApi.my()
  }

  async function switchTenant(tenantId: string) {
    const response = await tenantApi.switch(tenantId)
    activeTenant.value = response.activeTenant
    setSession({
      accessToken: response.accessToken,
      refreshToken: refreshToken.value,
      expiresAt: response.expiresAt,
      user: user.value!,
      activeTenant: response.activeTenant,
    })
  }

  async function reconcileTenants() {
    await loadTenants()

    const nextTenantId = resolveTenantSelection(activeTenant.value, tenants.value)
    if (nextTenantId === undefined) {
      return
    }

    if (nextTenantId !== null) {
      await switchTenant(nextTenantId)
      return
    }

    activeTenant.value = null
    if (user.value) {
      setStoredAuthSession({
        accessToken: accessToken.value,
        refreshToken: refreshToken.value,
        expiresAt: expiresAt.value,
        user: user.value,
      })
    }
  }

  async function logout() {
    const token = refreshToken.value

    try {
      if (accessToken.value && token) {
        await authApi.logout(token)
      }
    }
    finally {
      clearSession()
    }
  }

  function hasPermission(permission: string) {
    return permissions.value.includes(permission)
  }

  return {
    accessToken,
    refreshToken,
    expiresAt,
    user,
    activeTenant,
    tenants,
    loading,
    isLogin,
    permissions,
    login,
    logout,
    loadMe,
    loadTenants,
    switchTenant,
    reconcileTenants,
    hasPermission,
    clearSession,
  }
})
