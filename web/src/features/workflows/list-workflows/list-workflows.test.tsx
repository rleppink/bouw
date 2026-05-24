import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ListWorkflows } from './list-workflows'
import { useWorkflows } from './use-workflows'
import type { WorkflowSummary } from './workflow.contracts'
import { useWorkflowFilter } from './workflow-filter.store'

vi.mock('./use-workflows')
vi.mock('@tanstack/react-router', () => ({
  Link: ({
    children,
    className,
    params,
    to,
  }: {
    children: ReactNode
    className?: string
    params: { id: string }
    to: string
  }) => (
    <a className={className} href={to.replace('$id', params.id)}>
      {children}
    </a>
  ),
}))

const mockUseWorkflows = vi.mocked(useWorkflows)

function asResult(
  value: Partial<ReturnType<typeof useWorkflows>>,
): ReturnType<typeof useWorkflows> {
  return value as ReturnType<typeof useWorkflows>
}

const sample: WorkflowSummary[] = [
  {
    id: '1',
    name: 'Frame',
    description: 'Frame the feature before implementation.',
    status: 'active',
    steps: [{ id: 'step-1' }],
  },
  {
    id: '2',
    name: 'Refactor safely',
    description: 'Preserve behavior while changing structure.',
    status: 'archived',
    steps: [],
  },
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
    expect(screen.getByText('Frame')).toBeInTheDocument()
    expect(screen.getByText('Frame the feature before implementation.')).toBeInTheDocument()
    expect(screen.getByText('active · 1 step')).toBeInTheDocument()
    expect(screen.getByText('Refactor safely')).toBeInTheDocument()
    expect(screen.getByText('archived · 0 steps')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /frame/i })).toHaveAttribute('href', '/workflows/1')
  })

  it('shows an empty message when there are no workflows', () => {
    mockUseWorkflows.mockReturnValue(asResult({ isPending: false, isError: false, data: [] }))
    render(<ListWorkflows />)
    expect(screen.getByText(/no workflows match/i)).toBeInTheDocument()
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
    await userEvent.type(screen.getByLabelText('Filter workflows'), 'refactor')
    expect(screen.getByText('Refactor safely')).toBeInTheDocument()
    expect(screen.queryByText('Frame')).not.toBeInTheDocument()
  })
})
