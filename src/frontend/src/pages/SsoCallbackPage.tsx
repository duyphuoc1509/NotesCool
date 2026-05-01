import { useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { authService, type SsoCallbackPayload, getStoredSession } from '../services/auth'
import { useAuth } from '../hooks/useAuth'

/**
 * Module-level map to track in-flight or completed session code exchanges.
 *
 * React 18 StrictMode double-mounts components, causing useEffect to run
 * twice. Since the backend's session codes are one-time-use (TryRemove),
 * a second exchange call will always fail with 400. We cache the successful
 * response so the second mount can pick it up without a network call.
 */
const sessionCodeCache = new Map<string, ReturnType<typeof authService.exchangeSsoSession>>()

export function SsoCallbackPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { completeSsoLogin, isAuthenticated } = useAuth()

  useEffect(() => {
    // If user is already authenticated (e.g. from a previous successful call
    // during StrictMode double-mount), just redirect to home.
    if (isAuthenticated) {
      navigate('/', { replace: true })
      return
    }

    const params = new URLSearchParams(location.search)
    const hashParams = new URLSearchParams(location.hash.startsWith('#') ? location.hash.slice(1) : location.hash)
    const error = params.get('error')
    const accessToken = hashParams.get('accessToken')
    const refreshToken = hashParams.get('refreshToken')
    const tokenType = hashParams.get('tokenType')
    const expiresIn = hashParams.get('expiresIn')
    const email = hashParams.get('email')
    const displayName = hashParams.get('displayName')
    const userId = hashParams.get('userId')

    // Path 1: Tokens are embedded directly in URL hash (e.g. from some SSO providers)
    if (!error && accessToken) {
      completeSsoLogin(
        {
          accessToken,
          refreshToken: refreshToken ?? undefined,
          tokenType: tokenType ?? undefined,
          expiresIn: expiresIn ? Number(expiresIn) : undefined,
          user: {
            userId: userId ?? undefined,
            email: email ?? '',
            displayName: displayName ?? undefined,
          },
        },
        '/',
      )
      return
    }

    const provider = params.get('provider')
    const sessionCode = params.get('sessionCode')
    const code = params.get('code')
    const state = params.get('state')

    if (error || !provider || (!sessionCode && (!code || !state))) {
      navigate('/login?error=sso_failed', { replace: true })
      return
    }

    let cancelled = false

    const performCallback = async () => {
      try {
        let response

        if (sessionCode) {
          // De-duplicate session code exchange across StrictMode double-mounts.
          // The first call creates the promise and caches it; the second mount
          // awaits the same promise instead of firing a new HTTP request.
          if (!sessionCodeCache.has(sessionCode)) {
            sessionCodeCache.set(
              sessionCode,
              authService.exchangeSsoSession(sessionCode),
            )
          }
          response = await sessionCodeCache.get(sessionCode)!
        } else {
          response = await authService.ssoCallback({
            provider,
            code: code!,
            state: state!,
            email: email || undefined,
            providerUserId: params.get('providerUserId') || undefined,
            displayName: displayName || undefined,
          } satisfies SsoCallbackPayload)
        }

        // Don't update state if this effect instance was cleaned up
        if (cancelled) return

        completeSsoLogin(response, '/')
      } catch (err) {
        if (cancelled) return

        // If auth was already stored by a previous mount cycle, redirect to home
        const stored = getStoredSession()
        if (stored?.accessToken) {
          navigate('/', { replace: true })
          return
        }

        navigate('/login?error=sso_failed', { replace: true })
      }
    }

    performCallback()

    return () => {
      cancelled = true
    }
  }, [location.search, location.hash, navigate, completeSsoLogin, isAuthenticated])

  return (
    <div className="flex h-screen w-full items-center justify-center bg-gray-50">
      <div className="flex flex-col items-center space-y-4">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"></div>
        <p className="text-sm font-medium text-gray-600">Completing sign in...</p>
      </div>
    </div>
  )
}
