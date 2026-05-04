import { Home, FileText, CheckSquare, Settings, Plus, Users } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { cn } from '../utils/cn'
import { useAuth } from '../hooks/useAuth'

const navigation = [
  { name: 'Home', icon: Home, href: '/' },
  { name: 'Notes', icon: FileText, href: '/notes' },
  { name: 'Tasks', icon: CheckSquare, href: '/tasks' },
  { name: 'Settings', icon: Settings, href: '/settings' },
]

const adminNavigation = [{ name: 'Users', icon: Users, href: '/users' }]

interface BottomNavProps {
  onQuickCreate: () => void
}

export function BottomNav({ onQuickCreate }: BottomNavProps) {
  const location = useLocation()
  const { isAdmin } = useAuth()

  const allNavigation = [...navigation, ...(isAdmin ? adminNavigation : [])]

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-40 border-t border-gray-200 bg-white/80 backdrop-blur-md lg:hidden">
      <div className="flex items-center justify-around px-2 py-1">
        {allNavigation.map((item) => {
          const isActive = location.pathname === item.href
          return (
            <Link
              key={item.name}
              to={item.href}
              className={cn(
                'flex flex-col items-center justify-center rounded-xl px-3 py-2 transition-colors',
                isActive ? 'text-indigo-600' : 'text-gray-500 hover:text-gray-900'
              )}
            >
              <item.icon className={cn('h-5 w-5', isActive ? 'text-indigo-600' : 'text-gray-400')} />
              <span className="mt-1 text-[10px] font-medium">{item.name}</span>
            </Link>
          )
        })}
        <button
          onClick={onQuickCreate}
          className="flex flex-col items-center justify-center rounded-xl px-3 py-2 text-indigo-600 transition-colors hover:text-indigo-700"
          aria-label="Quick create"
        >
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-indigo-600 text-white shadow-lg shadow-indigo-200 transition-transform active:scale-95">
            <Plus className="h-6 w-6" />
          </div>
        </button>
      </div>
    </nav>
  )
}
