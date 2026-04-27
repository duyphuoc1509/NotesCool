/* eslint-disable react-refresh/only-export-components */
import { createContext, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { clearStoredSession, getStoredSession, storeSession } from '../services/auth'
import type { AuthSession } from '../services/auth'

export interface AuthContextType {
  session: AuthSession | null
  isAuthenticated: boolean
  login: (session: AuthSession) => void
  logout: () => void
  isLoading: boolean
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(() => getStoredSession())

  const login = (nextSession: AuthSession) => {
    storeSession(nextSession)
    setSession(nextSession)
  }

  const logout = () => {
    clearStoredSession()
    setSession(null)
    window.location.assign('/login')
  }

  const value = useMemo(
    () => ({
      session,
      isAuthenticated: !!session?.accessToken,
      login,
      logout,
      isLoading: false,
    }),
    [session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
