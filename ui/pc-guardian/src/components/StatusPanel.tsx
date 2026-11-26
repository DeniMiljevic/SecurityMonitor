import { useQuery } from '@tanstack/react-query'
import { getStatus } from '../api'

const toneClasses = {
  good: 'text-emerald-600 dark:text-emerald-400',
  warning: 'text-amber-600 dark:text-amber-400',
}

type StatusCardProps = {
  label: string
  value: string
  tone?: keyof typeof toneClasses
}

function StatusCard({ label, value, tone }: StatusCardProps) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-900">
      <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
      <p className={`mt-1 text-2xl font-semibold ${tone ? toneClasses[tone] : ''}`}>{value}</p>
    </div>
  )
}

function StatusPanel() {
  const { data, error, isPending } = useQuery({
    queryKey: ['status'],
    queryFn: getStatus,
    refetchInterval: 10_000,
  })

  if (isPending)
    return <p className="text-slate-500 dark:text-slate-400">Loading status...</p>

  if (error)
    return <p className="text-amber-600 dark:text-amber-400">The local API cannot be reached ({error.message}).</p>

  return (
    <section className="grid gap-4 sm:grid-cols-3">
      <StatusCard label="Monitoring" value={data.monitor ? 'Running' : 'Not reachable'} tone={data.monitor ? 'good' : 'warning'} />
      <StatusCard label="Incidents" value={data.monitor ? String(data.monitor.incidentCount) : '-'} />
      <StatusCard label="API" value={`v${data.version}`} tone="good" />
    </section>
  )
}

export default StatusPanel