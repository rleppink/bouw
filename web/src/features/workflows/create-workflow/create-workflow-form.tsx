import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

import { type CreateWorkflowInput, createWorkflowInputSchema } from './create-workflow.contracts'
import { useCreateWorkflow } from './use-create-workflow'

export function CreateWorkflowForm() {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateWorkflowInput>({
    resolver: zodResolver(createWorkflowInputSchema),
    defaultValues: { name: '', status: 'draft' },
  })

  const mutation = useCreateWorkflow()

  const onSubmit = handleSubmit(async (values) => {
    await mutation.mutateAsync(values)
    reset()
  })

  return (
    <form
      noValidate
      className="space-y-4"
      onSubmit={(event) => {
        void onSubmit(event)
      }}
    >
      <div className="space-y-2">
        <Label htmlFor="name">Name</Label>
        <Input id="name" {...register('name')} aria-invalid={Boolean(errors.name)} />
        {errors.name ? (
          <p role="alert" className="text-destructive text-sm">
            {errors.name.message}
          </p>
        ) : null}
      </div>

      <div className="space-y-2">
        <Label htmlFor="status">Status</Label>
        <select
          id="status"
          {...register('status')}
          className="border-input flex h-9 w-full rounded-md border bg-transparent px-3 text-sm shadow-sm"
        >
          <option value="draft">Draft</option>
          <option value="active">Active</option>
        </select>
      </div>

      {mutation.isError ? (
        <p role="alert" className="text-destructive text-sm">
          Could not create the workflow.
        </p>
      ) : null}

      <Button type="submit" disabled={isSubmitting}>
        Create workflow
      </Button>
    </form>
  )
}
