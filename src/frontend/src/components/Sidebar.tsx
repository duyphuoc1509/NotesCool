import { LayoutDashboard, FileText, CheckSquare, Settings, Users } from 'lucide-react'
import { cn } from '../utils/cn'

const navigation = [
  { name: 'Dashboard', icon: LayoutDashboard, href: '#' },
  { name: 'Notes', icon: FileText, href: '#' },
  { name: 'Tasks', icon: CheckSquare, href: '#' },
  { name: 'Users', icon: Users, href: '#' },
  { name: 'Settings', icon: Settings, href: '#' },
]

export function Sidebar() {
  return (
    <aside className="flex h-full w-64 flex-col border-r border-gray-200 bg-white">
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <span className="text-xl font-bold text-indigo-600">NotesCool</span>
      </div>
      <nav className="flex-1 space-y-1 px-3 py-4">
        {navigation.map((item) => (
          <a
            key={item.name}
            href={item.href}
            className={cn(
              'group flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors',
              item.name === 'Dashboard'
                ? 'bg-indigo-50 text-indigo-600'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
            )}
          >
            <item.icon
              className={cn(
                'mr-3 h-5 w-5 flex-shrink-0',
                item.name === 'Dashboard' ? 'text-indigo-600' : 'text-gray-400 group-hover:text-gray-500'
              )}
            />
            {item.name}
          </a>
        ))}
      </nav>
      <div className="border-t border-gray-200 p-4">
        <div className="flex items-center space-x-3">
          <div className="h-8 w-8 rounded-full bg-gray-200" />
          <div className="flex-1 overflow-hidden">
            <p className="truncate text-sm font-medium text-gray-900">Admin User</p>
            <p className="truncate text-xs text-gray-500">admin@notescool.com</p>
          </div>
        </div>
      </div>
    </aside>
  )
}
