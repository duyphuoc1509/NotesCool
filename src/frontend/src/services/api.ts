import axios from 'axios'
import { API_BASE_URL } from '../constants/env'
import { clearStoredSession, getStoredSession, storeSession } from './auth'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

let isRefreshing = false
let refreshSubscribers: ((token: string) => void)[] = []

function subscribeTokenRefresh(cb: (token: string) => void) {
  refreshSubscribers.push(cb)
}

function onRefreshed(token: string) {
  refreshSubscribers.forEach((cb) => cb(token))
  refreshSubscribers = []
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
  async (error) => {
    const { config, response } = error
    const originalRequest = config

    if (response?.status === 401 && !originalRequest._retry) {
      const session = getStoredSession()
      if (!session?.refreshToken) {
        clearStoredSession()
        return Promise.reject(error)
      }

      if (isRefreshing) {
        return new Promise((resolve) => {
          subscribeTokenRefresh((token: string) => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            resolve(api(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const { data } = await axios.post(`${API_BASE_URL}/api/auth/refresh`, {
          refreshToken: session.refreshToken,
        })

        const newSession = {
          accessToken: data.accessToken,
          refreshToken: data.refreshToken || session.refreshToken,
          expiresAt: data.expiresAt,
        }

        storeSession(newSession)
        onRefreshed(data.accessToken)
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`

        return api(originalRequest)
      } catch (refreshError) {
        clearStoredSession()
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export default api
