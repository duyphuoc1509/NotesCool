import { Navbar } from './components/Navbar'
import { Sidebar } from './components/Sidebar'
import { AccountSsoSettings } from './pages/AccountSsoSettings'

function App() {
  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto p-6 sm:p-8">
          <AccountSsoSettings />
        </main>
      </div>
    </div>
  )
}

export default App
