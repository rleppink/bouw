import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateWorkflowForm } from './create-workflow-form'
import { useCreateWorkflow } from './use-create-workflow'

vi.mock('./use-create-workflow')

const mockUseCreateWorkflow = vi.mocked(useCreateWorkflow)
const mutateAsync = vi.fn()

function asMutation(
  value: Partial<ReturnType<typeof useCreateWorkflow>>,
): ReturnType<typeof useCreateWorkflow> {
  return value as ReturnType<typeof useCreateWorkflow>
}

beforeEach(() => {
  vi.clearAllMocks()
  mutateAsync.mockResolvedValue({ id: '1', name: 'ok' })
  mockUseCreateWorkflow.mockReturnValue(asMutation({ mutateAsync, isError: false }))
})

describe('CreateWorkflowForm', () => {
  it('shows a validation error for a too-short name and does not submit', async () => {
    render(<CreateWorkflowForm />)
    await userEvent.type(screen.getByLabelText('Name'), 'ab')
    await userEvent.click(screen.getByRole('button', { name: /create workflow/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/at least 3 characters/i)
    expect(mutateAsync).not.toHaveBeenCalled()
  })

  it('submits a valid workflow', async () => {
    render(<CreateWorkflowForm />)
    await userEvent.type(screen.getByLabelText('Name'), 'Foundation pour')
    await userEvent.click(screen.getByRole('button', { name: /create workflow/i }))

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith({ name: 'Foundation pour', status: 'draft' })
    })
  })

  it('shows a submit error when the mutation fails', () => {
    mockUseCreateWorkflow.mockReturnValue(asMutation({ mutateAsync, isError: true }))
    render(<CreateWorkflowForm />)
    expect(screen.getByRole('alert')).toHaveTextContent(/could not create/i)
  })
})
