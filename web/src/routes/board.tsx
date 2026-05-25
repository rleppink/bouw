import { createFileRoute } from '@tanstack/react-router'

import { Board } from '@/features/board/board'

export const Route = createFileRoute('/board')({
  component: Board,
})
