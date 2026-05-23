import { format, isValid, parseISO } from 'date-fns'

/**
 * Format an ISO timestamp for display. Returns a placeholder for missing
 * values and a marker for unparseable ones, so a bad value from the API never
 * renders as `Invalid Date`.
 */
export function formatWorkflowDate(iso: string | null | undefined): string {
  if (!iso) {
    return '—'
  }

  const parsed = parseISO(iso)
  if (!isValid(parsed)) {
    return 'Unknown date'
  }

  return format(parsed, 'd MMM yyyy')
}
