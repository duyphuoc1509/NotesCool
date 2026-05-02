import { beforeEach, describe, expect, it, vi } from 'vitest'
import { userManagementService } from './userManagementService'
import api from './api'

vi.mock('./api', () => ({
  default: {
    get: vi.fn(),
    put: vi.fn(),
  },
}))

describe('userManagementService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('lists users', async () => {
    const users = [
      { id: '1', email: 'a@a.com', displayName: 'A', status: 'Active', roles: ['User'] },
    ]
    vi.mocked(api.get).mockResolvedValue({ data: users })

    const result = await userManagementService.listUsers()
    expect(result).toEqual(users)
    expect(api.get).toHaveBeenCalledWith('/api/admin/users')
  })

  it('updates user status', async () => {
    const user = { id: '1', email: 'a@a.com', displayName: 'A', status: 'Suspended', roles: ['User'] }
    vi.mocked(api.put).mockResolvedValue({ data: user })

    const result = await userManagementService.updateUserStatus('1', { status: 'Suspended' })
    expect(result).toEqual(user)
    expect(api.put).toHaveBeenCalledWith('/api/admin/users/1/status', { status: 'Suspended' })
  })
})
