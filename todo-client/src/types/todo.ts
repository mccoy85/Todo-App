/** Priority levels for todos (enum-like const object) */
export const Priority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const;

export type Priority = (typeof Priority)[keyof typeof Priority];

/**
 * Status filter values for the todo list view.
 * - 'all': Show all non-deleted todos
 * - 'active': Show only incomplete todos
 * - 'completed': Show only completed todos
 * - 'deleted': Show soft-deleted todos (separate view)
 */
export type StatusFilter = 'all' | 'active' | 'completed' | 'deleted';

/** Priority options for dropdowns and select components */
export const PRIORITY_OPTIONS = [
  { label: 'Low', value: Priority.Low },
  { label: 'Medium', value: Priority.Medium },
  { label: 'High', value: Priority.High },
];

/** Sort field options for the todo list */
export const SORT_OPTIONS = [
  { label: 'Date Created', value: 'createdat' },
  { label: 'Title', value: 'title' },
  { label: 'Priority', value: 'priority' },
  { label: 'Due Date', value: 'duedate' },
];

export interface Todo {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  createdAt: string;
  dueDate?: string;
  priority: Priority;
}

export interface TodoListResponse {
  items: Todo[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateTodoRequest {
  title: string;
  description?: string;
  dueDate?: string;
  priority: Priority;
}

export interface UpdateTodoRequest {
  title: string;
  description?: string;
  isCompleted: boolean;
  dueDate?: string;
  priority: Priority;
}

export interface TodoQueryParams {
  isCompleted?: boolean;
  priority?: Priority;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}
