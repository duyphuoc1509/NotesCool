import api from './api'

export interface AuthTokens {
  accessToken: string
  refreshToken: string
}

export interface AuthUser {
  id?: string
  email: string
  fullName?: string
}

export interface AuthResponse extends AuthTokens {
  user?: AuthUser
}

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  email: string
  password: string
  fullName: string
}

export interface RefreshPayload {
  refreshToken: string
}

export interface ApiErrorResponse {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export const authService = {
  async register(payload: RegisterPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/register', payload)
    return data
  },

  async login(payload: LoginPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/login', payload)
    return data
  },

  async logout(refreshToken?: string) {
    await api.post('/api/auth/logout', refreshToken ? { refreshToken } : {})
  },

  async refresh(payload: RefreshPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/refresh-token', payload)
    return data
  },
}
