import { useMemo, useState } from 'react'
import { defaultAccountProviders, getLinkedProviders, linkProvider, unlinkProvider } from '../services/settingsService'

export function useAccountSsoSettings() {
  const [providers, setProviders] = useState(defaultAccountProviders)
  const linkedProviders = useMemo(() => getLinkedProviders(providers), [providers])
  const hasPasswordLogin = false

  return {
    providers,
    linkedProviders,
    hasPasswordLogin,
    linkProvider: (providerId: string) => setProviders((current) => linkProvider(current, providerId)),
    unlinkProvider: (providerId: string) => setProviders((current) => unlinkProvider(current, providerId)),
  }
}
