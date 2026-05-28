import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateTicket } from './create-ticket'
import { useCreateTicket } from './use-create-ticket'

vi.mock('./use-create-ticket')

const navigate = vi.fn()

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => navigate,
}))

const mockUseCreateTicket = vi.mocked(useCreateTicket)

function asMutation(
  value: Partial<ReturnType<typeof useCreateTicket>>,
): ReturnType<typeof useCreateTicket> {
  return value as ReturnType<typeof useCreateTicket>
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('CreateTicket', () => {
  it('rejects empty input client-side', async () => {
    const mutateAsync = vi.fn()
    mockUseCreateTicket.mockReturnValue(
      asMutation({ mutateAsync, isPending: false, isError: false }),
    )

    render(<CreateTicket />)

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }))

    expect(screen.getByRole('alert')).toHaveTextContent(/required/i)
    expect(mutateAsync).not.toHaveBeenCalled()
  })

  it('shows processing state while pending', () => {
    mockUseCreateTicket.mockReturnValue(
      asMutation({ mutateAsync: vi.fn(), isPending: true, isError: false }),
    )

    render(<CreateTicket />)

    expect(screen.getByRole('button', { name: 'Creating...' })).toBeDisabled()
  })

  it('creates a ticket and navigates to detail', async () => {
    const mutateAsync = vi.fn().mockResolvedValue({
      id: '11111111-1111-1111-1111-111111111111',
    })
    mockUseCreateTicket.mockReturnValue(
      asMutation({ mutateAsync, isPending: false, isError: false }),
    )

    render(<CreateTicket />)

    await userEvent.type(screen.getByLabelText('User input'), 'Build me a thing')
    await userEvent.click(screen.getByRole('button', { name: 'Submit' }))

    expect(mutateAsync).toHaveBeenCalledWith({ userInput: 'Build me a thing' })
    expect(navigate).toHaveBeenCalledWith({
      to: '/tickets/$id',
      params: { id: '11111111-1111-1111-1111-111111111111' },
    })
  })
})
