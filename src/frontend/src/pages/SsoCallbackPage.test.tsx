import { act } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Routes, Route, useLocation } from 'react-router-dom'
import { SsoCallbackPage } from './SsoCallbackPage'
import { AuthProvider } from '../contexts/AuthContext'
import { authService } from '../services/auth'

// @ts-expect-error IS_REACT_ACT_ENVIRONMENT
globalThis.IS_REACT_ACT_ENVIRONMENT = true

vi.mock('../services/auth', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../services/auth')>()
  return {
    ...actual,
    authService: {
      ...actual.authService,
      ssoCallback: vi.fn(),
    },
  }
})

function TestLocation() {
  const location = useLocation()
  return <div data-testid="location">{location.pathname}{location.search}</div>
}

describe('SsoCallbackPage', () => {
  let container: HTMLDivElement
  let root: Root

  beforeEach(() => {
    container = document.createElement('div')
    document.body.appendChild(container)
    root = createRoot(container)
    vi.clearAllMocks()
    localStorage.clear()
  })

  afterEach(() => {
    act(() => root.unmount())
    container.remove()
  })

  it('redirects to /login?error=sso_failed when callback fails', async () => {
    vi.mocked(authService.ssoCallback).mockRejectedValueOnce(new Error('Failed'))

    await act(async () => {
      root.render(
        <MemoryRouter initialEntries={['/auth/sso/callback?provider=google&code=123&state=abc']}>
          <AuthProvider>
            <Routes>
              <Route path="/auth/sso/callback" element={<SsoCallbackPage />} />
              <Route path="/login" element={<TestLocation />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      )
    })

    await vi.waitFor(() => {
      const location = container.querySelector('[data-testid="location"]')
      expect(location?.textContent).toBe('/login?error=sso_failed')
    })
  })

  it('redirects to / and stores auth on success', async () => {
    vi.mocked(authService.ssoCallback).mockResolvedValueOnce({
      accessToken: 'access-123',
      refreshToken: 'refresh-123',
      expiresIn: 3600,
      tokenType: 'Bearer',
      user: {
        userId: 'u-1',
        email: 'test@example.com',
        displayName: 'Test User',
      }
    })

    await act(async () => {
      root.render(
        <MemoryRouter initialEntries={['/auth/sso/callback?provider=google&code=123&state=abc']}>
          <AuthProvider>
            <Routes>
              <Route path="/auth/sso/callback" element={<SsoCallbackPage />} />
              <Route path="/" element={<TestLocation />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      )
    })

    await vi.waitFor(() => {
      const location = container.querySelector('[data-testid="location"]')
      expect(location?.textContent).toBe('/')
    })

    expect(authService.ssoCallback).toHaveBeenCalledWith({
      provider: 'google',
      code: '123',
      state: 'abc'
    })
  })
})
