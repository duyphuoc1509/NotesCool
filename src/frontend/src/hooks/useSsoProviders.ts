import { useCallback, useEffect, useState } from 'react'
import { ssoProvidersService, type SsoProvider } from '../services/ssoProviders'

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
      const result = await ssoProvidersService.getProviders()
      const enabledProviders = result.filter((provider) => provider.enabled)
      setProviders(enabledProviders)
    } catch {
      // If the endpoint is not available (401, 404, 500, network error),
      // gracefully fall back to empty list — the login form is still usable.
      setProviders([])
      setError('Unable to load SSO providers.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchProviders()
  }, [fetchProviders])

  return { providers, isLoading, error }
}
