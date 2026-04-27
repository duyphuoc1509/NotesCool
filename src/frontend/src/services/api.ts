import axios from 'axios'
import type { AxiosError, AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios'
import { API_BASE_URL } from '../constants/env'
import { clearStoredSession, getStoredSession, storeSession } from './auth'
import { getRetryDelay, normalizeApiError, shouldRetryRequest } from './apiError'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

type RetriableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
  _retryCount?: number
  skipAuthRefresh?: boolean
  skipGlobalErrorHandler?: boolean
}

type RefreshResponse = {
  accessToken: string
  refreshToken?: string
  expiresAt?: number
}

let refreshPromise: Promise<string> | null = null

const notifyGlobalError = (error: unknown) => {
  if (typeof window === 'undefined') return

  window.dispatchEvent(
    new CustomEvent('api:error', {
      detail: normalizeApiError(error),
    }),
  )
}

const redirectToLogin = () => {
  if (typeof window === 'undefined') return
  if (window.location.pathname !== '/login') {
    window.location.assign('/login')
  }
}

async function refreshAccessToken() {
  if (refreshPromise) {
    return refreshPromise
  }

  const session = getStoredSession()
  if (!session?.refreshToken) {
    clearStoredSession()
    throw new Error('Missing refresh token')
  }

  refreshPromise = axios
    .post<RefreshResponse>(`${API_BASE_URL}/api/auth/refresh`, {
      refreshToken: session.refreshToken,
    })
    .then(({ data }) => {
      const nextSession = {
        accessToken: data.accessToken,
        refreshToken: data.refreshToken || session.refreshToken,
        expiresAt: data.expiresAt,
      }

      storeSession(nextSession)
      return data.accessToken
    })
    .catch((error) => {
      clearStoredSession()
      redirectToLogin()
      throw error
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

api.interceptors.request.use((config) => {
  const session = getStoredSession()
  if (session?.accessToken) {
    config.headers.Authorization = `Bearer ${session.accessToken}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined

    if (!originalRequest) {
      notifyGlobalError(error)
      return Promise.reject(error)
    }

    if (error.response?.status === 401 && !originalRequest._retry && !originalRequest.skipAuthRefresh) {
      originalRequest._retry = true

      try {
        const accessToken = await refreshAccessToken()
        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return api(originalRequest)
      } catch (refreshError) {
        notifyGlobalError(refreshError)
        return Promise.reject(refreshError)
      }
    }

    const retryCount = originalRequest._retryCount ?? 0
    if (shouldRetryRequest(error, retryCount, originalRequest.method ?? 'get')) {
      originalRequest._retryCount = retryCount + 1
      await new Promise((resolve) => window.setTimeout(resolve, getRetryDelay(retryCount)))
      return api(originalRequest)
    }

    if (!originalRequest.skipGlobalErrorHandler) {
      notifyGlobalError(error)
    }

    return Promise.reject(error)
  },
)

export type ApiRequestConfig = AxiosRequestConfig & {
  skipAuthRefresh?: boolean
  skipGlobalErrorHandler?: boolean
}

export { normalizeApiError }
export default api
