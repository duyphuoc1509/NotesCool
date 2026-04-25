import { API_BASE_URL } from '../constants/env'
import api from './api'

export type AuthUser = {
  id?: string
  email: string
  name?: string
}

export type AuthSession = {
  token: string
  user: AuthUser
}

export type LoginCredentials = {
  email: string
  password: string
}

const AUTH_USER_KEY = 'auth:user'

export function persistAuthSession(session: AuthSession) {
  localStorage.setItem('token', session.token)
  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(session.user))
}

export function buildSsoStartUrl(provider: string, redirectTo = '/') {
  return `${API_BASE_URL}/auth/sso/${provider}/start?redirect=${encodeURIComponent(redirectTo)}`
}

export async function loginWithPassword(credentials: LoginCredentials) {
  const { data } = await api.post<AuthSession>('/auth/login', credentials)
  persistAuthSession(data)
  return data
}

export function startSsoLogin(provider: string, redirectTo = '/') {
  window.location.href = buildSsoStartUrl(provider, redirectTo)
}
