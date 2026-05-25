export type Lane = {
  id: string
  title: string
  limit: number
}

export type Ticket = {
  id: string
  laneId: string
  title: string
  owner: string
  estimate: number
  priority: 'Low' | 'Medium' | 'High'
}

export const lanes: Lane[] = [
  { id: 'backlog', title: 'Backlog', limit: 5 },
  { id: 'ready', title: 'Ready', limit: 4 },
  { id: 'progress', title: 'In progress', limit: 3 },
  { id: 'review', title: 'Review', limit: 3 },
  { id: 'done', title: 'Done', limit: 6 },
]

export const initialTickets: Ticket[] = [
  {
    id: 'B-142',
    laneId: 'backlog',
    title: 'Map permit intake fields',
    owner: 'Mira',
    estimate: 3,
    priority: 'Medium',
  },
  {
    id: 'B-151',
    laneId: 'backlog',
    title: 'Draft session handoff summary',
    owner: 'Noor',
    estimate: 2,
    priority: 'Low',
  },
  {
    id: 'B-155',
    laneId: 'ready',
    title: 'Add workflow status filters',
    owner: 'Ravi',
    estimate: 5,
    priority: 'High',
  },
  {
    id: 'B-160',
    laneId: 'progress',
    title: 'Persist document action runs',
    owner: 'Lena',
    estimate: 8,
    priority: 'High',
  },
  {
    id: 'B-163',
    laneId: 'progress',
    title: 'Tune empty state copy',
    owner: 'Mira',
    estimate: 1,
    priority: 'Low',
  },
  {
    id: 'B-168',
    laneId: 'review',
    title: 'Review workflow detail contract',
    owner: 'Noor',
    estimate: 3,
    priority: 'Medium',
  },
  {
    id: 'B-171',
    laneId: 'done',
    title: 'Ship API proxy setup',
    owner: 'Ravi',
    estimate: 2,
    priority: 'Medium',
  },
]
