import { useCallback, useEffect, useState } from 'react'
import { settingsService } from '../services/settingsService'
import type { SsoProvider } from '../types'

export interface UseSsoProvidersResult {
  providers: SsoProvider[]
  isLoading: boolean
  error: string | null
}

export function useSsoProviders(): UseSsoProvidersResult {
  const [providers, setProviders] = useState<SsoProvider[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchProviders = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      setProviders(await settingsService.getEnabledSsoProviders())
    } catch {
      setProviders([])
      setError('Unable to load SSO providers.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void fetchProviders()
    }, 0)

    return () => window.clearTimeout(timeoutId)
  }, [fetchProviders])

  return { providers, isLoading, error }
}
