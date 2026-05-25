import { fireEvent, render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { Board } from './board'

function createDataTransfer() {
  const data = new Map<string, string>()

  return {
    dropEffect: 'none',
    effectAllowed: 'all',
    getData: (type: string) => data.get(type) ?? '',
    setData: (type: string, value: string) => {
      data.set(type, value)
    },
  }
}

describe('Board', () => {
  it('moves a ticket to another lane when it is dropped there', () => {
    render(<Board />)

    const dataTransfer = createDataTransfer()
    const ticket = screen.getByText('Map permit intake fields')
    const readyLane = screen.getByRole('region', { name: 'Ready' })

    fireEvent.dragStart(ticket, { dataTransfer, clientX: 12, clientY: 20 })
    fireEvent.dragOver(readyLane, { dataTransfer, clientX: 240, clientY: 80 })
    fireEvent.drop(readyLane, { dataTransfer, clientX: 240, clientY: 80 })

    expect(within(readyLane).getByText('Map permit intake fields')).toBeInTheDocument()
  })
})
