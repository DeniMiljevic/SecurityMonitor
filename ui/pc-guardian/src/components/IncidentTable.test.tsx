import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { IncidentSummary } from '../api'
import { stubApi } from '../test/fakeApi'
import { renderWithQueryClient } from '../test/renderWithQueryClient'
import IncidentTable from './IncidentTable'

const incidentsPath = '/api/incidents?take=50'

const incident: IncidentSummary = {
  id: 1,
  timestampUtc: '2026-11-24T20:43:35+00:00',
  authenticationMethod: 'Pin',
  userName: null,
  computerName: 'PC',
  attemptsInWindow: 1,
  captureSucceeded: false,
  captureFailureReason: 'Camera is not available to a Windows Service (Session 0)',
}

describe('IncidentTable', () => {
  it('shows one row for every incident', async () => {
    stubApi({ [incidentsPath]: [incident, { ...incident, id: 2, captureSucceeded: true, captureFailureReason: null }] })
    renderWithQueryClient(<IncidentTable />)

    // The header row plus one row per incident.
    expect(await screen.findAllByRole('row')).toHaveLength(3)
  })

  it('explains why there is no photo', async () => {
    stubApi({ [incidentsPath]: [incident] })
    renderWithQueryClient(<IncidentTable />)

    expect(await screen.findByText('No photo')).toHaveAttribute('title', incident.captureFailureReason)
  })

  it('shows a dash when the user name is unknown', async () => {
    stubApi({ [incidentsPath]: [incident] })
    renderWithQueryClient(<IncidentTable />)

    expect(await screen.findByText('-')).toBeInTheDocument()
  })

  it('says so when there are no incidents yet', async () => {
    stubApi({ [incidentsPath]: [] })
    renderWithQueryClient(<IncidentTable />)

    expect(await screen.findByText('No incidents yet.')).toBeInTheDocument()
  })

  it('tells when incidents cannot be loaded', async () => {
    stubApi({ [incidentsPath]: 503 })
    renderWithQueryClient(<IncidentTable />)

    expect(await screen.findByText(/Incidents cannot be loaded/)).toBeInTheDocument()
  })
})
