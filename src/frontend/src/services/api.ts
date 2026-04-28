import axios from 'axios'
import { API_BASE_URL } from '../constants/env'
import { AUTH_STORAGE_KEY } from '../constants/auth'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: unknown) => void
  reject: (reason?: unknown) => void
}> = []

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })

  failedQueue = []
}

api.interceptors.request.use((config) => {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      const token = parsed.tokens?.accessToken || parsed.accessToken
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    } catch {
      // invalid json
    }
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            return api(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      const raw = localStorage.getItem(AUTH_STORAGE_KEY)
      if (!raw) {
        isRefreshing = false
        return Promise.reject(error)
      }

      try {
        const authData = JSON.parse(raw)
        const refreshToken = authData.tokens?.refreshToken

        if (!refreshToken) {
          throw new Error('No refresh token available')
        }

        const { data } = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
          refreshToken,
        })

        const newAuthData = {
          ...authData,
          tokens: {
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
          },
        }

        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(newAuthData))
        api.defaults.headers.common.Authorization = `Bearer ${data.accessToken}`
        processQueue(null, data.accessToken)

        return api(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError as Error, null)
        localStorage.removeItem(AUTH_STORAGE_KEY)
        // Optionally redirect to login or let the component handle it
        // window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export default api
