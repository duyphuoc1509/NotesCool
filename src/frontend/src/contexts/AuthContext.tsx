import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AuthContext } from './auth-context'
import {
  authService,
  type AuthTokens,
  type AuthUser,
  type LoginPayload,
  type RegisterPayload,
} from '../services/auth'
import { AUTH_STORAGE_KEY } from '../constants/auth'

export interface AuthState {
  user: AuthUser | null
  tokens: AuthTokens | null
  isAuthenticated: boolean
  isLoading: boolean
}

export interface AuthContextValue extends AuthState {
  login: (payload: LoginPayload, redirectTo?: string) => Promise<void>
  register: (payload: RegisterPayload, redirectTo?: string) => Promise<void>
  logout: () => Promise<void>
}

interface PersistedAuth {
  tokens: AuthTokens
  user?: AuthUser
}

function loadPersistedAuth(): PersistedAuth | null {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY)
    if (!raw) return null
    return JSON.parse(raw) as PersistedAuth
  } catch {
    return null
  }
}

function persistAuth(auth: PersistedAuth | null) {
  if (auth) {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth))
  } else {
    localStorage.removeItem(AUTH_STORAGE_KEY)
    // Legacy key cleanup
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()

  const [state, setState] = useState<AuthState>(() => {
    const persisted = loadPersistedAuth()
    return {
      tokens: persisted?.tokens ?? null,
      user: persisted?.user ?? null,
      isAuthenticated: !!persisted?.tokens?.accessToken,
      isLoading: false,
    }
  })

  // Keep legacy token key in sync for backward-compat with existing axios interceptor
  useEffect(() => {
    if (state.tokens?.accessToken) {
      localStorage.setItem('token', state.tokens.accessToken)
    } else {
      localStorage.removeItem('token')
    }
  }, [state.tokens?.accessToken])

  const login = useCallback(
    async (payload: LoginPayload, redirectTo = '/') => {
      setState((s) => ({ ...s, isLoading: true }))
      const response = await authService.login(payload)
      const tokens: AuthTokens = {
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
      }
      const user = response.user ?? { email: payload.email }
      persistAuth({ tokens, user })
      setState({ tokens, user, isAuthenticated: true, isLoading: false })
      navigate(redirectTo, { replace: true })
    },
    [navigate],
  )

  const register = useCallback(
    async (payload: RegisterPayload, redirectTo = '/') => {
      setState((s) => ({ ...s, isLoading: true }))
      const response = await authService.register(payload)
      const tokens: AuthTokens = {
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
      }
      const user = response.user ?? { email: payload.email, fullName: payload.fullName }
      persistAuth({ tokens, user })
      setState({ tokens, user, isAuthenticated: true, isLoading: false })
      navigate(redirectTo, { replace: true })
    },
    [navigate],
  )

  const logout = useCallback(async () => {
    setState((s) => ({ ...s, isLoading: true }))
    try {
      const raw = localStorage.getItem(AUTH_STORAGE_KEY)
      const refreshToken = raw ? (JSON.parse(raw) as PersistedAuth).tokens?.refreshToken : undefined
      await authService.logout(refreshToken)
    } catch {
      // ignore server errors; proceed with local logout
    }
    persistAuth(null)
    setState({ tokens: null, user: null, isAuthenticated: false, isLoading: false })
    navigate('/login', { replace: true })
  }, [navigate])

  const value = useMemo<AuthContextValue>(
    () => ({ ...state, login, register, logout }),
    [state, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

