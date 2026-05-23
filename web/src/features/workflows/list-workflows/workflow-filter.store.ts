import { create } from 'zustand'

interface WorkflowFilterState {
  search: string
  setSearch: (search: string) => void
  clear: () => void
}

/** Slice-local client state: the list's text filter. Lives in the slice so the
 *  no-slice-imports-another-slice rule fences it in. */
export const useWorkflowFilter = create<WorkflowFilterState>((set) => ({
  search: '',
  setSearch: (search) => {
    set({ search })
  },
  clear: () => {
    set({ search: '' })
  },
}))

/** Pure predicate behind the filter, exported for direct unit testing. */
export function matchesSearch(name: string, search: string): boolean {
  const trimmed = search.trim().toLowerCase()
  if (trimmed.length === 0) {
    return true
  }
  return name.toLowerCase().includes(trimmed)
}
