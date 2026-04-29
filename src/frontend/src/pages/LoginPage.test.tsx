import { act } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { LoginPage } from './LoginPage'
import { AuthProvider } from '../contexts/AuthContext'

// @ts-expect-error IS_REACT_ACT_ENVIRONMENT is used by React internals for act() support
globalThis.IS_REACT_ACT_ENVIRONMENT = true

describe('LoginPage', () => {
  let container: HTMLDivElement
  let root: Root

  beforeEach(() => {
    container = document.createElement('div')
    document.body.appendChild(container)
    root = createRoot(container)
    vi.clearAllMocks()
  })

  afterEach(() => {
    act(() => root.unmount())
    container.remove()
  })

  it('displays SSO error when error=sso_failed in query params', async () => {
    await act(async () => {
      root.render(
        <MemoryRouter initialEntries={['/login?error=sso_failed']}>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      )
    })

    const errorDiv = container.querySelector('.bg-red-50')
    expect(errorDiv).not.toBeNull()
    expect(errorDiv?.textContent).toContain('SSO sign-in failed. Please try again.')
  })
})
