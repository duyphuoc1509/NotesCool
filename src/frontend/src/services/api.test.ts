import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import axios from 'axios'
import { AUTH_STORAGE_KEY } from '../constants/auth'
import { API_BASE_URL } from '../constants/env'

// Spy on window.location.assign
const assignMock = vi.fn()
Object.defineProperty(window, 'location', {
  writable: true,
  value: { ...window.location, assign: assignMock },
})

describe('api interceptor — refresh token flow', () => {
  let apiModule: typeof import('./api')

  beforeEach(async () => {
    localStorage.clear()
    assignMock.mockClear()
    vi.resetModules()
    apiModule = await import('./api')
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  const setSession = (data: {
    accessToken: string
    refreshToken: string
    expiresAt?: number
  }) => {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ tokens: data }))
  }

  it('AC-001: calls /api/auth/refresh (not /api/auth/refresh-token) when 401 received', async () => {
    setSession({ accessToken: 'expired', refreshToken: 'rt_valid' })

    const postSpy = vi.spyOn(axios, 'post').mockResolvedValueOnce({
      data: {
        accessToken: 'new_access',
        refreshToken: 'new_refresh',
        accessTokenExpiresInSeconds: 3600,
        accessTokenExpiresAtUtc: new Date(Date.now() + 3600_000).toISOString(),
      },
    })

    // Manually trigger the interceptor by calling the api with a 401 response
    const api = apiModule.default
    const axiosError = Object.assign(new Error('Unauthorized'), {
      response: { status: 401, headers: {}, data: {} },
      config: {
        headers: { Authorization: 'Bearer expired' },
        _retry: false,
        url: '/api/notes',
        method: 'get',
        baseURL: API_BASE_URL,
      },
      isAxiosError: true,
    })

    // The interceptor runs on errors coming from the api instance.
    // We simulate by making the API call that returns 401 then refresh.
    // Instead test that the interceptor uses the correct endpoint.
    // Patch-test: confirm the correct URL appears in the mocked POST.

    // Trigger via api instance to invoke the interceptor:
    const adapterMock = vi
      .fn()
      .mockRejectedValueOnce(axiosError)
      .mockResolvedValueOnce({
        data: { result: 'ok' },
        status: 200,
        config: axiosError.config,
      })

    api.defaults.adapter = adapterMock

    await api.get('/api/notes').catch(() => {})

    expect(postSpy).toHaveBeenCalledTimes(1)
    const [url] = postSpy.mock.calls[0]
    expect(url).toBe(`${API_BASE_URL}/api/auth/refresh`)
    expect(url).not.toContain('refresh-token')
  })

  it('AC-002: on refresh failure, calls clearStoredSession and redirects to /login', async () => {
    setSession({ accessToken: 'expired', refreshToken: 'rt_expired' })

    vi.spyOn(axios, 'post').mockRejectedValueOnce(
      Object.assign(new Error('Unauthorized'), {
        response: { status: 401, headers: {}, data: {} },
        isAxiosError: true,
      }),
    )

    const api = apiModule.default
    const axiosError = Object.assign(new Error('Unauthorized'), {
      response: { status: 401, headers: {}, data: {} },
      config: {
        headers: { Authorization: 'Bearer expired' },
        _retry: false,
        url: '/api/notes',
        method: 'get',
        baseURL: API_BASE_URL,
      },
      isAxiosError: true,
    })

    const adapterMock = vi.fn().mockRejectedValueOnce(axiosError)
    api.defaults.adapter = adapterMock

    await api.get('/api/notes').catch(() => {})

    // Session should be cleared
    expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull()
    // User redirected to /login
    expect(assignMock).toHaveBeenCalledWith('/login')
  })

  it('AC-003: multiple concurrent 401s should trigger only one refresh call', async () => {
    setSession({ accessToken: 'expired', refreshToken: 'rt_valid' })

    let resolveRefresh: (v: unknown) => void
    const refreshPromise = new Promise((resolve) => {
      resolveRefresh = resolve
    })

    const postSpy = vi.spyOn(axios, 'post').mockReturnValueOnce(
      refreshPromise.then(() => ({
        data: {
          accessToken: 'new_access',
          refreshToken: 'new_refresh',
          accessTokenExpiresInSeconds: 3600,
          accessTokenExpiresAtUtc: new Date(Date.now() + 3600_000).toISOString(),
        },
      })) as ReturnType<typeof axios.post>,
    )

    const api = apiModule.default

    const make401Error = () =>
      Object.assign(new Error('Unauthorized'), {
        response: { status: 401, headers: {}, data: {} },
        config: {
          headers: { Authorization: 'Bearer expired' },
          _retry: false,
          url: '/api/notes',
          method: 'get',
          baseURL: API_BASE_URL,
        },
        isAxiosError: true,
      })

    const adapterMock = vi
      .fn()
      .mockRejectedValueOnce(make401Error())
      .mockRejectedValueOnce(make401Error())
      .mockResolvedValue({
        data: { result: 'ok' },
        status: 200,
        config: make401Error().config,
      })

    api.defaults.adapter = adapterMock

    const p1 = api.get('/api/notes1').catch(() => {})
    const p2 = api.get('/api/notes2').catch(() => {})

    // Resolve the refresh
    resolveRefresh!(null)
    await Promise.all([p1, p2])

    expect(postSpy).toHaveBeenCalledTimes(1)
  })

  it('AC-004: saves expiresAt in session after successful refresh', async () => {
    setSession({ accessToken: 'expired', refreshToken: 'rt_valid' })
    const futureMs = Date.now() + 3600_000
    const futureIso = new Date(futureMs).toISOString()

    vi.spyOn(axios, 'post').mockResolvedValueOnce({
      data: {
        accessToken: 'new_access',
        refreshToken: 'new_refresh',
        accessTokenExpiresInSeconds: 3600,
        accessTokenExpiresAtUtc: futureIso,
      },
    })

    const api = apiModule.default
    const axiosError = Object.assign(new Error('Unauthorized'), {
      response: { status: 401, headers: {}, data: {} },
      config: {
        headers: { Authorization: 'Bearer expired' },
        _retry: false,
        url: '/api/notes',
        method: 'get',
        baseURL: API_BASE_URL,
      },
      isAxiosError: true,
    })

    const adapterMock = vi
      .fn()
      .mockRejectedValueOnce(axiosError)
      .mockResolvedValueOnce({
        data: { result: 'ok' },
        status: 200,
        config: axiosError.config,
      })

    api.defaults.adapter = adapterMock
    await api.get('/api/notes').catch(() => {})

    const stored = JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY) ?? '{}')
    expect(stored.tokens?.expiresAt).toBeDefined()
    expect(typeof stored.tokens?.expiresAt).toBe('number')
  })
})

describe('authService.refresh endpoint', () => {
  it('uses /api/auth/refresh (not /api/auth/refresh-token)', async () => {
    const { authService } = await import('./auth')
    const apiModule = await import('./api')
    const api = apiModule.default

    const postSpy = vi.spyOn(api, 'post').mockResolvedValueOnce({
      data: { accessToken: 'a', refreshToken: 'r' },
    })

    await authService.refresh({ refreshToken: 'rt' })

    expect(postSpy).toHaveBeenCalledWith('/api/auth/refresh', { refreshToken: 'rt' })
  })
})
