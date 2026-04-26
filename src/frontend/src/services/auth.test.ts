import { describe, expect, it, beforeEach, vi } from 'vitest'
import {
  clearStoredSession,
  getStoredSession,
  shouldRefreshSession,
  storeSession,
} from './auth'

beforeEach(() => {
  window.localStorage.clear()
  vi.useRealTimers()
})

describe('auth session storage', () => {
  it('persists and restores an auth session', () => {
    const session = {
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      expiresAt: Date.now() + 120_000,
    }

    storeSession(session)

    expect(getStoredSession()).toEqual(session)
  })

  it('clears the session and legacy token on logout', () => {
    storeSession({ accessToken: 'access-token' })
    window.localStorage.setItem('token', 'legacy-token')

    clearStoredSession()

    expect(getStoredSession()).toBeNull()
    expect(window.localStorage.getItem('token')).toBeNull()
  })

  it('requests refresh only when a refresh token is close to expiry', () => {
    vi.setSystemTime(new Date('2026-01-01T00:00:00.000Z'))

    expect(
      shouldRefreshSession({
        accessToken: 'access-token',
        refreshToken: 'refresh-token',
        expiresAt: Date.now() + 30_000,
      }),
    ).toBe(true)
    expect(
      shouldRefreshSession({
        accessToken: 'access-token',
        refreshToken: 'refresh-token',
        expiresAt: Date.now() + 120_000,
      }),
    ).toBe(false)
    expect(shouldRefreshSession({ accessToken: 'access-token' })).toBe(false)
  })
})
