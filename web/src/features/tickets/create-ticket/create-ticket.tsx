import { useNavigate } from '@tanstack/react-router'
import type { SyntheticEvent } from 'react'
import { useId, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

import { useCreateTicket } from './use-create-ticket'

export function CreateTicket() {
  const textareaId = useId()
  const navigate = useNavigate()
  const mutation = useCreateTicket()
  const [userInput, setUserInput] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  async function handleSubmit(event: SyntheticEvent<HTMLFormElement>) {
    event.preventDefault()

    if (userInput.trim().length === 0) {
      setValidationError('User input is required.')
      return
    }

    setValidationError(null)
    const ticket = await mutation.mutateAsync({ userInput })
    await navigate({ to: '/tickets/$id', params: { id: ticket.id } })
  }

  return (
    <form
      className="space-y-4"
      onSubmit={(event) => {
        void handleSubmit(event)
      }}
    >
      <div className="space-y-2">
        <Label htmlFor={textareaId}>User input</Label>
        <Textarea
          id={textareaId}
          value={userInput}
          onChange={(event) => {
            setUserInput(event.target.value)
          }}
          aria-invalid={validationError ? true : undefined}
        />
        {validationError ? (
          <p role="alert" className="text-destructive text-sm">
            {validationError}
          </p>
        ) : null}
      </div>

      {mutation.isError ? (
        <p role="alert" className="text-destructive text-sm">
          Could not create ticket.
        </p>
      ) : null}

      <Button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? 'Creating...' : 'Submit'}
      </Button>
    </form>
  )
}
