import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { WorkflowResponse } from '@/lib/api.generated'

import { useWorkflow } from './use-workflow'
import { ViewWorkflow } from './view-workflow'

vi.mock('./use-workflow')

const mockUseWorkflow = vi.mocked(useWorkflow)

function asResult(value: Partial<ReturnType<typeof useWorkflow>>): ReturnType<typeof useWorkflow> {
  return value as ReturnType<typeof useWorkflow>
}

const workflow: WorkflowResponse = {
  id: '1',
  key: 'frame',
  name: 'Frame',
  description: 'Frame the work',
  status: 'active',
  steps: [
    {
      id: '2',
      key: 'interview',
      name: 'Interview',
      position: 10,
      actions: [
        {
          id: '3',
          key: 'ask-context',
          type: 'ask_user_input',
          position: 20,
          configJson: '{"prompt":"What are we building?"}',
        },
      ],
    },
  ],
}

describe('ViewWorkflow', () => {
  it('shows a loading state while pending', () => {
    mockUseWorkflow.mockReturnValue(asResult({ isPending: true, isError: false }))

    render(<ViewWorkflow workflowKey="frame" />)

    expect(screen.getByRole('status')).toHaveTextContent(/loading workflow/i)
  })

  it('shows an error state on failure', () => {
    mockUseWorkflow.mockReturnValue(asResult({ isPending: false, isError: true }))

    render(<ViewWorkflow workflowKey="frame" />)

    expect(screen.getByRole('alert')).toHaveTextContent(/could not load/i)
  })

  it('shows a not found state when the API returns no workflow', () => {
    mockUseWorkflow.mockReturnValue(asResult({ isPending: false, isError: false, data: null }))

    render(<ViewWorkflow workflowKey="missing" />)

    expect(screen.getByRole('alert')).toHaveTextContent(/not found/i)
  })

  it('renders workflow, step, and action values', () => {
    mockUseWorkflow.mockReturnValue(asResult({ isPending: false, isError: false, data: workflow }))

    render(<ViewWorkflow workflowKey="frame" />)

    expect(screen.getByRole('heading', { name: 'Frame' })).toBeInTheDocument()
    expect(screen.getByText(/frame .* active/)).toBeInTheDocument()
    expect(screen.getByText('Frame the work')).toBeInTheDocument()
    expect(screen.getByText('10. Interview')).toBeInTheDocument()
    expect(screen.getByText('interview')).toBeInTheDocument()
    expect(screen.getByText('20. ask-context')).toBeInTheDocument()
    expect(screen.getByText('ask_user_input')).toBeInTheDocument()
    expect(screen.getByText('{"prompt":"What are we building?"}')).toBeInTheDocument()
  })
})
