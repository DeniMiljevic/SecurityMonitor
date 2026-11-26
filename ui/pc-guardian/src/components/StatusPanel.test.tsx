import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { ApiStatus } from '../api'
import { stubApi } from '../test/fakeApi'
import { renderWithQueryClient } from '../test/renderWithQueryClient'
import StatusPanel from './StatusPanel'

const status: ApiStatus = {
  service: 'SecurityMonitor.Api',
  version: '1.0.0.0',
  startedAtUtc: '2026-11-26T08:00:00+00:00',
  monitor: { version: '1.0.0.0', startedAtUtc: '2026-11-26T08:00:00+00:00', incidentCount: 4 },
}

describe('StatusPanel', () => {
  it('shows that monitoring runs and how many incidents there are', async () => {
    stubApi({ '/api/status': status })
    renderWithQueryClient(<StatusPanel />)

    expect(await screen.findByText('Running')).toBeInTheDocument()
    expect(screen.getByText('4')).toBeInTheDocument()
  })

  it('warns when the monitoring service cannot be reched', async () => {
    stubApi({ '/api/status': { ...status, monitor: null } })
    renderWithQueryClient(<StatusPanel />)

    expect(await screen.findByText('Not reachable')).toBeInTheDocument()
  })

  it('tells when the local API cannot be reached', async () => {
    stubApi({ '/api/status': 502 })
    renderWithQueryClient(<StatusPanel />)

    expect(await screen.findByText(/The local API cannot be reached/)).toBeInTheDocument()
  })
})
