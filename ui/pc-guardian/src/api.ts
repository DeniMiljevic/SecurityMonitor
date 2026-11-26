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

async function getJson<T>(path: string): Promise<T> {
    const response = await fetch(path)

    if (!response.ok)
        throw new Error(`${path} returned ${response.status}`)

    return (await response.json()) as T
}

export const getStatus = () => getJson<ApiStatus>('/api/status')