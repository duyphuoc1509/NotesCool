import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AuthContext } from './auth-context'
import {
  authService,
  getStoredSession,
  storeSession,
  clearStoredSession,
  type AuthTokens,
  type AuthUser,
  type LoginPayload,
  type RegisterPayload,
  type AuthSession,
  type AuthResponse,
} from '../services/auth'

export interface AuthState {
  user: AuthUser | null
  tokens: AuthTokens | null
  isAuthenticated: boolean
  isLoading: boolean
}

export interface AuthContextValue extends AuthState {
  login: (payload: LoginPayload, redirectTo?: string) => Promise<void>
  register: (payload: RegisterPayload, redirectTo?: string) => Promise<void>
  completeSsoLogin: (response: AuthResponse, redirectTo?: string) => void
  logout: () => Promise<void>
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()

  const [state, setState] = useState<AuthState>(() => {
    const session = getStoredSession()
    return {
      tokens: session
        ? { accessToken: session.accessToken, refreshToken: session.refreshToken }
        : null,
      user: session?.user ?? null,
      isAuthenticated: !!session?.accessToken,
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
      const session: AuthSession = {
        accessToken: response.accessToken,
        refreshToken: response.refreshToken || '',
        user: response.user ?? { email: payload.email },
      }
      storeSession(session)
      setState({
        tokens: { accessToken: session.accessToken, refreshToken: session.refreshToken },
        user: session.user ?? null,
        isAuthenticated: true,
        isLoading: false,
      })
      navigate(redirectTo, { replace: true })
    },
    [navigate],
  )

  const register = useCallback(
    async (payload: RegisterPayload, redirectTo = '/') => {
      setState((s) => ({ ...s, isLoading: true }))
      const response = await authService.register(payload)
      const session: AuthSession = {
        accessToken: response.accessToken,
        refreshToken: response.refreshToken || '',
        user: response.user ?? { email: payload.email, fullName: payload.fullName },
      }
      storeSession(session)
      setState({
        tokens: { accessToken: session.accessToken, refreshToken: session.refreshToken },
        user: session.user ?? null,
        isAuthenticated: true,
        isLoading: false,
      })
      navigate(redirectTo, { replace: true })
    },
    [navigate],
  )

  const completeSsoLogin = useCallback((response: AuthResponse, redirectTo = '/') => {
    const session: AuthSession = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken || '',
      user: response.user,
    }
    storeSession(session)
    setState({
      user: session.user ?? null,
      tokens: { accessToken: session.accessToken, refreshToken: session.refreshToken },
      isAuthenticated: true,
      isLoading: false,
    })
    navigate(redirectTo, { replace: true })
  }, [navigate])

  const logout = useCallback(async () => {
    setState((s) => ({ ...s, isLoading: true }))
    try {
      const session = getStoredSession()
      await authService.logout(session?.refreshToken)
    } catch {
      // ignore server errors; proceed with local logout
    }
    clearStoredSession()
    setState({ tokens: null, user: null, isAuthenticated: false, isLoading: false })
    navigate('/login', { replace: true })
  }, [navigate])

  const value = useMemo<AuthContextValue>(
    () => ({ ...state, login, register, completeSsoLogin, logout }),
    [state, login, register, completeSsoLogin, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
