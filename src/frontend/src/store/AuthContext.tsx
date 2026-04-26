/* eslint-disable react-refresh/only-export-components */
import { createContext, useState } from 'react'
import type { ReactNode } from 'react'
import { getStoredSession, storeSession, clearStoredSession } from '../services/auth'
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

  const login = (newSession: AuthSession) => {
    storeSession(newSession)
    setSession(newSession)
  }

  const logout = () => {
    clearStoredSession()
    setSession(null)
    window.location.href = '/login'
  }

  return (
    <AuthContext.Provider
      value={{
        session,
        isAuthenticated: !!session?.accessToken,
        login,
        logout,
        isLoading: false,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
