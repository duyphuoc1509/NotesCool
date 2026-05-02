import { useCallback, useEffect, useMemo, useState } from 'react'
import { AlertCircle, Loader2, ShieldCheck, ShieldX, Users } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import {
  userManagementService,
  type ManagedUser,
  type UserStatus,
} from '../services/userManagementService'

const STATUS_STYLES: Record<string, string> = {
  Active: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Suspended: 'bg-rose-50 text-rose-700 ring-rose-200',
}

export function AdminUsersPage() {
  const { user } = useAuth()
  const [users, setUsers] = useState<ManagedUser[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [savingUserId, setSavingUserId] = useState<string | null>(null)

  const loadUsers = useCallback(async () => {
    setLoading(true)

    try {
      const data = await userManagementService.listUsers()
      setUsers(data)
      setError(null)
    } catch {
      setError('Unable to load users. Please try again.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    let cancelled = false

    userManagementService
      .listUsers()
      .then((data) => {
        if (!cancelled) {
          setUsers(data)
          setError(null)
        }
      })
      .catch(() => {
        if (!cancelled) {
          setError('Unable to load users. Please try again.')
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false)
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  const totals = useMemo(() => {
    const active = users.filter((entry) => entry.status === 'Active').length
    const suspended = users.filter((entry) => entry.status === 'Suspended').length
    return { total: users.length, active, suspended }
  }, [users])

  const handleStatusChange = useCallback(async (managedUser: ManagedUser, status: UserStatus) => {
    if (managedUser.status === status) {
      return
    }

    setSavingUserId(managedUser.id)
    setError(null)

    try {
      const updatedUser = await userManagementService.updateUserStatus(managedUser.id, { status })
      setUsers((current) => current.map((entry) => (entry.id === updatedUser.id ? updatedUser : entry)))
    } catch {
      setError('Unable to update user status. Please try again.')
    } finally {
      setSavingUserId(null)
    }
  }, [])

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <div className="inline-flex items-center gap-2 rounded-full bg-indigo-50 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-indigo-700">
              <ShieldCheck className="h-3.5 w-3.5" />
              Admin only
            </div>
            <h1 className="mt-3 text-2xl font-bold tracking-tight text-gray-950 sm:text-3xl">User management</h1>
            <p className="mt-2 max-w-2xl text-sm text-gray-600">
              Review workspace accounts. Suspend access for users when needed.
            </p>
          </div>

          <div className="rounded-2xl border border-gray-100 bg-gray-50 px-4 py-3 text-sm text-gray-600">
            <p className="font-semibold text-gray-900">Signed in as</p>
            <p>{user?.displayName ?? user?.fullName ?? user?.email}</p>
          </div>
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-3">
        <StatCard label="Total users" value={totals.total} accent="text-indigo-600" />
        <StatCard label="Active" value={totals.active} accent="text-emerald-600" />
        <StatCard label="Suspended" value={totals.suspended} accent="text-rose-600" />
      </section>

      {error ? (
        <div className="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
          <div className="flex-1">
            <p className="font-semibold">Action failed</p>
            <p>{error}</p>
          </div>
          <button
            type="button"
            onClick={() => setError(null)}
            className="text-rose-400 transition hover:text-rose-600"
          >
            ✕
          </button>
        </div>
      ) : null}

      <section className="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Accounts</h2>
            <p className="text-sm text-gray-500">Manage admin and member access.</p>
          </div>
          <button
            type="button"
            onClick={() => void loadUsers()}
            className="rounded-xl border border-gray-200 px-3 py-2 text-sm font-medium text-gray-600 transition hover:border-indigo-200 hover:bg-indigo-50 hover:text-indigo-600"
          >
            Refresh
          </button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center gap-3 px-6 py-16 text-sm text-gray-500">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading users...
          </div>
        ) : users.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-3 px-6 py-16 text-center text-gray-500">
            <Users className="h-10 w-10 text-gray-300" />
            <div>
              <p className="font-medium text-gray-900">No users found</p>
              <p className="text-sm">User accounts will appear here after signup.</p>
            </div>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <HeaderCell>User</HeaderCell>
                  <HeaderCell>Roles</HeaderCell>
                  <HeaderCell>Status</HeaderCell>
                  <HeaderCell align="right">Actions</HeaderCell>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 bg-white">
                {users.map((managedUser) => {
                  const isSaving = savingUserId === managedUser.id
                  const nextStatus: UserStatus = managedUser.status === 'Active' ? 'Suspended' : 'Active'
                  const actionLabel = nextStatus === 'Suspended' ? 'Block' : 'Unblock'

                  return (
                    <tr key={managedUser.id} className="align-top">
                      <td className="px-6 py-4">
                        <div className="flex flex-col">
                          <span className="font-medium text-gray-900">{managedUser.displayName}</span>
                          <span className="text-sm text-gray-500">{managedUser.email}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-wrap gap-2">
                          {managedUser.roles.map((role) => (
                            <span
                              key={`${managedUser.id}-${role}`}
                              className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700"
                            >
                              {role}
                            </span>
                          ))}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset ${STATUS_STYLES[managedUser.status] ?? 'bg-gray-100 text-gray-700 ring-gray-200'}`}
                        >
                          {managedUser.status}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex justify-end">
                          <button
                            type="button"
                            onClick={() => void handleStatusChange(managedUser, nextStatus)}
                            disabled={isSaving}
                            className={`inline-flex min-w-24 items-center justify-center gap-2 rounded-xl px-3 py-2 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-70 ${
                              nextStatus === 'Suspended'
                                ? 'bg-rose-600 text-white hover:bg-rose-500'
                                : 'bg-emerald-600 text-white hover:bg-emerald-500'
                            }`}
                          >
                            {isSaving ? (
                              <Loader2 className="h-4 w-4 animate-spin" />
                            ) : nextStatus === 'Suspended' ? (
                              <ShieldX className="h-4 w-4" />
                            ) : (
                              <ShieldCheck className="h-4 w-4" />
                            )}
                            {actionLabel}
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function StatCard({ label, value, accent }: { label: string; value: number; accent: string }) {
  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
      <p className="text-sm text-gray-500">{label}</p>
      <p className={`mt-2 text-3xl font-bold tracking-tight ${accent}`}>{value}</p>
    </div>
  )
}

function HeaderCell({
  children,
  align = 'left',
}: {
  children: React.ReactNode
  align?: 'left' | 'right'
}) {
  return (
    <th
      scope="col"
      className={`px-6 py-3 text-xs font-semibold uppercase tracking-wide text-gray-500 ${
        align === 'right' ? 'text-right' : 'text-left'
      }`}
    >
      {children}
    </th>
  )
}
