import { Bell, LogOut, Menu, Search } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import { LanguageSwitcher } from './LanguageSwitcher'
import { useTranslation } from 'react-i18next'

interface NavbarProps {
  onMenuClick: () => void
}

export function Navbar({ onMenuClick }: NavbarProps) {
  const { logout } = useAuth()
  const { t } = useTranslation()

  return (
    <header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 md:px-8">
      <div className="flex flex-1 items-center gap-3 md:gap-4">
        <button
          onClick={onMenuClick}
          className="rounded-md p-1.5 text-gray-500 hover:bg-gray-50 hover:text-gray-600 lg:hidden"
        >
          <Menu className="h-6 w-6" />
        </button>

        <div className="relative flex-1 md:w-96 md:flex-none">
          <span className="absolute inset-y-0 left-0 flex items-center pl-3">
            <Search className="h-4 w-4 md:h-5 md:w-5 text-gray-400" />
          </span>
          <input
            type="text"
            className="block w-full rounded-md border-0 py-1.5 pl-9 md:pl-10 pr-3 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 text-xs md:text-sm sm:leading-6"
            placeholder={t('nav.searchPlaceholder')}
          />
        </div>
      </div>
      <div className="flex items-center space-x-2 md:space-x-4">
        <div className="hidden sm:block">
          <LanguageSwitcher />
        </div>
        <button className="hidden rounded-full p-1 text-gray-400 hover:text-gray-500 sm:block" type="button">
          <Bell className="h-6 w-6" />
        </button>
        <button
          className="inline-flex items-center gap-1 md:gap-2 rounded-full border border-gray-200 px-3 py-1.5 md:px-4 md:py-2 text-xs md:text-sm font-medium text-gray-600 transition hover:border-rose-200 hover:bg-rose-50 hover:text-rose-600"
          onClick={logout}
          type="button"
        >
          <LogOut className="h-3.5 w-3.5 md:h-4 md:w-4" />
          <span className="hidden xs:inline">{t('nav.logout')}</span>
        </button>
      </div>
    </header>
  )
}
