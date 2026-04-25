import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { buildSsoStartUrl, persistAuthSession, type AuthSession } from './auth'

describe('auth service', () => {
  beforeEach(() => {
    const store = new Map<string, string>()

    vi.stubGlobal('localStorage', {
      clear: () => store.clear(),
      getItem: (key: string) => store.get(key) ?? null,
      setItem: (key: string, value: string) => store.set(key, value),
      removeItem: (key: string) => store.delete(key),
    })
    vi.stubGlobal('location', { origin: 'https://app.notescool.test', href: '' })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('stores the auth token and user profile after successful login', () => {
    const session: AuthSession = {
      token: 'jwt-token',
      user: { id: 'user-1', email: 'user@example.com', name: 'User One' },
    }

    persistAuthSession(session)

    expect(localStorage.getItem('token')).toBe('jwt-token')
    expect(localStorage.getItem('auth:user')).toBe(JSON.stringify(session.user))
  })

  it('builds provider start URLs with a redirect target', () => {
    expect(buildSsoStartUrl('google', '/notes')).toBe(
      'http://localhost:5000/auth/sso/google/start?redirect=%2Fnotes',
    )
  })
})
