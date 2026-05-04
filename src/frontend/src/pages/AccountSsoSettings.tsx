import { AlertTriangle, CheckCircle2, ExternalLink, Link2, LockKeyhole, ShieldCheck, Unlink } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { cn } from '../utils/cn'

type SsoProviderStatus = 'linked' | 'available'

type SsoProvider = {
  id: string
  name: string
  description: string
  status: SsoProviderStatus
  email?: string
  linkedAt?: string
  lastUsedAt?: string
  accentClassName: string
}

const initialProviders: SsoProvider[] = [
  {
    id: 'google',
    name: 'Google',
    description: 'Use your Google Workspace or Gmail account to sign in.',
    status: 'linked',
    email: 'admin@notescool.com',
    linkedAt: 'Jan 12, 2026',
    lastUsedAt: 'Today at 09:14',
    accentClassName: 'bg-red-50 text-red-600 ring-red-100',
  },
  {
    id: 'github',
    name: 'GitHub',
    description: 'Connect GitHub for developer-friendly single sign-on.',
    status: 'linked',
    email: 'admin@github.example',
    linkedAt: 'Feb 03, 2026',
    lastUsedAt: 'Mar 20, 2026',
    accentClassName: 'bg-slate-100 text-slate-700 ring-slate-200',
  },
  {
    id: 'microsoft',
    name: 'Microsoft',
    description: 'Sign in with a Microsoft 365 or Azure AD account.',
    status: 'available',
    accentClassName: 'bg-blue-50 text-blue-600 ring-blue-100',
  },
]

function getLinkedProviders(providers: SsoProvider[]) {
  return providers.filter((provider) => provider.status === 'linked')
}

export function AccountSsoSettings() {
  const { t } = useTranslation()
  const [providers, setProviders] = useState(initialProviders)
  const linkedProviders = useMemo(() => getLinkedProviders(providers), [providers])
  const hasPasswordLogin = false

  const handleLink = (providerId: string) => {
    setProviders((currentProviders) =>
      currentProviders.map((provider) =>
        provider.id === providerId
          ? {
              ...provider,
              status: 'linked',
              email: `admin@${provider.id}.example`,
              linkedAt: 'Just now',
              lastUsedAt: 'Not used yet',
            }
          : provider,
      ),
    )
  }

  const handleUnlink = (providerId: string) => {
    setProviders((currentProviders) =>
      currentProviders.map((provider) =>
        provider.id === providerId
          ? {
              ...provider,
              status: 'available',
              email: undefined,
              linkedAt: undefined,
              lastUsedAt: undefined,
            }
          : provider,
      ),
    )
  }

  return (
    <section className="mx-auto max-w-6xl space-y-6" aria-labelledby="sso-settings-title">
      <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">{t('settings.account')}</p>
            <h1 id="sso-settings-title" className="mt-2 text-3xl font-bold tracking-tight text-gray-950">
              {t('settings.ssoTitle')}
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-gray-600">{t('settings.ssoDesc')}</p>
          </div>
          <div className="rounded-xl bg-indigo-50 px-4 py-3 text-sm text-indigo-700 ring-1 ring-indigo-100">
            <div className="flex items-center gap-2 font-semibold">
              <ShieldCheck className="h-4 w-4" aria-hidden="true" />
              {t('settings.linkedProviders', { count: linkedProviders.length })}
            </div>
            <p className="mt-1 text-indigo-600">{t('settings.passwordDisabled')}</p>
          </div>
        </div>
      </div>

      <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
        <div className="flex gap-3">
          <AlertTriangle className="mt-0.5 h-5 w-5 flex-shrink-0" aria-hidden="true" />
          <div>
            <p className="font-semibold">{t('settings.unlinkConstraint')}</p>
            <p className="mt-1 text-amber-800">{t('settings.unlinkConstraintDesc')}</p>
          </div>
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        {providers.map((provider) => {
          const isLinked = provider.status === 'linked'
          const isLastLoginMethod = isLinked && linkedProviders.length === 1 && !hasPasswordLogin

          return (
            <article key={provider.id} className="flex flex-col rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <div className="flex items-start justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className={cn('flex h-11 w-11 items-center justify-center rounded-xl ring-1', provider.accentClassName)}>
                    <LockKeyhole className="h-5 w-5" aria-hidden="true" />
                  </div>
                  <div>
                    <h2 className="text-lg font-semibold text-gray-950">{provider.name}</h2>
                    <span className={cn('mt-1 inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium', isLinked ? 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-100' : 'bg-gray-100 text-gray-600')}>
                      {isLinked ? t('settings.linked') : t('settings.notLinked')}
                    </span>
                  </div>
                </div>
                {isLinked ? <CheckCircle2 className="h-5 w-5 text-emerald-500" aria-label={t('settings.linked')} /> : null}
              </div>

              <p className="mt-4 text-sm leading-6 text-gray-600">{provider.description}</p>

              <dl className="mt-5 space-y-3 border-t border-gray-100 pt-4 text-sm">
                <div>
                  <dt className="text-gray-500">{t('settings.accountLabel')}</dt>
                  <dd className="mt-1 font-medium text-gray-900">{provider.email ?? t('settings.noAccountConnected')}</dd>
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <dt className="text-gray-500">{t('settings.linked')}</dt>
                    <dd className="mt-1 text-gray-900">{provider.linkedAt ?? '—'}</dd>
                  </div>
                  <div>
                    <dt className="text-gray-500">{t('settings.lastUsed')}</dt>
                    <dd className="mt-1 text-gray-900">{provider.lastUsedAt ?? '—'}</dd>
                  </div>
                </div>
              </dl>

              {isLastLoginMethod ? (
                <p className="mt-4 rounded-lg bg-gray-50 px-3 py-2 text-xs font-medium text-gray-600 ring-1 ring-gray-200">
                  {t('settings.unlinkDisabled')}
                </p>
              ) : null}

              <div className="mt-auto pt-5">
                {isLinked ? (
                  <button
                    type="button"
                    onClick={() => handleUnlink(provider.id)}
                    disabled={isLastLoginMethod}
                    title={isLastLoginMethod ? t('settings.unlinkDisabled') : t('settings.unlinkProvider')}
                    aria-label={isLastLoginMethod ? t('settings.unlinkDisabled') : t('settings.unlinkProvider')}
                    className="inline-flex w-full items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-semibold text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:border-gray-200 disabled:bg-gray-100 disabled:text-gray-400"
                  >
                    <Unlink className="h-4 w-4" aria-hidden="true" />
                    {t('settings.unlinkProvider')}
                  </button>
                ) : (
                  <button
                    type="button"
                    onClick={() => handleLink(provider.id)}
                    className="inline-flex w-full items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                  >
                    <Link2 className="h-4 w-4" aria-hidden="true" />
                    {t('settings.linkProvider', { name: provider.name })}
                  </button>
                )}
              </div>
            </article>
          )
        })}
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
        <h2 className="text-base font-semibold text-gray-950">{t('settings.whatHappens')}</h2>
        <p className="mt-2 text-sm leading-6 text-gray-600">{t('settings.whatHappensDesc')}</p>
        <a href="#" className="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-indigo-600 hover:text-indigo-700">
          {t('settings.readSsoHelp')}
          <ExternalLink className="h-4 w-4" aria-hidden="true" />
        </a>
      </div>
    </section>
  )
}
