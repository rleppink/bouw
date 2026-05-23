import { describe, expect, it } from 'vitest'

import { formatWorkflowDate } from './format-workflow-date'

describe('formatWorkflowDate', () => {
  it('returns a placeholder for missing values', () => {
    expect(formatWorkflowDate(null)).toBe('—')
    expect(formatWorkflowDate(undefined)).toBe('—')
    expect(formatWorkflowDate('')).toBe('—')
  })

  it('returns a marker for unparseable values', () => {
    expect(formatWorkflowDate('not-a-date')).toBe('Unknown date')
  })

  it('formats a valid ISO timestamp', () => {
    expect(formatWorkflowDate('2026-01-02T00:00:00.000Z')).toBe('2 Jan 2026')
  })
})
