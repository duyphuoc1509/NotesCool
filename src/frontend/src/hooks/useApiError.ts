import { useEffect } from 'react'
import type { StandardError } from '../services/apiError'

type ErrorHandler = (error: StandardError) => void

/**
 * Subscribe to global API errors dispatched from the axios interceptor.
 * Use this at the top of your component tree to show centralized toast notifications.
 *
 * @example
 * useApiError((err) => toast.error(err.message))
 */
export function useApiError(onError: ErrorHandler) {
  useEffect(() => {
    const handler = (event: Event) => {
      const detail = (event as CustomEvent<StandardError>).detail
      onError(detail)
    }

    window.addEventListener('api:error', handler)
    return () => window.removeEventListener('api:error', handler)
  }, [onError])
}
