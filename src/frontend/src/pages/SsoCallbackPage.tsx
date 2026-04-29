import { useEffect, useRef } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { authService, type SsoCallbackPayload } from '../services/auth'
import { useAuth } from '../hooks/useAuth'

export function SsoCallbackPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { completeSsoLogin } = useAuth()
  const hasAttempted = useRef(false)

  useEffect(() => {
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
    const code = params.get('code')
    const state = params.get('state')
    
    if (error || !provider || !code || !state) {
      navigate('/login?error=sso_failed', { replace: true })
      return
    }

    if (hasAttempted.current) {
      return
    }
    hasAttempted.current = true

    const payload: SsoCallbackPayload = {
      provider,
      code,
      state,
      email: email || undefined,
      providerUserId: params.get('providerUserId') || undefined,
      displayName: displayName || undefined,
    }

    const performCallback = async () => {
      try {
        const response = await authService.ssoCallback(payload)
        completeSsoLogin(response, '/')
      } catch (err) {
        navigate('/login?error=sso_failed', { replace: true })
      }
    }

    performCallback()
  }, [location.search, navigate, completeSsoLogin])

  return (
    <div className="flex h-screen w-full items-center justify-center bg-gray-50">
      <div className="flex flex-col items-center space-y-4">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"></div>
        <p className="text-sm font-medium text-gray-600">Completing sign in...</p>
      </div>
    </div>
  )
}
