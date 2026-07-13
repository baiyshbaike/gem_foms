import type { AuthSession } from './types/dialysis'

const STORAGE_KEY = 'dialysis.auth'

export function getStoredAuthSession(): AuthSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw)
    return null

  try {
    return JSON.parse(raw) as AuthSession
  }
  catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function setStoredAuthSession(session: AuthSession) {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
  window.dispatchEvent(new CustomEvent('dialysis-auth-changed'))
}

export function clearStoredAuthSession() {
  sessionStorage.removeItem(STORAGE_KEY)
  window.dispatchEvent(new CustomEvent('dialysis-auth-changed'))
}
