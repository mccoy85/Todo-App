export const Priority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const;

export type Priority = (typeof Priority)[keyof typeof Priority];

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
