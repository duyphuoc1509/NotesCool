import { Sidebar } from './components/Sidebar'
import { Navbar } from './components/Navbar'

function App() {
  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto p-8">
          <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">
              NotesCool CMS
            </p>
            <h1 className="mt-3 text-3xl font-bold tracking-tight text-gray-950 sm:text-4xl">
              Frontend codebase is ready
            </h1>
            <p className="mt-4 max-w-2xl text-base text-gray-600">
              This React + TypeScript + Vite application is prepared as the frontend
              foundation for the NotesCool CMS.
            </p>

            <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {[
                'components',
                'pages',
                'hooks',
                'services',
                'store',
                'utils',
                'styles',
                'constants',
              ].map((folder) => (
                <div
                  key={folder}
                  className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 font-mono text-sm text-gray-700"
                >
                  src/{folder}
                </div>
              ))}
            </div>
          </div>
        </main>
      </div>
    </div>
  )
}

export default App
