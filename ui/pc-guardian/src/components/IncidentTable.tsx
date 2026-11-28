import { useQuery } from '@tanstack/react-query'
import { getIncidents, type IncidentSummary } from '../api'

const timeFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'medium' })

const cellClasses = 'px-4 py-2'

function PhotoCell({ incident }: { incident: IncidentSummary }) {
  if (incident.captureSucceeded) return <span className="text-emerald-600 dark:text-emerald-400">Saved</span>

  return (
    <span className="text-slate-500 dark:text-slate-400" title={incident.captureFailureReason ?? undefined}>
      No photo
    </span>
  )
}

function IncidentTable() {
  const { data, error, isPending } = useQuery({
    queryKey: ['incidents'],
    queryFn: () => getIncidents(),
    refetchInterval: 10_000,
  })

  if (isPending) return <p className="text-slate-500 dark:text-slate-400">Loading incidents...</p>

  if (error) return <p className="text-amber-600 dark:text-amber-400">Incidents cannot be loaded ({error.message}).</p>

  if (data.length === 0) return <p className="text-slate-500 dark:text-slate-400">No incidents yet.</p>

  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 dark:border-slate-800">
      <table className="w-full text-left text-sm">
        <thead className="bg-slate-100 text-slate-600 dark:bg-slate-900 dark:text-slate-400">
          <tr>
            <th className={`${cellClasses} font-medium`}>Time</th>
            <th className={`${cellClasses} font-medium`}>Method</th>
            <th className={`${cellClasses} font-medium`}>User</th>
            <th className={`${cellClasses} font-medium`}>Computer</th>
            <th className={`${cellClasses} font-medium`}>Attempts</th>
            <th className={`${cellClasses} font-medium`}>Photo</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
          {data.map((incident) => (
            <tr key={incident.id}>
              <td className={`${cellClasses} whitespace-nowrap`}>
                {timeFormat.format(new Date(incident.timestampUtc))}
              </td>
              <td className={cellClasses}>{incident.authenticationMethod}</td>
              <td className={cellClasses}>{incident.userName ?? '-'}</td>
              <td className={cellClasses}>{incident.computerName}</td>
              <td className={cellClasses}>{incident.attemptsInWindow}</td>
              <td className={cellClasses}>
                <PhotoCell incident={incident} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default IncidentTable
