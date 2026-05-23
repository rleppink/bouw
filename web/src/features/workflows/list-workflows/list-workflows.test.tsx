import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ListWorkflows } from './list-workflows'
import { useWorkflows } from './use-workflows'
import type { WorkflowSummary } from './workflow.contracts'
import { useWorkflowFilter } from './workflow-filter.store'

vi.mock('./use-workflows')

const mockUseWorkflows = vi.mocked(useWorkflows)

function asResult(
  value: Partial<ReturnType<typeof useWorkflows>>,
): ReturnType<typeof useWorkflows> {
  return value as ReturnType<typeof useWorkflows>
}

const sample: WorkflowSummary[] = [
  { id: '1', name: 'Foundation pour', status: 'active', createdAt: '2026-01-02T00:00:00.000Z' },
  { id: '2', name: 'Roof inspection', status: 'draft', createdAt: '2026-02-10T00:00:00.000Z' },
]

beforeEach(() => {
  useWorkflowFilter.setState({ search: '' })
  vi.clearAllMocks()
})

describe('ListWorkflows', () => {
  it('shows a loading state while pending', () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: true, isError: false }))
    render(<ListWorkflows />)
    expect(screen.getByRole('status')).toHaveTextContent(/loading/i)
  })

  it('shows an error state on failure', () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: false, isError: true }))
    render(<ListWorkflows />)
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })

  it('renders the workflows on success', () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: false, isError: false, data: sample }))
    render(<ListWorkflows />)
    expect(screen.getByText('Foundation pour')).toBeInTheDocument()
    expect(screen.getByText('Roof inspection')).toBeInTheDocument()
  })

  it('shows an empty message when the filter matches nothing', async () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: false, isError: false, data: sample }))
    render(<ListWorkflows />)
    await userEvent.type(screen.getByLabelText('Filter workflows'), 'nonexistent')
    expect(screen.getByText(/no workflows match/i)).toBeInTheDocument()
  })

  it('filters down to matching workflows', async () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: false, isError: false, data: sample }))
    render(<ListWorkflows />)
    await userEvent.type(screen.getByLabelText('Filter workflows'), 'roof')
    expect(screen.getByText('Roof inspection')).toBeInTheDocument()
    expect(screen.queryByText('Foundation pour')).not.toBeInTheDocument()
  })
})
