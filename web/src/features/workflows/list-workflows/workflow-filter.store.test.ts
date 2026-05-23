import { beforeEach, describe, expect, it } from 'vitest'

import { matchesSearch, useWorkflowFilter } from './workflow-filter.store'

describe('matchesSearch', () => {
  it('matches everything when the search is blank', () => {
    expect(matchesSearch('anything', '   ')).toBe(true)
  })

  it('matches case-insensitively on substring', () => {
    expect(matchesSearch('Roof inspection', 'ROOF')).toBe(true)
  })

  it('rejects non-matching names', () => {
    expect(matchesSearch('Roof inspection', 'foundation')).toBe(false)
  })
})

describe('useWorkflowFilter', () => {
  beforeEach(() => {
    useWorkflowFilter.setState({ search: '' })
  })

  it('sets and clears the search term', () => {
    useWorkflowFilter.getState().setSearch('roof')
    expect(useWorkflowFilter.getState().search).toBe('roof')

    useWorkflowFilter.getState().clear()
    expect(useWorkflowFilter.getState().search).toBe('')
  })
})
