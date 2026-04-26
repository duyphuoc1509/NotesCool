export type AuthSession = {
  accessToken: string
  refreshToken?: string
  expiresAt?: number
}

const SESSION_STORAGE_KEY = 'notescool.auth.session'
const REFRESH_SKEW_MS = 60_000

function isBrowserStorageAvailable() {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined'
}

export function getStoredSession(): AuthSession | null {
  if (!isBrowserStorageAvailable()) {
    return null
  }

  const value = window.localStorage.getItem(SESSION_STORAGE_KEY)
  if (!value) {
    return null
  }

  try {
    const session = JSON.parse(value) as Partial<AuthSession>
    if (!session.accessToken || typeof session.accessToken !== 'string') {
      clearStoredSession()
      return null
    }

    return {
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
      expiresAt: session.expiresAt,
    }
  } catch {
    clearStoredSession()
    return null
  }
}

export function storeSession(session: AuthSession) {
  if (!isBrowserStorageAvailable()) {
    return
  }

  window.localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session))
}

export function clearStoredSession() {
  if (!isBrowserStorageAvailable()) {
    return
  }

  window.localStorage.removeItem(SESSION_STORAGE_KEY)
  window.localStorage.removeItem('token')
}

export function shouldRefreshSession(session: AuthSession | null) {
  if (!session?.refreshToken || !session.expiresAt) {
    return false
  }

  return Date.now() >= session.expiresAt - REFRESH_SKEW_MS
}

export function getLoginPath() {
  return '/login'
}
