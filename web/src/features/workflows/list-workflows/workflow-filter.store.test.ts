import { beforeEach, describe, expect, it } from 'vitest'

import { matchesSearch, useWorkflowFilter } from './workflow-filter.store'

describe('matchesSearch', () => {
  it('matches everything when the search is blank', () => {
    expect(matchesSearch('anything', '   ')).toBe(true)
  })

  it('matches case-insensitively on substring', () => {
    expect(matchesSearch('Refactor safely', 'REFACTOR')).toBe(true)
  })

  it('rejects non-matching names', () => {
    expect(matchesSearch('Refactor safely', 'frame')).toBe(false)
  })
})

describe('useWorkflowFilter', () => {
  beforeEach(() => {
    useWorkflowFilter.setState({ search: '' })
  })

  it('sets and clears the search term', () => {
    useWorkflowFilter.getState().setSearch('refactor')
    expect(useWorkflowFilter.getState().search).toBe('refactor')

    useWorkflowFilter.getState().clear()
    expect(useWorkflowFilter.getState().search).toBe('')
  })
})
