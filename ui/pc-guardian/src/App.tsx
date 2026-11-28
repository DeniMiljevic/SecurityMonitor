import StatusPanel from './components/StatusPanel'
import IncidentTable from './components/IncidentTable'

function App() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
      <header className="border-b border-slate-200 dark:border-slate-800">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
          <h1 className="text-lg font-semibold">PC Guardian</h1>
          <span className="text-sm text-slate-500 dark:text-slate-400">Local dashboard</span>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-8">
        <StatusPanel />

        <h2 className="mt-8 mb-3 text-base font-semibold">Recent incidents</h2>
        <IncidentTable />
      </main>
    </div>
  )
}

export default App
