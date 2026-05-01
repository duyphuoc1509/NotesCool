import { useAuth } from '../../../contexts/useAuth'

export function ProfileSettingsPage() {
  const { user } = useAuth()

  return (
    <section className="mx-auto max-w-4xl space-y-6" aria-labelledby="profile-settings-title">
      <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
        <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">Profile</p>
        <h1 id="profile-settings-title" className="mt-2 text-3xl font-bold tracking-tight text-gray-950">
          Personal information
        </h1>
        <p className="mt-3 text-sm leading-6 text-gray-600">
          Review the account information currently available in your workspace session.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-gray-500">Full name</p>
          <p className="mt-2 text-base font-semibold text-gray-950">{user?.fullName ?? user?.displayName ?? 'Not available'}</p>
        </div>
        <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
          <p className="text-sm text-gray-500">Email</p>
          <p className="mt-2 text-base font-semibold text-gray-950">{user?.email ?? 'Not available'}</p>
        </div>
      </div>
    </section>
  )
}
