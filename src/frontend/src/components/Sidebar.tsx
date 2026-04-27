import { LogOut, LayoutDashboard, FileText, CheckSquare, Settings, Users } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { cn } from '../utils/cn'
import { useAuth } from '../hooks/useAuth'

const navigation = [
  { name: 'Dashboard', icon: LayoutDashboard, href: '/' },
  { name: 'Notes', icon: FileText, href: '/notes' },
  { name: 'Tasks', icon: CheckSquare, href: '/tasks' },
  { name: 'Users', icon: Users, href: '/users' },
  { name: 'Settings', icon: Settings, href: '/settings' },
]

export function Sidebar() {
  const location = useLocation()
  const { user, logout } = useAuth()

  return (
    <aside className="flex h-full w-64 flex-col border-r border-gray-200 bg-white">
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <span className="text-xl font-bold text-indigo-600">NotesCool</span>
      </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {navigation.map((item) => {
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
              {item.name}
            </Link>
          )
        })}
      </nav>
      <div className="border-t border-gray-200 p-4">
        <div className="flex items-center space-x-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-600 uppercase">
            {user?.fullName?.charAt(0) ?? user?.email?.charAt(0) ?? 'A'}
          </div>
          <div className="flex-1 overflow-hidden">
            <p className="truncate text-sm font-medium text-gray-900">
              {user?.fullName ?? 'Admin User'}
            </p>
            <p className="truncate text-xs text-gray-500">
              {user?.email ?? 'admin@notescool.com'}
            </p>
          </div>
          <button
            onClick={() => logout()}
            className="rounded-md p-1.5 text-gray-400 hover:bg-gray-50 hover:text-gray-500 transition"
            title="Sign out"
          >
            <LogOut className="h-5 w-5" />
          </button>
        </div>
      </div>
    </aside>
  )
}
