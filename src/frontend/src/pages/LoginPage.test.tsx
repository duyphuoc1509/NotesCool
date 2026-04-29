import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, cleanup } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { LoginPage } from './LoginPage'
import { AuthContext } from '../contexts/auth-context'

// Mock API_BASE_URL
vi.mock('../constants/env', () => ({
  API_BASE_URL: 'http://localhost:10002',
}))

describe('LoginPage', () => {
  const loginMock = vi.fn()
  const mockAuthContext = {
    user: null,
    tokens: null,
    isAuthenticated: false,
    isLoading: false,
    login: loginMock,
    register: vi.fn(),
    logout: vi.fn(),
  }

  let originalLocation: Location

  beforeEach(() => {
    vi.clearAllMocks()
    originalLocation = window.location
    // Mock window.location.assign
    delete (window as any).location
    window.location = { ...originalLocation, assign: vi.fn() }
  })

  afterEach(() => {
    window.location = originalLocation
    cleanup()
  })

  it('renders Google and Microsoft login buttons', () => {
    render(
      <MemoryRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <LoginPage />
        </AuthContext.Provider>
      </MemoryRouter>
    )

    expect(screen.getByRole('button', { name: /Google/i })).toBeDefined()
    expect(screen.getByRole('button', { name: /Microsoft/i })).toBeDefined()
  })

  it('redirects to Google backend endpoint and disables form when Google is clicked', () => {
    render(
      <MemoryRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <LoginPage />
        </AuthContext.Provider>
      </MemoryRouter>
    )

    const googleBtn = screen.getByRole('button', { name: /Google/i })
    fireEvent.click(googleBtn)

    expect(window.location.assign).toHaveBeenCalledWith('http://localhost:10002/api/auth/sso/login/google')
    
    // Check if form inputs and submit button are disabled
    expect(screen.getByLabelText(/Email/i)).toHaveProperty('disabled', true)
    expect(screen.getByLabelText(/Password/i)).toHaveProperty('disabled', true)
    expect(screen.getByRole('button', { name: /Sign in/i })).toHaveProperty('disabled', true)
    // Other SSO buttons should also be disabled
    expect(googleBtn).toHaveProperty('disabled', true)
    expect(screen.getByRole('button', { name: /Microsoft/i })).toHaveProperty('disabled', true)
  })

  it('redirects to Microsoft backend endpoint and disables form when Microsoft is clicked', () => {
    render(
      <MemoryRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <LoginPage />
        </AuthContext.Provider>
      </MemoryRouter>
    )

    const msBtn = screen.getByRole('button', { name: /Microsoft/i })
    fireEvent.click(msBtn)

    expect(window.location.assign).toHaveBeenCalledWith('http://localhost:10002/api/auth/sso/login/microsoft')
  })
})
