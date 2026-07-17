import type { Router } from 'vue-router'

import { storeToRefs } from 'pinia'

import pinia from '@/plugins/pinia/setup'
import { useAuthStore } from '@/stores/auth'

export function setupAuthGuard(router: Router) {
  router.beforeEach((to, from) => {
    const authStore = useAuthStore(pinia)
    const { isLogin } = storeToRefs(authStore)

    const authPaths = ['/auth/sign-in', '/auth/sign-up', '/auth/forgot-password', '/auth/otp']
    const publicPrefixes = ['/auth', '/errors', '/marketing']
    const isAuthPage = authPaths.includes(to.path)
    const isFromAuthPage = authPaths.includes(from.path)
    const isPublicPage = publicPrefixes.some(path => to.path.startsWith(path))

    // If logged in, redirect from auth pages to the previous non-auth page (if valid), otherwise redirect to home
    if (isLogin.value && isAuthPage) {
      // Check if from route is valid (has path, is different from target, and is not an auth page)
      if (from.path && from.path !== to.path && !isFromAuthPage) {
        return from
      }
      // Fallback: redirect to home on first visit or invalid/unsafe source (including other auth pages)
      return { path: '/' }
    }

    // If page requires auth but user is not logged in, redirect to sign-in page
    if (!isLogin.value && !isPublicPage) {
      return {
        name: '/auth/sign-in',
        query: { redirect: to.fullPath },
      }
    }

    const requiredPermission = to.meta.requiredPermission
    if (isLogin.value
      && requiredPermission
      && !authStore.hasPermission(requiredPermission)) {
      return { path: '/errors/403' }
    }
  })
}
