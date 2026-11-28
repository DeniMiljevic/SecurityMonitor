import { describe, expect, it } from 'vitest'
import { getIncidents, getStatus } from './api'
import { stubApi } from './test/fakeApi'

describe('getStatus', () => {
  it('returns the status from the API', async () => {
    stubApi({ '/api/status': { service: 'SecurityMonitor.Api', version: '1.0.0.0', monitor: null } })

    const status = await getStatus()

    expect(status.service).toBe('SecurityMonitor.Api')
    expect(status.monitor).toBeNull()
  })

  it('fails with the HTTP status when the API rejects the request', async () => {
    stubApi({ '/api/status': 401 })

    await expect(getStatus()).rejects.toThrow('/api/status returned 401')
  })
})

describe('getIncidents', () => {
  it('asks for fifty incidents by default', async () => {
    const fetchMock = stubApi({ '/api/incidents?take=50': [] })

    await getIncidents()

    expect(fetchMock).toHaveBeenCalledWith('/api/incidents?take=50')
  })
})
