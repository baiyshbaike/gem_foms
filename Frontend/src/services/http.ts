import axios from 'axios'

import { API_BASE_URL, API_TIMEOUT } from '@/constants/app-config'
import {
  clearStoredAuthSession,
  getStoredAuthSession,
  setStoredAuthSession,
} from '@/services/auth-session'

import type { LoginResponse, SwitchTenantResponse } from './types/dialysis'

let refreshPromise: Promise<LoginResponse> | null = null

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
})

apiClient.interceptors.request.use((config) => {
  const session = getStoredAuthSession()

  if (session?.accessToken) {
    config.headers.Authorization = `Bearer ${session.accessToken}`
  }

  return config
})

apiClient.interceptors.response.use(
  response => response,
  async (error) => {
    const session = getStoredAuthSession()
    const originalRequest = error.config
    const status = error.response?.status
    const url = String(originalRequest?.url ?? '')

    if (
      status !== 401
      || !session?.refreshToken
      || originalRequest?._retry
      || url.includes('/auth/login')
      || url.includes('/auth/refresh')
    ) {
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      refreshPromise ??= axios
        .post<LoginResponse>(`${API_BASE_URL}/auth/refresh`, {
          refreshToken: session.refreshToken,
        })
        .then(response => response.data)
        .finally(() => {
          refreshPromise = null
        })

      const refreshed = await refreshPromise
      let nextSession = {
        ...refreshed,
        activeTenant: session.activeTenant,
      }

      if (session.activeTenant?.id) {
        const switched = await axios.post<SwitchTenantResponse>(
          `${API_BASE_URL}/tenants/${session.activeTenant.id}/switch`,
          {},
          {
            headers: {
              Authorization: `Bearer ${refreshed.accessToken}`,
            },
          },
        )

        nextSession = {
          ...refreshed,
          accessToken: switched.data.accessToken,
          expiresAt: switched.data.expiresAt,
          activeTenant: switched.data.activeTenant,
        }
      }

      setStoredAuthSession(nextSession)

      originalRequest.headers.Authorization = `Bearer ${nextSession.accessToken}`
      return apiClient(originalRequest)
    }
    catch (refreshError) {
      clearStoredAuthSession()
      return Promise.reject(refreshError)
    }
  },
)

export function unwrapData<T>(request: Promise<{ data: T }>): Promise<T> {
  return request.then(response => response.data)
}
