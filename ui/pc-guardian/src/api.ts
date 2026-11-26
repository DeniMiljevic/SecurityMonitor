export type MonitorStatus = {
  version: string
  startedAtUtc: string
  incidentCount: number
}

export type ApiStatus = {
  service: string
  version: string
  startedAtUtc: string
  monitor: MonitorStatus | null
}

export type IncidentSummary = {
  id: number
  timestampUtc: string
  authenticationMethod: string
  userName: string | null
  computerName: string
  attemptsInWindow: number
  captureSucceeded: boolean
  captureFailureReason: string | null
}

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(path)

  if (!response.ok) throw new Error(`${path} returned ${response.status}`)

  return (await response.json()) as T
}

export const getStatus = () => getJson<ApiStatus>('/api/status')

export const getIncidents = (take = 50) => getJson<IncidentSummary[]>(`/api/incidents?take=${take}`)
