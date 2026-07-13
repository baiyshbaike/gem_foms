import { storeToRefs } from 'pinia'
import { toast } from 'vue-sonner'

import { useAuthStore } from '@/stores/auth'

export function useAuth() {
  const router = useRouter()
  const authStore = useAuthStore()
  const { loading, isLogin } = storeToRefs(authStore)

  async function logout() {
    await authStore.logout()
    router.push({ path: '/auth/sign-in' })
  }

  function toHome() {
    router.push({ path: '/dashboard' })
  }

  async function login(username: string, password: string) {
    try {
      await authStore.login(username, password)

      const redirect = router.currentRoute.value.query.redirect as string | undefined
      if (!redirect || redirect.startsWith('//')) {
        toHome()
      }
      else {
        router.push(redirect)
      }
    }
    catch {
      toast.error('Login failed', {
        description: 'Username or password is incorrect.',
      })
    }
  }

  return {
    loading,
    isLogin,
    logout,
    login,
  }
}
