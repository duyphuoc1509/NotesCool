import { LogOut, LayoutDashboard, FileText, CheckSquare, Settings, Users, X } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { cn } from '../utils/cn'
import { useAuth } from '../hooks/useAuth'
import { useTranslation } from 'react-i18next'
import { LanguageSwitcher } from './LanguageSwitcher'

const navigation = [
  { name: 'nav.dashboard', icon: LayoutDashboard, href: '/' },
  { name: 'nav.notes', icon: FileText, href: '/notes' },
  { name: 'nav.tasks', icon: CheckSquare, href: '/tasks' },
  { name: 'nav.settings', icon: Settings, href: '/settings' },
]

const adminNavigation = [{ name: 'nav.users', icon: Users, href: '/users' }]

interface SidebarProps {
  isOpen?: boolean
  onClose?: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const location = useLocation()
  const { user, logout, isAdmin } = useAuth()
  const { t } = useTranslation()

  const allNavigation = [...navigation, ...(isAdmin ? adminNavigation : [])]

  return (
    <>
      {/* Mobile Overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-slate-900/40 backdrop-blur-sm lg:hidden"
          onClick={onClose}
        />
      )}

      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 flex h-full w-64 flex-col border-r border-gray-200 bg-white transition-transform duration-300 ease-in-out lg:static lg:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        <div className="flex h-16 items-center justify-between border-b border-gray-200 px-6">
          <span className="text-xl font-bold text-indigo-600">NotesCool</span>
          <button
            onClick={onClose}
            className="rounded-md p-1.5 text-gray-400 hover:bg-gray-50 hover:text-gray-500 lg:hidden"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {allNavigation.map((item) => {
          const isActive = location.pathname === item.href
          return (
            <Link
              key={item.name}
              to={item.href}
              className={cn(
                'group flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-indigo-50 text-indigo-600'
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
              )}
            >
              <item.icon
                className={cn(
                  'mr-3 h-5 w-5 flex-shrink-0',
                  isActive ? 'text-indigo-600' : 'text-gray-400 group-hover:text-gray-500'
                )}
              />
              {t(item.name)}
            </Link>
          )
        })}
      </nav>
      <div className="border-t border-gray-200 p-4">
        <div className="mb-4 lg:hidden">
          <LanguageSwitcher />
        </div>
        <div className="flex items-center space-x-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-600 uppercase">
            {user?.fullName?.charAt(0) ?? user?.email?.charAt(0) ?? 'A'}
          </div>
          <div className="flex-1 overflow-hidden">
            <p className="truncate text-sm font-medium text-gray-900">
              {user?.fullName ?? t('user.adminUserFallback')}
            </p>
            <p className="truncate text-xs text-gray-500">
              {user?.email ?? t('user.adminEmailFallback')}
            </p>
          </div>
          <button
            onClick={() => logout()}
            className="rounded-md p-1.5 text-gray-400 hover:bg-gray-50 hover:text-gray-500 transition"
            title={t('nav.signOut')}
          >
            <LogOut className="h-5 w-5" />
          </button>
        </div>
      </div>
      </aside>
    </>
  )
}
