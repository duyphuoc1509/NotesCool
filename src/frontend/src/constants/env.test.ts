import { describe, it, expect, vi } from 'vitest'

describe('API_BASE_URL', () => {
  it('defaults to same-origin requests when VITE_API_BASE_URL is not provided', async () => {
    vi.resetModules()
    vi.stubEnv('VITE_API_BASE_URL', undefined)

    const { API_BASE_URL } = await import('./env')

    expect(API_BASE_URL).toBe('')

    vi.unstubAllEnvs()
  })
})
