import api from './api'
import type {
  AuthResponse,
  ChangePasswordRequest,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
} from '../types/auth'

const AUTH_BASE_PATH = '/api/auth'

export const authService = {
  async login(payload: LoginRequest): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>(`${AUTH_BASE_PATH}/login`, payload)
    return data
  },

  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>(`${AUTH_BASE_PATH}/register`, payload)
    return data
  },

  async refreshToken(payload: RefreshTokenRequest): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>(`${AUTH_BASE_PATH}/refresh`, payload)
    return data
  },

  async getProfile(): Promise<AuthResponse['user']> {
    const { data } = await api.get<AuthResponse['user']>(`${AUTH_BASE_PATH}/me`)
    return data
  },

  async logout(): Promise<void> {
    await api.post(`${AUTH_BASE_PATH}/logout`)
  },

  async changePassword(payload: ChangePasswordRequest): Promise<void> {
    await api.post(`${AUTH_BASE_PATH}/change-password`, payload)
  },
}

export default authService
