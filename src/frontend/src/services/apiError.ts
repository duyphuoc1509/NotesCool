import { AxiosError } from 'axios'

export type StandardError = {
  status?: number
  code: string
  message: string
  retryable: boolean
}

const ERROR_MAP: Record<number, string> = {
  400: 'The request is invalid. Please check your input.',
  401: 'Your session has expired. Please sign in again.',
  403: 'You do not have permission to perform this action.',
  404: 'The requested resource was not found.',
  409: 'This action conflicts with the current data. Please refresh and try again.',
  500: 'Something went wrong on our side. Please try again later.',
}

export function normalizeApiError(error: unknown): StandardError {
  if (error instanceof AxiosError) {
    const status = error.response?.status
    const data = error.response?.data as { message?: string; code?: string } | undefined

    if (error.code === AxiosError.ERR_NETWORK || error.message === 'Network Error') {
      return {
        code: 'network_error',
        message: 'Network error. Please check your connection and try again.',
        retryable: true,
      }
    }

    if (status) {
      return {
        status,
        code: data?.code || (status >= 500 ? 'server_error' : 'request_error'),
        message: data?.message || ERROR_MAP[status] || 'An unexpected error occurred.',
        retryable: status >= 500,
      }
    }
  }

  return {
    code: 'unknown_error',
    message: error instanceof Error ? error.message : 'An unknown error occurred.',
    retryable: false,
  }
}

export function shouldRetryRequest(error: unknown, retryCount: number, method: string): boolean {
  if (retryCount >= 2) return false

  const normalized = normalizeApiError(error)
  if (!normalized.retryable) return false

  // Only retry idempotent methods or network errors
  const isIdempotent = ['get', 'put', 'delete', 'patch', 'head', 'options'].includes(method.toLowerCase())
  const isNetworkError = normalized.code === 'network_error'

  return isIdempotent || isNetworkError
}

export function getRetryDelay(retryCount: number): number {
  return Math.min(500 * Math.pow(2, retryCount), 4000)
}
